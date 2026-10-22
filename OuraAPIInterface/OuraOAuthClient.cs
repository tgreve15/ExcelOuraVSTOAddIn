using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OuraAPIInterface
{
    public sealed class OuraAuthorizationDeniedException : OuraException
    {
        public OuraAuthorizationDeniedException() : base("Oura authorization was declined or failed. Start sign-in again.") { }
    }

    public static class OuraOAuthEndpoints
    {
        public const string PublicToken = "https://api.ouraring.com/oauth/token";
        public const string ExternalIssuer = "https://moi.ouraring.com/oauth/v2/ext/oauth-anonymous";
        // Retained only to recognize/migrate settings saved by 1.0.0.12.
        public const string ExternalToken = "https://moi.ouraring.com/oauth/v2/ext/oauth-token";

        // An issuer is callback input, not a URL to fetch. Never discover endpoints
        // from an arbitrary callback or send credentials to an unrecognized host.
        internal static string TokenForIssuer(string issuer)
        {
            if (issuer == null) return PublicToken;
            // The public OAuth gateway handles external-issuer grants. The direct
            // identity-provider endpoint is not the public API application entry point.
            if (issuer == ExternalIssuer) return PublicToken;
            throw new OuraException("Unexpected Oura authorization issuer. Sign-in was stopped before sending credentials.");
        }
        internal static void ValidateTokenEndpoint(string endpoint)
        {
            if (endpoint != PublicToken && endpoint != ExternalToken)
                throw new OuraException("The saved Oura token endpoint is not recognized. Disconnect and reconnect Oura.");
        }
    }

    public sealed class OuraOAuthSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; } = "http://localhost:48179/oura/callback/";
        public string TokenEndpoint { get; set; } = OuraOAuthEndpoints.PublicToken;
        public void Validate()
        {
            OuraOAuthEndpoints.ValidateTokenEndpoint(TokenEndpoint);
            Uri uri;
            OuraArguments.ValidateSecret(ClientId, "client ID", 1024);
            OuraArguments.ValidateSecret(ClientSecret, "client secret", 8192);
            if (RedirectUri == null || RedirectUri.Length > 2048 || !Uri.TryCreate(RedirectUri, UriKind.Absolute, out uri) || uri.Scheme != "http" ||
                uri.Host != "localhost" || uri.Port < 1024 || !uri.AbsolutePath.EndsWith("/") ||
                !String.IsNullOrEmpty(uri.Query + uri.Fragment + uri.UserInfo))
                throw new OuraException("Use an HTTP localhost redirect with a port above 1023 and a path ending in /, registered exactly in Oura.");
        }
    }

    public sealed class OuraOAuthTokens
    {
        [JsonProperty("access_token")] public string AccessToken { get; set; }
        [JsonProperty("refresh_token")] public string RefreshToken { get; set; }
        [JsonProperty("token_type")] public string TokenType { get; set; }
        [JsonProperty("expires_in")] public long ExpiresIn { get; set; }
        [JsonProperty("scope")] public string Scope { get; set; }
        public DateTimeOffset ExpiresAtUtc { get; set; }

        public void Validate()
        {
            OuraArguments.ValidateAccessToken(AccessToken);
            OuraArguments.ValidateSecret(RefreshToken, "refresh token");
            if (!String.Equals(TokenType, "bearer", StringComparison.OrdinalIgnoreCase) || ExpiresIn <= 0 || ExpiresIn > 31536000)
                throw new OuraException("Oura returned an incomplete token response. Reconnect Oura.");
        }
    }

    public sealed class OuraAuthorization
    {
        public string State { get; }
        public string CodeVerifier { get; }
        public string AuthorizationUrl { get; }
        public string TokenEndpoint { get; private set; } = OuraOAuthEndpoints.PublicToken;
        public OuraAuthorization(OuraOAuthSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            settings.Validate();
            State = RandomValue();
            CodeVerifier = RandomValue();
            string challenge;
            using (var sha = SHA256.Create()) challenge = Base64Url(sha.ComputeHash(Encoding.ASCII.GetBytes(CodeVerifier)));
            AuthorizationUrl = "https://cloud.ouraring.com/oauth/authorize?" + OuraOAuthClient.FormQuery(new Dictionary<string, string> {
                { "response_type", "code" }, { "client_id", settings.ClientId }, { "redirect_uri", settings.RedirectUri },
                { "scope", OuraCollections.RequestedScopes }, { "state", State },
                { "code_challenge", challenge }, { "code_challenge_method", "S256" }
            });
        }
        public string ReadCode(Uri callback, string redirectUri)
        {
            if (callback == null || !callback.IsAbsoluteUri || callback.Query.Length > 8192)
                throw new OuraException("The Oura callback is missing or invalid.");
            var expected = new Uri(redirectUri);
            if (callback.GetLeftPart(UriPartial.Path) != expected.GetLeftPart(UriPartial.Path) || callback.Fragment.Length != 0)
                throw new OuraException("Unexpected Oura callback address.");
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var item in callback.Query.TrimStart('?').Split('&'))
            {
                if (item.Length == 0) continue;
                var parts = item.Split(new[] { '=' }, 2);
                var key = Uri.UnescapeDataString(parts[0].Replace("+", " "));
                if (values.ContainsKey(key)) throw new OuraException("Duplicate OAuth callback parameter.");
                values.Add(key, parts.Length == 2 ? Uri.UnescapeDataString(parts[1].Replace("+", " ")) : "");
            }
            string state, code;
            if (!values.TryGetValue("state", out state) || state != State)
                throw new OuraException("Oura sign-in state did not match. Start sign-in again.");
            if (values.ContainsKey("error")) throw new OuraAuthorizationDeniedException();
            if (!values.TryGetValue("code", out code) || String.IsNullOrWhiteSpace(code))
                throw new OuraException("Oura did not return an authorization code.");
            string issuer;
            values.TryGetValue("iss", out issuer);
            TokenEndpoint = OuraOAuthEndpoints.TokenForIssuer(issuer);
            return code;
        }
        private static string RandomValue()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);
            return Base64Url(bytes);
        }
        private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public sealed class OuraOAuthClient
    {
        private readonly HttpClient http;
        public OuraOAuthClient(HttpClient http) { this.http = http ?? throw new ArgumentNullException(nameof(http)); }
        internal static string FormQuery(IDictionary<string, string> values) => String.Join("&", values.Select(p => Uri.EscapeDataString(p.Key) + "=" + Uri.EscapeDataString(p.Value)));
        public Task<OuraOAuthTokens> ExchangeAsync(OuraOAuthSettings settings, string code, string verifier, CancellationToken cancellation)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            OuraArguments.ValidateSecret(code, "authorization code", 4096);
            if (verifier == null || verifier.Length < 43 || verifier.Length > 128 || verifier.Any(c => !(Char.IsLetterOrDigit(c) && c < 128) && "-._~".IndexOf(c) < 0))
                throw new OuraException("The sign-in verifier is invalid. Start sign-in again.");
            return RequestAsync(settings, new Dictionary<string, string> {
                { "grant_type", "authorization_code" }, { "code", code }, { "redirect_uri", settings.RedirectUri }, { "code_verifier", verifier }
            }, cancellation);
        }
        public Task<OuraOAuthTokens> RefreshAsync(OuraOAuthSettings settings, string refreshToken, CancellationToken cancellation)
        {
            OuraArguments.ValidateSecret(refreshToken, "refresh token");
            return RequestAsync(settings, new Dictionary<string, string> { { "grant_type", "refresh_token" }, { "refresh_token", refreshToken } }, cancellation);
        }
        internal static string DescribeTokenFailure(HttpResponseMessage response, string body)
        {
            string code = null;
            bool blocked = false;
            try
            {
                var json = JObject.Parse(body);
                if (json["error"]?.Type == JTokenType.String) code = (string)json["error"];
                blocked = json["message"]?.Type == JTokenType.String &&
                    String.Equals((string)json["message"], "Request blocked", StringComparison.OrdinalIgnoreCase);
            }
            catch (JsonException) { } // An HTML proxy error is not an OAuth error response.
            string guidance;
            switch (code)
            {
                case "invalid_client": guidance = "Oura rejected the application credentials. Check the client ID and secret in developer.ouraring.com/applications."; break;
                case "invalid_grant": guidance = "The authorization code or refresh token was rejected. Start a fresh sign-in and check that the registered redirect address matches exactly."; break;
                case "invalid_scope": guidance = "Oura rejected the requested data permissions. Check your application's permissions in the developer portal."; break;
                case "access_denied": guidance = "Oura denied authorization for this application. Check its access in the developer portal, then start sign-in again."; break;
                default:
                    code = null; // Never echo arbitrary API fields, descriptions or response content.
                    guidance = blocked ? "Oura's request filter blocked the token request. Check firewall/proxy filtering or contact Oura support if it persists." :
                        "Oura refused the token request without a recognized OAuth error. Check your application registration and connection, then start sign-in again.";
                    break;
            }
            return "Oura sign-in/refresh failed via api.ouraring.com (HTTP " + (int)response.StatusCode +
                (code == null ? "" : ", " + code) + "). " + guidance;
        }
        private async Task<OuraOAuthTokens> RequestAsync(OuraOAuthSettings settings, Dictionary<string, string> form, CancellationToken cancellation)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            settings.Validate();
            cancellation.ThrowIfCancellationRequested();
            form.Add("client_id", settings.ClientId);
            form.Add("client_secret", settings.ClientSecret);
            // Never retry a token exchange automatically: Oura refresh tokens are single-use.
            try
            {
                using (var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation))
                using (var request = new HttpRequestMessage(HttpMethod.Post, OuraOAuthEndpoints.PublicToken))
                {
                    deadline.CancelAfter(TimeSpan.FromSeconds(45));
                    request.Content = new FormUrlEncodedContent(form);
                    using (var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, deadline.Token).ConfigureAwait(false))
                    {
                        string json = await OuraHttpContent.ReadAsync(response.Content, OuraHttpContent.MaximumTokenBytes, deadline.Token).ConfigureAwait(false);
                        if (!response.IsSuccessStatusCode) throw new OuraException(DescribeTokenFailure(response, json));
                        var parsed = OuraJson.ParseObject(json);
                        if (parsed["expires_in"]?.Type != JTokenType.Integer)
                            throw new OuraException("Oura returned an invalid token lifetime. Start sign-in again.");
                        var tokens = parsed.ToObject<OuraOAuthTokens>();
                        if (tokens == null) throw new OuraException("Oura returned an empty token response. Start sign-in again.");
                        tokens.Validate();
                        tokens.ExpiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(tokens.ExpiresIn);
                        return tokens;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                throw new OuraException(OuraHttpClient.DescribeSendFailure(ex, "sign-in/refresh request"));
            }
            catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
            {
                throw new OuraException("The Oura sign-in/refresh request timed out. Check your connection and start sign-in again.");
            }
            catch (System.IO.IOException)
            {
                throw new OuraException("The Oura sign-in response was interrupted. Start sign-in again.");
            }
            catch (JsonException)
            {
                throw new OuraException("Oura returned an unreadable sign-in/refresh response. Start sign-in again.");
            }
        }
    }
}
