using ExcelOuraVSTOAddIn;
using Newtonsoft.Json;
using OuraAPIInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal static class Program
{
    private static int passed;
    private static void Check(bool ok, string name) { if (!ok) throw new Exception(name); passed++; Console.WriteLine("PASS " + name); }
    private static void Reject(Action action, string name) { try { action(); } catch (InvalidOperationException) { Check(true, name); return; } throw new Exception("Expected rejection: " + name); }
    private static OuraOAuthSettings Settings() => new OuraOAuthSettings { ClientId = "test-client", ClientSecret = "test-secret" };
    private static HttpResponseMessage Reply(int code, string body = "{}") => new HttpResponseMessage((HttpStatusCode)code) { Content = new StringContent(body) };
    private sealed class Handler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, Task<HttpResponseMessage>> Run;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellation) => Run(request);
    }
    private static async Task Main()
    {
        var auth = new OuraAuthorization(Settings());
        using (var sha = SHA256.Create())
        {
            var challenge = Convert.ToBase64String(sha.ComputeHash(Encoding.ASCII.GetBytes(auth.CodeVerifier))).TrimEnd('=').Replace('+','-').Replace('/','_');
            Check(auth.AuthorizationUrl.Contains("code_challenge=" + challenge) && auth.AuthorizationUrl.Contains("code_challenge_method=S256"), "PKCE S256 challenge");
        }
        Check(auth.State.Length >= 43 && auth.State != new OuraAuthorization(Settings()).State, "Random per-attempt state");
        string redirect = Settings().RedirectUri;
        Check(auth.ReadCode(new Uri(redirect + "?state=" + auth.State + "&code=abc%2B123"), redirect) == "abc+123", "Callback code decoding");
        Reject(() => auth.ReadCode(new Uri(redirect + "?state=wrong&code=x"), redirect), "Reject wrong state");
        Reject(() => auth.ReadCode(new Uri(redirect + "?code=x"), redirect), "Reject missing state");
        Reject(() => auth.ReadCode(new Uri(redirect + "?state=" + auth.State + "&error=access_denied"), redirect), "Handle consent denial");
        Reject(() => auth.ReadCode(new Uri(redirect + "?state=" + auth.State + "&code=x&code=y"), redirect), "Reject duplicate callback parameters");
        Reject(() => auth.ReadCode(new Uri("http://localhost:48180/oura/callback/?state=" + auth.State + "&code=x"), redirect), "Reject callback on wrong port");
        Reject(() => new OuraOAuthSettings { ClientId="x", ClientSecret="y", RedirectUri="https://example.com/" }.Validate(), "Reject non-loopback redirect");
        var handler = new Handler();
        using (var http = new HttpClient(handler))
        {
            handler.Run = async request => {
                var body = await request.Content.ReadAsStringAsync();
                Check(request.RequestUri.AbsoluteUri == "https://api.ouraring.com/oauth/token" && request.Method == HttpMethod.Post, "Official token endpoint POST");
                Check(!request.RequestUri.Query.Contains("secret") && body.Contains("client_secret=test-secret") && body.Contains("code_verifier="), "Code exchange credentials and verifier in form body");
                return Reply(200, "{\"token_type\":\"Bearer\",\"access_token\":\"access\",\"refresh_token\":\"refresh\",\"expires_in\":3600}");
            };
            var oauth = new OuraOAuthClient(http);
            var tokens = await oauth.ExchangeAsync(Settings(), "code", auth.CodeVerifier, CancellationToken.None);
            Check(tokens.ExpiresAtUtc > DateTimeOffset.UtcNow.AddMinutes(59), "Track token expiry");
            int refreshCalls = 0;
            handler.Run = async request => {
                var body = await request.Content.ReadAsStringAsync(); refreshCalls++;
                Check(body.Contains("grant_type=refresh_token") && body.Contains("refresh_token=refresh"), "Refresh form contains previous refresh token");
                return Reply(200, "{\"token_type\":\"bearer\",\"access_token\":\"next-access\",\"refresh_token\":\"next-refresh\",\"expires_in\":3600}");
            };
            var rotated = await oauth.RefreshAsync(Settings(), tokens.RefreshToken, CancellationToken.None);
            Check(rotated.RefreshToken == "next-refresh" && refreshCalls == 1, "Use rotated refresh token");
            handler.Run = request => { refreshCalls++; return Task.FromResult(Reply(400, "test-secret refresh")); };
            try { await oauth.RefreshAsync(Settings(), "refresh", CancellationToken.None); throw new Exception("Expected token failure"); }
            catch (InvalidOperationException ex) { Check(!ex.Message.Contains("test-secret") && refreshCalls == 2, "Do not retry token failure or expose body"); }
            handler.Run = request => Task.FromResult(Reply(200, "{\"token_type\":\"bearer\",\"access_token\":\"x\",\"expires_in\":3600}"));
            try { await oauth.RefreshAsync(Settings(), "refresh", CancellationToken.None); throw new Exception("Expected incomplete tokens"); }
            catch (InvalidOperationException) { Check(true, "Reject incomplete token response"); }
            int calls = 0, forced = 0;
            handler.Run = request => {
                calls++;
                Check(request.Headers.Authorization.Scheme == "Bearer" && request.Headers.Authorization.Parameter == (calls == 1 ? "old" : "new"), "Access token in authorization header");
                return Task.FromResult(calls == 1 ? Reply(401) : Reply(200, "success"));
            };
            var transport = new OuraApiTransport(http, force => { if (force) forced++; return force ? "new" : "old"; });
            string endpoint = "https://api.ouraring.com/v2/usercollection/daily_sleep";
            Check(await transport.GetAsync(endpoint, CancellationToken.None) == "success" && forced == 1 && calls == 2, "Refresh and retry a 401 once");
            calls = 0;
            handler.Run = request => { calls++; return Task.FromResult(Reply(401)); };
            try { await transport.GetAsync(endpoint, CancellationToken.None); throw new Exception("Expected 401"); }
            catch (OuraApiException) { Check(calls == 2, "Repeated 401 stops after one refresh"); }
            calls = 0; var waits = new List<TimeSpan>();
            handler.Run = request => { calls++; var r = Reply(calls < 3 ? 429 : 200); r.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(7)); return Task.FromResult(r); };
            transport = new OuraApiTransport(http, force => "token", (wait, ct) => { waits.Add(wait); return Task.CompletedTask; });
            await transport.GetAsync(endpoint, CancellationToken.None);
            Check(calls == 3 && waits.Count == 2 && waits.All(w => w.TotalSeconds == 7), "Honor Retry-After with bounded retries");
            calls = 0;
            handler.Run = request => { calls++; var r=Reply(429); r.Headers.RetryAfter=new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromMinutes(2)); return Task.FromResult(r); };
            try { await transport.GetAsync(endpoint, CancellationToken.None); throw new Exception("Expected rate limit"); }
            catch(OuraApiException ex) { Check(calls == 1 && ex.Message.Contains("120"), "Long Retry-After asks user to wait without early retry"); }
            handler.Run = request => Task.FromResult(Reply(403, "private health payload"));
            try { await transport.GetAsync(endpoint, CancellationToken.None); throw new Exception("Expected 403"); }
            catch(OuraApiException ex) { Check(ex.Message.Contains("consent") && !ex.Message.Contains("private"), "Explain scope/membership without exposing data"); }
            try { await transport.GetAsync("https://example.com/v2/usercollection/sleep", CancellationToken.None); throw new Exception("Expected origin rejection"); }
            catch(ArgumentException) { Check(true, "Reject foreign API origin"); }
        }
        CheckFields(OuraFields.CurrentFields(), typeof(OuraCombinedObjectV2));
        CheckFields(OuraFields.CurrentSpo2Fields(), typeof(OuraAPIInterface.DailySpo2.Datum));
        CheckFields(OuraFields.CurrentWorkoutFields(), typeof(OuraAPIInterface.Workouts.Datum));
        CheckFields(OuraFields.CurrentTagFields(), typeof(OuraAPIInterface.EnhancedTags.Datum));
        var sleep = JsonConvert.DeserializeObject<OuraAPIInterface.DailySleep.Datum>("{\"score\":null,\"contributors\":{\"deep_sleep\":null,\"efficiency\":null},\"timestamp\":\"2026-10-22T00:00:00Z\",\"future_field\":123}");
        var row = new OuraCombinedObjectV2("2026-10-22"); row.UpdateFrom(null, sleep, null, null);
        Check(row.ds_score == null && row.ds_deep_sleep == null && row.act_steps == null && row.rr_Timestamp == null, "Nullable API values and missing days remain blank");
        row.UpdateFrom(null, new OuraAPIInterface.DailySleep.Datum { contributors = null }, new OuraAPIInterface.DailyReadiness.Datum { contributors = null }, new OuraAPIInterface.DailyActivity.Datum { contributors = null });
        Check(row.rr_body_temperature == null && row.act_meet_daily_targets == null, "Null contributors do not abort exports");
        Check(new OuraAPIInterface.DailySpo2.Datum().spo2PercentageAverage() == null, "Null SpO2 remains blank");
        var fields = new List<OuraFields> {
            new OuraFields("Steps", "Activity", "act_steps", 1, true, OuraFields.AccessorType.Property, "Steps") { CustomLabel = "=bad formula" },
            new OuraFields("Tag", "Sleep", "s_type", 2, true, OuraFields.AccessorType.Property, "Tag")
        };
        row.act_steps = 1234; row.s_type = "=HYPERLINK(test)";
        var table = OuraExportTable.Build(new[] { row }, fields, true);
        Check(table.GetLength(0) == 2 && table.GetLength(1) == 2, "Bulk table shape includes headers");
        Check(table[1, 0] is double && (double)table[1,0] == 1234, "Preserve numeric cells");
        Check((string)table[0,0] == "'=bad formula" && ((string)table[1,1]).StartsWith("'="), "Escape formulas in headings and values");
        row.act_steps = null;
        Check(OuraExportTable.Build(new[] { row }, fields, false)[0,0] == null, "Missing values become empty Excel cells");
        fields[0].MethodName = "MissingField";
        Reject(() => OuraExportTable.Build(new[] { row }, fields, true), "Invalid field fails before writing to Excel");
        row.s_type = new string('x', 32768);
        Reject(() => OuraExportTable.Build(new[] { row }, fields.Skip(1).ToList(), false), "Reject values beyond Excel cell limit");
        var sleeps = new[] {
            new OuraAPIInterface.Sleep.Datum { day = "day", type = "sleep", total_sleep_duration = 20000 },
            new OuraAPIInterface.Sleep.Datum { day = "day", type = "long_sleep", total_sleep_duration = 25000, bedtime_end = "2026-10-22T08:00:00" },
            new OuraAPIInterface.Sleep.Datum { day = "day", type = "long_sleep", total_sleep_duration = 26000, bedtime_end = "2026-10-22T09:00:00" },
            new OuraAPIInterface.Sleep.Datum { day = "day", type = "deleted", total_sleep_duration = 90000 }
        };
        Check(OuraAPI2Wrapper.MainSleepForDay(sleeps, "day") == sleeps[2], "Select longest main sleep and ignore deleted periods");
        Check(OuraAPI2Wrapper.MainSleepForDay(sleeps, "missing") == null, "Missing sleep day remains empty");
        var met = JsonConvert.DeserializeObject<OuraAPIInterface.DailyActivity.Met>("{\"items\":[1.2,null,2.0]}");
        Check(met.items.Count == 3 && met.items[1] == null, "Nullable MET samples survive deserialization");
        using (var tlsHandler = OuraHttpClient.CreateHandler())
        {
            Check(tlsHandler.SslProtocols == System.Security.Authentication.SslProtocols.Tls12, "Set TLS 1.2 explicitly for Excel-hosted HTTP clients");
            Check(!tlsHandler.AllowAutoRedirect && tlsHandler.ServerCertificateCustomValidationCallback == null, "Keep certificate validation and disable credential redirects");
        }
        var newAuth = new OuraAuthorization(Settings());
        var newCallback = new Uri(redirect + "?iss=" + Uri.EscapeDataString(OuraOAuthEndpoints.ExternalIssuer) + "&state=" + newAuth.State + "&code=new-code");
        Check(newAuth.ReadCode(newCallback, redirect) == "new-code" && newAuth.TokenEndpoint == OuraOAuthEndpoints.PublicToken, "Validate external issuer while keeping the public token gateway");
        var badIssuer = new OuraAuthorization(Settings());
        Reject(() => badIssuer.ReadCode(new Uri(redirect + "?iss=https%3A%2F%2Fexample.com&state=" + badIssuer.State + "&code=secret"), redirect), "Reject untrusted issuer before sending credentials");
        Reject(() => new OuraOAuthSettings { ClientId = "test", ClientSecret = "secret", TokenEndpoint = "https://example.com/token" }.Validate(), "Reject untrusted stored token endpoint");
        Check(auth.TokenEndpoint == OuraOAuthEndpoints.PublicToken, "No-issuer callbacks use the documented public gateway");
        var externalSettings = Settings(); externalSettings.TokenEndpoint = OuraOAuthEndpoints.ExternalToken; // Simulate settings saved by 1.0.0.12.
        // This is the persisted settings shape used by encrypted storage and refresh.
        externalSettings = JsonConvert.DeserializeObject<OuraOAuthSettings>(JsonConvert.SerializeObject(externalSettings));
        using (var externalHttp = new HttpClient(new Handler { Run = async request => {
            Check(request.RequestUri.AbsoluteUri == OuraOAuthEndpoints.PublicToken, "Exchange and refresh migrate old direct settings to the public token gateway");
            string body = await request.Content.ReadAsStringAsync();
            Check(body.Contains("client_secret=test-secret") && !request.RequestUri.Query.Contains("secret"), "Public gateway receives credentials only in POST body");
            return Reply(200, "{\"token_type\":\"Bearer\",\"access_token\":\"external-access\",\"refresh_token\":\"external-refresh\",\"expires_in\":3600}");
        } }))
        {
            var externalOAuth = new OuraOAuthClient(externalHttp);
            await externalOAuth.ExchangeAsync(externalSettings, "new-code", newAuth.CodeVerifier, CancellationToken.None);
            await externalOAuth.RefreshAsync(externalSettings, "external-refresh", CancellationToken.None);
        }
        var secureError = new HttpRequestException("access-token test-secret code=private", new WebException("private-certificate-detail", WebExceptionStatus.SecureChannelFailure));
        var secureMessage = OuraHttpClient.DescribeSendFailure(secureError, "sign-in request");
        Check(secureMessage.Contains("SecureChannelFailure") && secureMessage.Contains("TLS 1.2") && !secureMessage.Contains("test-secret") && !secureMessage.Contains("private"), "Report safe TLS diagnostics without echoing exception credentials");
        var dnsMessage = OuraHttpClient.DescribeSendFailure(new HttpRequestException("secret", new WebException("private-host", WebExceptionStatus.NameResolutionFailure)), "data request");
        Check(dnsMessage.Contains("NameResolutionFailure") && !dnsMessage.Contains("private-host"), "Report DNS failure safely");
        int sendFailures = 0;
        using (var failingHttp = new HttpClient(new Handler { Run = request => { sendFailures++; throw secureError; } }))
        {
            try { await new OuraOAuthClient(failingHttp).ExchangeAsync(Settings(), "private-code", auth.CodeVerifier, CancellationToken.None); throw new Exception("Expected network failure"); }
            catch (InvalidOperationException ex) { Check(ex.Message.Contains("SecureChannelFailure") && !ex.Message.Contains("test-secret") && sendFailures == 1, "Surface token transport cause without retrying a single-use code"); }
            try { await new OuraApiTransport(failingHttp, force => "private-access").GetAsync("https://api.ouraring.com/v2/usercollection/sleep", CancellationToken.None); throw new Exception("Expected data network failure"); }
            catch (InvalidOperationException ex) { Check(ex.Message.Contains("data request") && ex.Message.Contains("SecureChannelFailure"), "Data requests also use safe transport diagnostics"); }
        }
        using (var timeoutHttp = new HttpClient(new Handler { Run = request => { throw new TaskCanceledException(); } }))
        {
            try { await new OuraOAuthClient(timeoutHttp).RefreshAsync(Settings(), "private-refresh", CancellationToken.None); throw new Exception("Expected timeout"); }
            catch (InvalidOperationException ex) { Check(ex.Message.Contains("timed out"), "Report HTTP timeout separately from user cancellation"); }
            using (var cancelled = new CancellationTokenSource())
            {
                cancelled.Cancel();
                try { await new OuraOAuthClient(timeoutHttp).RefreshAsync(Settings(), "private-refresh", cancelled.Token); throw new Exception("Expected cancellation"); }
                catch (OperationCanceledException) { Check(true, "Preserve user cancellation semantics"); }
            }
        }
        using (var blocked = Reply(403))
        {
            var message = OuraOAuthClient.DescribeTokenFailure(blocked, "{\"message\":\"Request blocked\",\"private\":\"test-secret\"}");
            Check(message.Contains("request filter") && message.Contains("api.ouraring.com") && !message.Contains("test-secret"), "Explain blocked token request without blaming credentials or exposing body");
        }
        using (var invalid = Reply(400))
        {
            var message = OuraOAuthClient.DescribeTokenFailure(invalid, "{\"error\":\"invalid_client\",\"error_description\":\"test-secret private-code\"}");
            Check(message.Contains("invalid_client") && message.Contains("developer.ouraring.com/applications") && !message.Contains("test-secret"), "Explain a genuine invalid-client response using only an allowlisted error code");
            message = OuraOAuthClient.DescribeTokenFailure(invalid, "{\"error\":\"invalid_grant\"}");
            Check(message.Contains("fresh sign-in") && message.Contains("redirect"), "Explain code/refresh rejection without replaying the grant");
            message = OuraOAuthClient.DescribeTokenFailure(invalid, "{\"error\":\"test-secret\"}");
            Check(!message.Contains("test-secret"), "Do not echo arbitrary OAuth error codes");
            message = OuraOAuthClient.DescribeTokenFailure(invalid, "<html>private-health-response test-secret</html>");
            Check(!message.Contains("test-secret") && !message.Contains("private-health"), "Handle HTML proxy failures without exposing content");
        }
        passed += CollectionTests.Run();
        Console.WriteLine(passed + " checks passed.");
    }
    private static void CheckFields(List<OuraFields> fields, Type type)
    {
        foreach(var field in fields)
        {
            bool ok = field.Accessor == OuraFields.AccessorType.Method ? type.GetMethod(field.MethodName, Type.EmptyTypes) != null : type.GetProperty(field.MethodName) != null;
            Check(ok, type.Name + "." + field.MethodName + " field mapping");
        }
    }
}
