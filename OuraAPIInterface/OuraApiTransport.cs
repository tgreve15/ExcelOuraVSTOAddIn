using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace OuraAPIInterface
{
    public sealed class OuraApiException : OuraException
    {
        public HttpStatusCode StatusCode { get; }

        public OuraApiException(HttpStatusCode status, string message) : base(message)
        {
            StatusCode = status;
        }
    }

    public sealed class OuraApiTransport
    {
        private readonly HttpClient http;
        private readonly Func<bool, string, CancellationToken, Task<string>> tokenProvider;
        private readonly Func<TimeSpan, CancellationToken, Task> delay;
        private readonly Func<DateTimeOffset> utcNow;

        public OuraApiTransport(HttpClient http, Func<bool, string> tokenProvider,
            Func<TimeSpan, CancellationToken, Task> delay = null)
            : this(http, Adapt(tokenProvider), delay) { }

        public OuraApiTransport(HttpClient http, Func<bool, string, CancellationToken, Task<string>> tokenProvider,
            Func<TimeSpan, CancellationToken, Task> delay = null, Func<DateTimeOffset> utcNow = null)
        {
            this.http = http ?? throw new ArgumentNullException(nameof(http));
            this.tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            this.delay = delay ?? ((duration, cancellation) => Task.Delay(duration, cancellation));
            this.utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        }

        private static Func<bool, string, CancellationToken, Task<string>> Adapt(Func<bool, string> provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            return (force, rejected, cancellation) => Task.FromResult(provider(force));
        }

        public async Task<string> GetAsync(string url, CancellationToken cancellation)
        {
            Uri uri = ValidateUrl(url);
            cancellation.ThrowIfCancellationRequested();
            string token = await tokenProvider(false, null, cancellation).ConfigureAwait(false);
            bool refreshed = false;
            int retries = 0;

            while (true)
            {
                cancellation.ThrowIfCancellationRequested();
                OuraArguments.ValidateAccessToken(token);
                using (var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation))
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    deadline.CancelAfter(TimeSpan.FromSeconds(45));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    HttpResponseMessage response;
                    try
                    {
                        response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, deadline.Token).ConfigureAwait(false);
                    }
                    catch (HttpRequestException ex) { throw new OuraException(OuraHttpClient.DescribeSendFailure(ex, "data request")); }
                    catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
                    { throw new OuraException("The Oura data request timed out. Check your connection and try again."); }

                    using (response)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            try { return await OuraHttpContent.ReadAsync(response.Content, OuraHttpContent.MaximumDataBytes, deadline.Token).ConfigureAwait(false); }
                            catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
                            { throw new OuraException("The Oura response timed out. Check your connection and try again."); }
                            catch (IOException) { throw new OuraException("The Oura response was interrupted. Try the export again."); }
                        }

                        if (response.StatusCode == HttpStatusCode.Unauthorized && !refreshed)
                        {
                            refreshed = true;
                            token = await tokenProvider(true, token, cancellation).ConfigureAwait(false);
                            continue;
                        }

                        int status = (int)response.StatusCode;
                        if ((status == 429 || status == 500 || status == 502 || status == 503 || status == 504) && retries < 2)
                        {
                            TimeSpan wait = response.Headers.RetryAfter?.Delta ??
                                (response.Headers.RetryAfter?.Date - utcNow()) ?? TimeSpan.FromSeconds(Math.Pow(2, retries));
                            if (wait < TimeSpan.Zero) wait = TimeSpan.Zero;
                            if (wait > TimeSpan.FromSeconds(30))
                                throw new OuraApiException(response.StatusCode,
                                    "Oura is busy or rate limited. Retry the export after " + Math.Ceiling(wait.TotalSeconds).ToString(CultureInfo.InvariantCulture) + " seconds.");
                            retries++;
                            // The response is disposed before the wait, so pooled connections are not held idle.
                            response.Dispose();
                            await delay(wait, cancellation).ConfigureAwait(false);
                            continue;
                        }

                        string guidance = status == 401 ? "Reconnect Oura; the authorization is expired or revoked." :
                            status == 403 ? "Check consent for this data type and your active Oura membership. Reconnect to change consent." :
                            status == 429 ? "Oura's rate limit was reached. Try the export later." :
                            status == 400 || status == 422 ? "Oura rejected the request parameters. Check the dates and update the add-in if the problem persists." :
                            "The Oura service could not complete this request. Try again later.";
                        throw new OuraApiException(response.StatusCode, "Oura request failed (HTTP " + status + "). " + guidance);
                    }
                }
            }
        }

        internal static Uri ValidateUrl(string url)
        {
            Uri uri;
            if (String.IsNullOrWhiteSpace(url) || url.Length > 16384 || !Uri.TryCreate(url, UriKind.Absolute, out uri) ||
                uri.Scheme != "https" || uri.Host != "api.ouraring.com" || uri.Port != 443 ||
                !uri.AbsolutePath.StartsWith("/v2/usercollection/", StringComparison.Ordinal) || uri.UserInfo.Length != 0 || uri.Fragment.Length != 0)
                throw new ArgumentException("Only official Oura V2 data URLs without credentials are supported.", nameof(url));

            var allowed = new HashSet<string>(StringComparer.Ordinal) { "start_date", "end_date", "start_datetime", "end_datetime", "next_token", "fields", "latest" };
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (string item in uri.Query.TrimStart('?').Split('&'))
            {
                if (item.Length == 0) continue;
                string key = Uri.UnescapeDataString(item.Split('=')[0].Replace("+", " "));
                if (!allowed.Contains(key) || !seen.Add(key))
                    throw new ArgumentException("The Oura data URL contains an unsupported or duplicate query parameter.", nameof(url));
            }
            return uri;
        }
    }
}
