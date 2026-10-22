using ExcelOuraVSTOAddIn;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OuraAPIInterface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

internal static class CollectionTests
{
    public static int Run()
    {
        int passed = 0;
        Action<bool, string> check = (ok, name) => { if (!ok) throw new Exception(name); passed++; Console.WriteLine("PASS " + name); };
        Action<Action, string> reject = (action, name) => { try { action(); } catch (InvalidOperationException) { check(true, name); return; } throw new Exception("Expected rejection: " + name); };
        var fixtures = ParseJson(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixtures", "oura-sandbox-exports.json")));
        var schemas = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixtures", "oura-export-schemas.json")));
        var schemaNames = new Dictionary<string, string> {
            { "heartrate", "PublicHeartRateRow" }, { "session", "PublicSession" }, { "sleep", "PublicModifiedSleepModel" },
            { "daily_stress", "PublicDailyStress" }, { "daily_resilience", "DailyResilienceModel" }, { "sleep_time", "PublicSleepTime" },
            { "rest_mode_period", "PublicRestModePeriod" }, { "daily_cardiovascular_age", "PublicDailyCardiovascularAge" },
            { "vO2_max", "PublicVO2Max" }, { "ring_configuration", "PublicRingConfiguration" }, { "ring_battery_level", "PublicRingBatteryLevelRow" }
        };
        check(OuraCollections.All.Select(c => c.Endpoint).Distinct().Count() == OuraCollections.All.Count, "One export definition per endpoint");
        foreach (var collection in OuraCollections.All)
        {
            check(collection.Fields().Any(f => f.FieldSelected), collection.Label + " has selected default fields");
            if (schemaNames.ContainsKey(collection.Endpoint))
                foreach (var field in collection.Fields())
                    check(ResolvePath(schemas, schemas[schemaNames[collection.Endpoint]], field.MethodName), collection.Label + " schema field " + field.MethodName);
            var sampleDay = collection.DateMode == OuraDateMode.Daily ? ((string)fixtures[collection.Endpoint]?["data"]?[0]?["day"] ?? (string)fixtures[collection.Endpoint]?["data"]?[0]?["start_day"]) : null;
            var start = sampleDay == null ? new DateTime(2026, 10, 1) : DateTime.ParseExact(sampleDay, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var rows = OuraCollectionClient.Fetch(collection, start, start.AddDays(1), CancellationToken.None, null, TimeZoneInfo.Utc,
                (url, cancel) => {
                    check(url.StartsWith("https://api.ouraring.com/v2/usercollection/" + collection.Endpoint, StringComparison.Ordinal), collection.Label + " official API URL");
                    return fixtures[collection.Endpoint].ToString(Formatting.None);
                });
            check(rows.Count > 0, collection.Label + " exports sandbox fixture rows");
            var table = OuraExportTable.Build(rows, collection.Fields(), true);
            check(table.GetLength(0) == rows.Count + 1 && table.GetLength(1) == collection.Fields().Count, collection.Label + " builds selectable bulk table");
            // Each export gets fresh field objects; cancelled or edited choices cannot leak to another run.
            collection.Fields()[0].CustomLabel = "temporary";
            check(collection.Fields()[0].CustomLabel == null, collection.Label + " fresh field selections");
        }
        var heart = OuraCollections.Get("heartrate");
        var windows = OuraCollectionClient.Windows(heart, new DateTime(2020, 1, 8), new DateTime(2026, 10, 22), TimeZoneInfo.Utc).ToList();
        check(windows.Count > 80 && windows.All(w => (w.EndExclusiveUtc.Value - w.StartUtc.Value).TotalDays <= 29), "Multi-year heart rate is split below the API 30-day limit");
        check(windows.First().StartUtc == new DateTimeOffset(2020, 1, 8, 0, 0, 0, TimeSpan.Zero) && windows.Last().EndExclusiveUtc == new DateTimeOffset(2026, 10, 23, 0, 0, 0, TimeSpan.Zero), "Heart-rate range includes every selected calendar day");
        check(windows.Zip(windows.Skip(1), (left, right) => left.EndExclusiveUtc == right.StartUtc).All(x => x), "Timestamp windows are contiguous without gaps");
        check(windows.All(w => w.Url.Contains("start_datetime=") && w.Url.Contains("end_datetime=") && !w.Url.Contains("9999999")), "Use API datetime parameters without seven-digit fractional boundaries");
        var tz = TimeZoneInfo.CreateCustomTimeZone("Fixture+10", TimeSpan.FromHours(10), "Fixture+10", "Fixture+10");
        var local = OuraCollectionClient.Windows(heart, new DateTime(2026, 10, 1), new DateTime(2026, 10, 1), tz).Single();
        check(local.StartUtc.Value.UtcDateTime == new DateTime(2026, 9, 30, 14, 0, 0, DateTimeKind.Utc), "Heart-rate day selection respects local timezone rather than assuming UTC midnight");
        TimeZoneInfo dst;
        try { dst = TimeZoneInfo.FindSystemTimeZoneById("Australia/Sydney"); }
        catch (TimeZoneNotFoundException) { dst = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"); }
        var dstWindow = OuraCollectionClient.Windows(heart, new DateTime(2026, 10, 4), new DateTime(2026, 10, 4), dst).Single();
        check((dstWindow.EndExclusiveUtc.Value - dstWindow.StartUtc.Value).TotalHours == 23, "Spring DST transition exports the actual 23-hour local day");
        var daily = OuraCollectionClient.Windows(OuraCollections.Get("daily_stress"), new DateTime(2026, 1, 1), new DateTime(2026, 3, 1), TimeZoneInfo.Utc).ToList();
        check(daily.Count == 3 && daily[0].Url.EndsWith("end_date=2026-01-30") && daily[1].Url.Contains("start_date=2026-01-30"), "Daily windows include the selected end day with an exclusive API upper bound");
        check(!OuraCollectionClient.Windows(OuraCollections.Get("ring_configuration"), DateTime.MinValue, DateTime.MinValue).Single().Url.Contains("?"), "Ring configuration omits unsupported date parameters");
        check(!OuraCollectionClient.Windows(OuraCollections.Get("personal_info"), DateTime.MinValue, DateTime.MinValue).Single().Url.Contains("?"), "Profile omits date parameters");
        // First window includes its end boundary; the following window owns that row.
        var first = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var boundary = first.AddDays(29);
        int calls = 0;
        var sampled = OuraCollectionClient.Fetch(heart, first.Date, boundary.Date, CancellationToken.None, null, TimeZoneInfo.Utc, (url, ct) => {
            calls++;
            if (calls == 1) return Page(Sample(first, 60), Sample(boundary, 70), "next+a/b=");
            if (calls == 2) {
                check(url.Contains("next_token=next%2Ba%2Fb%3D"), "Encode continuation token inside a timestamp window");
                return Page(Sample(first, 60), null, null);
            }
            return Page(Sample(boundary, 70), Sample(boundary.AddDays(1), 80), null);
        });
        check(calls == 3 && sampled.Count == 2, "Read all pages/windows and remove repeated or out-of-range boundary samples");
        int pageCalls = 0;
        var configurations = OuraCollectionClient.Fetch(OuraCollections.Get("ring_configuration"), DateTime.Today, DateTime.Today, CancellationToken.None, null, null, (url, ct) => {
            pageCalls++;
            if (pageCalls == 1) return "{\"data\":[{\"id\":\"one\"}],\"next_token\":\"next\"}";
            check(url.EndsWith("?next_token=next"), "Paginate undated collections with question-mark separator");
            return "{\"data\":[{\"id\":\"two\"}],\"next_token\":null}";
        });
        check(configurations.Count == 2, "Ring details include all paginated configurations");
        reject(() => OuraCollectionClient.Fetch(heart, first.Date, first.Date, CancellationToken.None, null, TimeZoneInfo.Utc,
            (url, ct) => "{\"data\":[],\"next_token\":\"repeat\"}"), "Repeated continuation token aborts a collection export");
        reject(() => OuraCollectionClient.Fetch(heart, first.Date, first.Date, CancellationToken.None, null, TimeZoneInfo.Utc,
            (url, ct) => "{\"data\":null}"), "Missing collection data aborts rather than returning partial results");
        reject(() => OuraCollectionClient.Fetch(heart, first.Date, first.Date, CancellationToken.None, null, TimeZoneInfo.Utc,
            (url, ct) => "{\"data\":[{\"timestamp\":\"invalid\",\"bpm\":60}]}"), "Invalid sample timestamp stops the export before Excel is touched");
        int failedCalls = 0;
        reject(() => OuraCollectionClient.Fetch(heart, first.Date, boundary.Date, CancellationToken.None, null, TimeZoneInfo.Utc, (url, ct) => {
            if (++failedCalls == 1) return Page(Sample(first, 60), null, null);
            throw new InvalidOperationException("simulated later-window failure");
        }), "Failure in a later window never returns earlier windows as a successful export");
        using (var cancel = new CancellationTokenSource())
        {
            cancel.Cancel(); int requested = 0;
            try { OuraCollectionClient.Fetch(heart, first.Date, first.Date, cancel.Token, null, TimeZoneInfo.Utc, (url, ct) => { requested++; return "{}"; }); throw new Exception("Expected cancellation"); }
            catch (OperationCanceledException) { check(requested == 0, "Cancel before fetch prevents any HTTP requests"); }
        }
        var session = new OuraRecord(ParseJson("{\"heart_rate\":{\"items\":[60,null,62]},\"heart_rate_variability\":null,\"start_datetime\":\"2026-10-22T12:00:00+11:00\"}"));
        var sessionTable = OuraExportTable.Build(new[] { session }, OuraCollections.Get("session").Fields(), false);
        check((string)sessionTable[0,5] == "60;;62" && sessionTable[0,6] == null, "Session arrays flatten with missing samples and optional HRV stays blank");
        check((string)sessionTable[0,1] == "2026-10-22T12:00:00+11:00", "New exports retain timestamp UTC offsets");
        var stress = new OuraRecord(JObject.Parse("{\"day\":\"2026-10-22\",\"stress_high\":0,\"recovery_high\":null}"));
        var stressTable = OuraExportTable.Build(new[] { stress }, OuraCollections.Get("daily_stress").Fields(), false);
        check(stressTable[0,2] is long && (long)stressTable[0,2] == 0 && stressTable[0,3] == null, "Additional exports distinguish measured zero from missing data");
        foreach (string scope in new[] { "stress", "heart_health", "ring_configuration", "personal", "email", "heartrate", "session" })
            check(OuraCollections.RequestedScopes.Split(' ').Contains(scope), "Request permission for " + scope + " exports on fresh sign-in");
        var trimmed = OuraCollectionClient.Fetch(OuraCollections.Get("daily_stress"), new DateTime(2026, 10, 1), new DateTime(2026, 10, 1), CancellationToken.None, null, TimeZoneInfo.Utc,
            (url, ct) => "{\"data\":[{\"id\":\"selected\",\"day\":\"2026-10-01\"},{\"id\":\"next-day\",\"day\":\"2026-10-02\"}],\"next_token\":null}");
        check(trimmed.Count == 1 && (string)trimmed[0].Read("id") == "selected", "Inclusive API responses cannot leak the following day into a selected-day export");
        using (var cancel = new CancellationTokenSource())
        {
            int requests = 0;
            try
            {
                OuraCollectionClient.Fetch(heart, first.Date, first.Date, cancel.Token, pages => cancel.Cancel(), TimeZoneInfo.Utc, (url, ct) => {
                    requests++;
                    check(ct == cancel.Token, "Pass export cancellation through to each HTTP request");
                    return Page(Sample(first, 60), null, "next");
                });
                throw new Exception("Expected mid-export cancellation");
            }
            catch (OperationCanceledException) { check(requests == 1, "Cancellation after a page prevents the next page and prevents partial success"); }
        }
        return passed;
    }
    private static JObject ParseJson(string json) => JsonConvert.DeserializeObject<JObject>(json, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
    private static JObject Sample(DateTimeOffset stamp, int bpm) => new JObject { ["timestamp"] = stamp.ToString("o", CultureInfo.InvariantCulture), ["bpm"] = bpm, ["source"] = "awake" };
    private static string Page(JObject one, JObject two, string next)
    {
        var rows = new JArray(); if (one != null) rows.Add(one); if (two != null) rows.Add(two);
        return new JObject { ["data"] = rows, ["next_token"] = next }.ToString(Formatting.None);
    }
    private static bool ResolvePath(JObject schemas, JToken schema, string path)
    {
        foreach (string part in path.Split('.'))
        {
            while (true)
            {
                if (schema["$ref"] != null) { schema = schemas[((string)schema["$ref"]).Split('/').Last()]; continue; }
                if (schema["anyOf"] != null) { schema = schema["anyOf"].First(x => (string)x["type"] != "null"); continue; }
                break;
            }
            schema = schema["properties"]?[part];
            if (schema == null) return false;
        }
        return true;
    }
}
