using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExcelOuraVSTOAddIn;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OuraAPIInterface;
using Xunit;

public sealed class HardeningTests
{
    private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 10, 22, 0, 0, 0, TimeSpan.Zero);
    private static OuraOAuthTokens Tokens(string access = "old-access") => new OuraOAuthTokens
    {
        AccessToken = access, RefreshToken = "single-use-refresh", TokenType = "Bearer", ExpiresIn = 3600,
        ExpiresAtUtc = Now.AddHours(1)
    };
    private static OuraAccount Account() => new OuraAccount
    {
        Settings = new OuraOAuthSettings { ClientId = "client", ClientSecret = "secret" }, Tokens = Tokens()
    };
    private static OuraAccount Copy(OuraAccount value) => JsonConvert.DeserializeObject<OuraAccount>(JsonConvert.SerializeObject(value));

    [Fact]
    public void FreshAccessTokenDoesNotRefreshOrWrite()
    {
        var manager = new OuraTokenManager(Account, _ => throw new Exception("Unexpected write"),
            (s, r, c) => throw new Exception("Unexpected refresh"), () => Now);
        Assert.Equal("old-access", manager.GetAccessToken(false, null, default));
    }

    [Fact]
    public void RefreshPersistsPendingBeforePostingAndSavesRotatedToken()
    {
        var stored = Account();
        var writes = new List<bool>();
        var manager = new OuraTokenManager(() => Copy(stored), a => { stored = Copy(a); writes.Add(a.RefreshPending); },
            (s, r, c) => { Assert.True(stored.RefreshPending); Assert.Equal("single-use-refresh", r); return Tokens("new-access"); }, () => Now);
        Assert.Equal("new-access", manager.GetAccessToken(true, "old-access", default));
        Assert.Equal(new[] { true, false }, writes);
        Assert.Equal("new-access", stored.Tokens.AccessToken);
    }

    [Fact]
    public void LostRefreshResponseCannotReplayRefreshToken()
    {
        var stored = Account();
        int posts = 0;
        var manager = new OuraTokenManager(() => Copy(stored), a => stored = Copy(a),
            (s, r, c) => { posts++; throw new OuraException("Connection interrupted"); }, () => Now);
        Assert.Throws<OuraException>(() => manager.GetAccessToken(true, null, default));
        Assert.True(stored.RefreshPending);
        Assert.Throws<OuraException>(() => manager.GetAccessToken(true, null, default));
        Assert.Equal(1, posts);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void PersistenceFailureNeverAllowsUnsafeReplay(int failingWrite)
    {
        var stored = Account();
        int writes = 0, posts = 0;
        var manager = new OuraTokenManager(() => Copy(stored), a =>
        {
            if (++writes == failingWrite) throw new System.IO.IOException("private path");
            stored = Copy(a);
        }, (s, r, c) => { posts++; return Tokens("new-access"); }, () => Now);
        var error = Assert.Throws<OuraException>(() => manager.GetAccessToken(true, null, default));
        Assert.DoesNotContain("private path", error.Message);
        Assert.Equal(failingWrite == 1 ? 0 : 1, posts);
        Assert.Equal(failingWrite == 2, stored.RefreshPending);
    }

    [Fact]
    public void AnotherProcessRefreshedTheRejectedToken()
    {
        var account = Account();
        account.Tokens.AccessToken = "newer-access";
        var manager = new OuraTokenManager(() => account, _ => throw new Exception(), (s, r, c) => throw new Exception(), () => Now);
        Assert.Equal("newer-access", manager.GetAccessToken(true, "old-access", default));
    }

    [Fact]
    public void ExpiringTokenRefreshesProactively()
    {
        var account = Account(); account.Tokens.ExpiresAtUtc = Now.AddMinutes(1);
        var manager = new OuraTokenManager(() => account, _ => { }, (s, r, c) => Tokens("rotated"), () => Now);
        Assert.Equal("rotated", manager.GetAccessToken(false, null, default));
    }

    [Fact]
    public void PreCancelledTokenDoesNotReadCredentials()
    {
        var manager = new OuraTokenManager(() => throw new Exception(), _ => throw new Exception(), (s, r, c) => throw new Exception());
        Assert.Throws<OperationCanceledException>(() => manager.GetAccessToken(false, null, new CancellationToken(true)));
    }

    [Theory]
    [InlineData("{\"data\":[],\"data\":[]}")]
    [InlineData("{} {}")]
    [InlineData("[]")]
    [InlineData("{ broken")]
    public void StrictJsonRejectsAmbiguousResponses(string json) => Assert.ThrowsAny<JsonException>(() => OuraJson.ParseObject(json));

    [Fact]
    public void ExcessiveJsonDepthIsRejected() => Assert.ThrowsAny<JsonException>(() =>
        OuraJson.ParseObject("{\"v\":" + new string('[', 40) + "0" + new string(']', 40) + "}"));

    [Fact]
    public async Task ResponseLimitsApplyToActualBodyBytes()
    {
        using var content = new ByteArrayContent(new byte[100]);
        await Assert.ThrowsAsync<OuraException>(() => OuraHttpContent.ReadAsync(content, 50, default));
    }

    [Fact]
    public async Task InvalidUtf8FailsWithSafeMessage()
    {
        using var content = new ByteArrayContent(new byte[] { 0xff });
        await Assert.ThrowsAsync<OuraException>(() => OuraHttpContent.ReadAsync(content, 50, default));
    }

    [Theory]
    [InlineData("http://api.ouraring.com/v2/usercollection/sleep")]
    [InlineData("https://api.ouraring.com.attacker.example/v2/usercollection/sleep")]
    [InlineData("https://user@api.ouraring.com/v2/usercollection/sleep")]
    [InlineData("https://api.ouraring.com/v2/usercollection/sleep?%61ccess_token=secret")]
    [InlineData("https://api.ouraring.com/v2/usercollection/sleep?start_date=a&start_date=b")]
    [InlineData("https://api.ouraring.com/v2/usercollection/sleep#fragment")]
    public void RejectsCredentialLeakageAndAmbiguousDataUrls(string url) => Assert.Throws<ArgumentException>(() => OuraApiTransport.ValidateUrl(url));

    [Fact]
    public void OpaqueContinuationValuesAreAllowed() => Assert.NotNull(OuraApiTransport.ValidateUrl(
        "https://api.ouraring.com/v2/usercollection/sleep?next_token=access_token%3Dopaque"));

    private sealed class Handler : HttpMessageHandler
    {
        internal Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Respond;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellation) => Respond(request, cancellation);
    }

    [Fact]
    public async Task UnauthorizedRequestPassesRejectedTokenToRefresh()
    {
        int sends = 0;
        using var handler = new Handler { Respond = (r, c) => Task.FromResult(++sends == 1
            ? new HttpResponseMessage(HttpStatusCode.Unauthorized)
            : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") }) };
        using var http = new HttpClient(handler);
        var transport = new OuraApiTransport(http, (force, rejected, c) =>
        {
            Assert.Equal(force ? "old" : null, rejected);
            return Task.FromResult(force ? "new" : "old");
        });
        Assert.Equal("{}", await transport.GetAsync("https://api.ouraring.com/v2/usercollection/sleep", default));
        Assert.Equal(2, sends);
    }

    [Fact]
    public async Task RetryAfterDateUsesInjectedClockAndStopsAfterTwoRetries()
    {
        int sends = 0;
        var waits = new List<TimeSpan>();
        using var handler = new Handler { Respond = (r, c) =>
        {
            sends++;
            var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(Now.AddSeconds(4));
            return Task.FromResult(response);
        } };
        using var http = new HttpClient(handler);
        var transport = new OuraApiTransport(http, (f, r, c) => Task.FromResult("token"),
            (d, c) => { waits.Add(d); return Task.CompletedTask; }, () => Now);
        await Assert.ThrowsAsync<OuraApiException>(() => transport.GetAsync("https://api.ouraring.com/v2/usercollection/sleep", default));
        Assert.Equal(3, sends);
        Assert.Equal(new[] { TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(4) }, waits);
    }

    [Theory]
    [InlineData(400)]
    [InlineData(403)]
    [InlineData(404)]
    [InlineData(422)]
    public async Task PermanentErrorsAreNotRetriedOrEchoed(int status)
    {
        int sends = 0;
        using var handler = new Handler { Respond = (r, c) =>
        {
            sends++;
            return Task.FromResult(new HttpResponseMessage((HttpStatusCode)status) { Content = new StringContent("private health data") });
        } };
        using var http = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<OuraApiException>(() => new OuraApiTransport(http, _ => "token")
            .GetAsync("https://api.ouraring.com/v2/usercollection/sleep", default));
        Assert.Equal(1, sends);
        Assert.DoesNotContain("private health data", error.Message);
    }

    [Theory]
    [InlineData(1048577, 1, 1, 1)]
    [InlineData(2, 1, 1048576, 1)]
    [InlineData(1, 2, 1, 16384)]
    [InlineData(1, 0, 1, 1)]
    [InlineData(1000000, 6, 1, 1)]
    [InlineData(int.MaxValue, 1, int.MaxValue, 1)]
    public void ExportBoundsRejectOverflowAndExcessiveAllocations(int rows, int columns, int startRow, int startColumn) =>
        Assert.Throws<OuraException>(() => OuraExportLimits.Validate(rows, columns, startRow, startColumn));

    [Theory]
    [InlineData("=1+1")]
    [InlineData(" +1")]
    [InlineData("-1")]
    [InlineData("\t@SUM(A1)")]
    public void FormulaLikeTextIsEscaped(string value) => Assert.Equal("'" + value, OuraExportTable.CellValue(value));

    [Fact]
    public void EscapingCannotExceedExcelTextLimit() => Assert.Throws<OuraException>(() =>
        OuraExportTable.CellValue("=" + new string('x', OuraExportLimits.MaximumTextLength - 1)));

    [Fact]
    public void LongIdentifiersRetainPrecision() => Assert.Equal("'1234567890123456", OuraExportTable.CellValue(1234567890123456L));

    [Fact]
    public void MeasuredZeroIsNumericAndMissingIsBlank()
    {
        Assert.Equal(0, OuraExportTable.CellValue(0));
        Assert.Null(OuraExportTable.CellValue(null));
        Assert.Throws<OuraException>(() => OuraExportTable.CellValue(double.NaN));
    }

    [Fact]
    public void SavedPreferencesCannotInjectAccessorOrHideFields()
    {
        var defaults = OuraFields.CurrentFields();
        var saved = OuraFieldSelections.Clone(defaults[0]);
        saved.MethodName = "ToString"; saved.Accessible = false; saved.CustomLabel = "My label";
        var merged = OuraFieldSelections.Merge(defaults, JsonConvert.SerializeObject(new[] { saved }));
        Assert.Equal(defaults[0].MethodName, merged[0].MethodName);
        Assert.Equal(defaults[0].Accessible, merged[0].Accessible);
        Assert.Equal("My label", merged[0].CustomLabel);
        Assert.NotEqual("My label", defaults[0].CustomLabel);
    }

    [Theory]
    [InlineData("malformed")]
    [InlineData("[null]")]
    [InlineData("null")]
    public void CorruptPreferencesFallBackToDefaults(string json) =>
        Assert.Equal(OuraFields.CurrentFields().Count, OuraFieldSelections.Merge(OuraFields.CurrentFields(), json).Count);

    [Fact]
    public void EditingDefaultsDoesNotMutateFutureExports()
    {
        var fields = OuraFields.CurrentFields(); fields[0].MethodName = "broken"; fields.Clear();
        Assert.NotEmpty(OuraFields.CurrentFields());
        Assert.DoesNotContain(OuraFields.CurrentFields(), f => f.MethodName == "broken");
    }

    [Fact]
    public void RecordsOwnTheirSnapshotAndAcceptOnlySimplePaths()
    {
        var source = JObject.Parse("{\"a\":{\"b\":1}}"); var record = new OuraRecord(source);
        source["a"]["b"] = 2;
        Assert.Equal(1L, record.Read("a.b"));
        Assert.Null(record.Read("missing.b"));
        Assert.Throws<ArgumentException>(() => record.Read("$..b"));
    }

    [Fact]
    public async Task RowLimitFailsBeforeReturningPartialResults()
    {
        await Assert.ThrowsAsync<OuraException>(() => OuraCollectionClient.FetchAsync(OuraCollections.Get("ring_configuration"),
            Now.Date, Now.Date, default, null, TimeZoneInfo.Utc,
            (u, c) => Task.FromResult("{\"data\":[{\"id\":\"a\"},{\"id\":\"b\"}]}"), new OuraFetchOptions(1)));
    }

    [Fact]
    public void SleepWallTimePreservesRecordedTimezoneAndHandlesInvalidInput()
    {
        var row = new OuraCombinedObjectV2("2026-10-22") { s_bedtime_start = "2026-10-22T23:30:10-08:00" };
        Assert.Equal("2026-10-22 23:30:10", row.BedtimeStartFormatLocal());
        row.s_bedtime_start = "short";
        Assert.Throws<OuraException>(() => row.BedtimeStartFormatLocal());
        row.s_bedtime_start = null;
        Assert.Equal("", row.BedtimeStartFormatLocal());
    }
    [Theory]
    [InlineData("{\"access_token\":\"a\",\"refresh_token\":\"r\",\"token_type\":\"Bearer\",\"expires_in\":0}")]
    [InlineData("{\"access_token\":\"a\",\"refresh_token\":\"r\",\"token_type\":\"Basic\",\"expires_in\":3600}")]
    [InlineData("{\"access_token\":\"a\",\"token_type\":\"Bearer\",\"expires_in\":3600}")]
    [InlineData("{\"access_token\":\"a\",\"refresh_token\":\"r\",\"token_type\":\"Bearer\",\"expires_in\":\"3600\"}")]
    [InlineData("{\"access_token\":\"a\",\"refresh_token\":\"r\",\"token_type\":\"Bearer\",\"expires_in\":31536001}")]
    public async Task InvalidTokenResponsesAreRejectedWithoutRetry(string body)
    {
        int posts = 0;
        using var handler = new Handler { Respond = (r, c) =>
        {
            posts++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) });
        } };
        using var http = new HttpClient(handler);
        await Assert.ThrowsAsync<OuraException>(() => new OuraOAuthClient(http).RefreshAsync(Account().Settings, "old-refresh", default));
        Assert.Equal(1, posts);
    }

    [Theory]
    [InlineData("https://attacker.example/")]
    [InlineData("http://localhost:80/oura/callback/")]
    [InlineData("http://localhost:48179/oura/callback")]
    [InlineData("http://localhost:48179/oura/callback/?extra=true")]
    public void InvalidRedirectsFailBeforeOpeningBrowser(string redirect)
    {
        var settings = Account().Settings; settings.RedirectUri = redirect;
        Assert.Throws<OuraException>(() => new OuraAuthorization(settings));
    }

    [Fact]
    public async Task PageLimitStopsBeforeSendingAnotherRequest()
    {
        int requests = 0;
        await Assert.ThrowsAsync<OuraException>(() => OuraCollectionClient.FetchAsync(OuraCollections.Get("ring_configuration"),
            Now.Date, Now.Date, default, null, TimeZoneInfo.Utc, (u, c) =>
            {
                requests++;
                return Task.FromResult("{\"data\":[{\"id\":\"a\"}],\"next_token\":\"more\"}");
            }, new OuraFetchOptions(100, 1)));
        Assert.Equal(1, requests);
    }

    [Theory]
    [InlineData("{\"data\":[{\"id\":\"a\"}],\"next_token\":42}")]
    [InlineData("{\"data\":[null]}")]
    [InlineData("{\"data\":[{\"not_an_id\":1}]}")]
    [InlineData("{\"data\":[] ,\"data\":[]}")]
    public async Task InvalidPagesFailBeforeReturningData(string body) =>
        await Assert.ThrowsAsync<OuraException>(() => OuraCollectionClient.FetchAsync(OuraCollections.Get("ring_configuration"),
            Now.Date, Now.Date, default, null, TimeZoneInfo.Utc, (u, c) => Task.FromResult(body), new OuraFetchOptions()));

    [Fact]
    public void ReversedDatesAndExtremeDatesFailWithoutOverflow()
    {
        var collection = OuraCollections.Get("heartrate");
        Assert.Throws<OuraException>(() => OuraCollectionClient.Windows(collection, Now.Date.AddDays(1), Now.Date).ToList());
        Assert.Throws<OuraException>(() => OuraCollectionClient.Windows(collection, Now.Date, DateTime.MaxValue).ToList());
    }

    [Fact]
    public void CombinedDailyExportIncludesBlankDaysAndSelectsMainSleep()
    {
        var result = OuraAPI2Wrapper.Combine(Now.Date, Now.Date.AddDays(1), new[] {
            new OuraAPIInterface.Sleep.Datum { id = "nap", day = "2026-10-22", type = "late_nap", total_sleep_duration = 1000 },
            new OuraAPIInterface.Sleep.Datum { id = "main", day = "2026-10-22", type = "long_sleep", total_sleep_duration = 20000 }
        }, Array.Empty<OuraAPIInterface.DailySleep.Datum>(), Array.Empty<OuraAPIInterface.DailyActivity.Datum>(),
            Array.Empty<OuraAPIInterface.DailyReadiness.Datum>(), default);
        Assert.Equal(2, result.Count);
        Assert.Equal("main", result[0].s_id);
        Assert.Null(result[1].s_total_sleep_duration);
        Assert.Equal("2026-10-23", result[1].SummaryDate);
    }

    [Fact]
    public async Task PreCancelledGetDoesNotReadTokenOrSendRequest()
    {
        using var handler = new Handler { Respond = (r, c) => throw new Exception("Unexpected request") };
        using var http = new HttpClient(handler);
        var transport = new OuraApiTransport(http, (f, r, c) => throw new Exception("Unexpected credential access"));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => transport.GetAsync(
            "https://api.ouraring.com/v2/usercollection/sleep", new CancellationToken(true)));
    }

    [Fact]
    public async Task ResponseBodyCancellationIsHonoured()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        using var content = new ByteArrayContent(Encoding.UTF8.GetBytes("{}"));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => OuraHttpContent.ReadAsync(content, 100, cancellation.Token));
    }

    [Fact]
    public void CancelledRefreshLeavesPendingMarker()
    {
        var stored = Account();
        var manager = new OuraTokenManager(() => Copy(stored), a => stored = Copy(a),
            (s, r, c) => throw new OperationCanceledException(), () => Now);
        Assert.Throws<OperationCanceledException>(() => manager.GetAccessToken(true, null, default));
        Assert.True(stored.RefreshPending);
    }

    [Fact]
    public void ExportWithNoFieldsOrInvalidFieldDefinitionFailsBeforeAllocation()
    {
        Assert.Throws<OuraException>(() => OuraExportTable.Build(new[] { new OuraRecord(new JObject()) }, Array.Empty<OuraFields>(), false));
        var invalid = OuraFieldSelections.Clone(OuraFields.CurrentFields()[0]); invalid.MethodName = "MissingProperty";
        Assert.Throws<OuraException>(() => OuraExportTable.Build(new[] { new OuraCombinedObjectV2("2026-10-22") }, new[] { invalid }, false));
    }
}
