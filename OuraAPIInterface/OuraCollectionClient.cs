using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OuraAPIInterface
{
    public sealed class OuraFetchOptions
    {
        public int MaximumRows { get; }
        public int MaximumPages { get; }

        public OuraFetchOptions(int maximumRows = OuraExportLimits.MaximumRows, int maximumPages = 20000)
        {
            if (maximumRows < 1 || maximumRows > OuraExportLimits.MaximumRows) throw new ArgumentOutOfRangeException(nameof(maximumRows));
            if (maximumPages < 1 || maximumPages > 20000) throw new ArgumentOutOfRangeException(nameof(maximumPages));
            MaximumRows = maximumRows;
            MaximumPages = maximumPages;
        }
    }

    public sealed class OuraRequestWindow
    {
        public string Url { get; internal set; }
        public DateTime? StartDay { get; internal set; }
        public DateTime? EndExclusiveDay { get; internal set; }
        public DateTimeOffset? StartUtc { get; internal set; }
        public DateTimeOffset? EndExclusiveUtc { get; internal set; }
    }

    public static class OuraCollectionClient
    {
        public const int WindowDays = 29;

        public static IEnumerable<OuraRequestWindow> Windows(OuraCollection collection, DateTime start, DateTime end, TimeZoneInfo timezone = null)
        {
            if (!OuraCollections.IsKnown(collection)) throw new ArgumentException("A known Oura export is required.", nameof(collection));
            string url = OuraAPI2Wrapper.BaseURL() + "/v2/usercollection/" + collection.Endpoint;
            if (collection.DateMode == OuraDateMode.All || collection.DateMode == OuraDateMode.Single)
            {
                yield return new OuraRequestWindow { Url = url };
                yield break;
            }

            OuraArguments.ValidateDateRange(start, end);
            if (collection.DateMode == OuraDateMode.Timestamp)
            {
                timezone = timezone ?? TimeZoneInfo.Local;
                var cursor = StartOfDayUtc(start.Date, timezone);
                var limit = StartOfDayUtc(end.Date.AddDays(1), timezone);
                while (cursor < limit)
                {
                    var stop = (limit - cursor).TotalDays > WindowDays ? cursor.AddDays(WindowDays) : limit;
                    yield return new OuraRequestWindow
                    {
                        StartUtc = cursor,
                        EndExclusiveUtc = stop,
                        Url = url + "?start_datetime=" + EncodeUtc(cursor) + "&end_datetime=" + EncodeUtc(stop)
                    };
                    cursor = stop;
                }
            }
            else
            {
                var limit = end.Date.AddDays(1);
                for (var cursor = start.Date; cursor < limit;)
                {
                    var stop = cursor.AddDays(Math.Min(WindowDays, (limit - cursor).Days));
                    yield return new OuraRequestWindow
                    {
                        StartDay = cursor,
                        EndExclusiveDay = stop,
                        Url = url + "?start_date=" + cursor.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) +
                            "&end_date=" + stop.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    };
                    cursor = stop;
                }
            }
        }

        private static DateTimeOffset StartOfDayUtc(DateTime day, TimeZoneInfo timezone)
        {
            try
            {
                return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(day, DateTimeKind.Unspecified), timezone));
            }
            catch (ArgumentException)
            {
                throw new OuraException("The selected calendar date cannot be represented in this timezone. Choose an adjacent valid date.");
            }
        }

        private static string EncodeUtc(DateTimeOffset value) =>
            Uri.EscapeDataString(value.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture));

        public static List<OuraRecord> Fetch(OuraCollection collection, DateTime start, DateTime end, CancellationToken cancellation,
            Action<int> pageProgress = null, TimeZoneInfo timezone = null)
            => FetchAsync(collection, start, end, cancellation, pageProgress, timezone).GetAwaiter().GetResult();

        internal static List<OuraRecord> Fetch(OuraCollection collection, DateTime start, DateTime end, CancellationToken cancellation,
            Action<int> pageProgress, TimeZoneInfo timezone, Func<string, CancellationToken, string> request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return FetchAsync(collection, start, end, cancellation, pageProgress, timezone,
                (url, token) => Task.FromResult(request(url, token)), new OuraFetchOptions()).GetAwaiter().GetResult();
        }

        public static Task<List<OuraRecord>> FetchAsync(OuraCollection collection, DateTime start, DateTime end, CancellationToken cancellation,
            Action<int> pageProgress = null, TimeZoneInfo timezone = null, OuraFetchOptions options = null)
            => FetchAsync(collection, start, end, cancellation, pageProgress, timezone,
                OuraAPI2Wrapper.MakeRequestStringAsync, options ?? new OuraFetchOptions());

        internal static async Task<List<OuraRecord>> FetchAsync(OuraCollection collection, DateTime start, DateTime end,
            CancellationToken cancellation, Action<int> pageProgress, TimeZoneInfo timezone,
            Func<string, CancellationToken, Task<string>> request, OuraFetchOptions options)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (options == null) throw new ArgumentNullException(nameof(options));
            var result = new List<OuraRecord>();
            var identities = new HashSet<string>(StringComparer.Ordinal);
            int pages = 0;

            foreach (var window in Windows(collection, start, end, timezone))
            {
                string next = null;
                var seen = new HashSet<string>(StringComparer.Ordinal);
                do
                {
                    cancellation.ThrowIfCancellationRequested();
                    if (pages >= options.MaximumPages) throw new OuraException("The export exceeded the safe page limit. Select a shorter date range.");
                    string url = window.Url + (String.IsNullOrEmpty(next) ? "" : (window.Url.Contains("?") ? "&" : "?") + "next_token=" + Uri.EscapeDataString(next));
                    JObject response;
                    try { response = OuraJson.ParseObject(await request(url, cancellation).ConfigureAwait(false)); }
                    catch (JsonException) { throw new OuraException("Oura returned malformed " + collection.Label + " data. The export has stopped."); }

                    var data = collection.DateMode == OuraDateMode.Single ? (IEnumerable<JToken>)new[] { response } :
                        response["data"] as JArray ?? throw new OuraException("Oura returned a page without data. The export has stopped.");
                    foreach (var item in data.ToArray())
                    {
                        cancellation.ThrowIfCancellationRequested();
                        var json = item as JObject;
                        if (json == null) throw new OuraException("Oura returned an invalid row. The export has stopped.");
                        if (json.Parent != null) json.Remove();
                        var record = new OuraRecord(json, true);
                        string identity = IdentityInWindow(record, collection, window);
                        if (identity == null || !identities.Add(identity)) continue;
                        if (result.Count >= options.MaximumRows)
                            throw new OuraException("The export contains more rows than the selected worksheet area or safe memory limit allows. Use fewer fields or a shorter date range.");
                        result.Add(record);
                    }

                    JToken continuation = response["next_token"];
                    if (continuation != null && continuation.Type != JTokenType.Null && continuation.Type != JTokenType.String)
                        throw new OuraException("Oura returned an invalid continuation token. The export has stopped.");
                    next = collection.DateMode == OuraDateMode.Single ? null : (string)continuation;
                    if (next != null && (next.Length > 8192 || !seen.Add(next)))
                        throw new OuraException("Oura returned an invalid or repeated continuation token. The export has stopped.");
                    pages++;
                    pageProgress?.Invoke(pages);
                } while (!String.IsNullOrEmpty(next));
            }
            cancellation.ThrowIfCancellationRequested();
            return result;
        }

        private static string IdentityInWindow(OuraRecord record, OuraCollection collection, OuraRequestWindow window)
        {
            if (collection.DateMode == OuraDateMode.Timestamp)
            {
                DateTimeOffset stamp;
                string timestamp = record.Read("timestamp") as string;
                if (timestamp == null || !(timestamp.EndsWith("Z", StringComparison.OrdinalIgnoreCase) ||
                    timestamp.LastIndexOf('+') > 9 || timestamp.LastIndexOf('-') > 9) ||
                    !DateTimeOffset.TryParse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.None, out stamp))
                    throw new OuraException("Oura returned an invalid sample timestamp. The export has stopped.");
                if (stamp < window.StartUtc.Value || stamp >= window.EndExclusiveUtc.Value) return null;
                return stamp.UtcTicks.ToString(CultureInfo.InvariantCulture) + ":" + record.Read("bpm") + ":" + record.Read("source") +
                    ":" + record.Read("level") + ":" + record.Read("charging") + ":" + record.Read("in_charger");
            }

            string id = record.Read("id") as string;
            if (String.IsNullOrWhiteSpace(id)) throw new OuraException("Oura returned a row without its required identifier. The export has stopped.");
            if (window.StartDay.HasValue)
            {
                if (collection.Endpoint == "rest_mode_period" || collection.Endpoint == "enhanced_tag")
                {
                    DateTime start = ReadDay(record, "start_day");
                    DateTime? end = record.Read("end_day") == null ? (DateTime?)null : ReadDay(record, "end_day");
                    if (end < start) throw new OuraException("Oura returned invalid rest-mode dates. The export has stopped.");
                    if (start >= window.EndExclusiveDay.Value || end < window.StartDay.Value) return null;
                }
                else
                {
                    DateTime day = ReadDay(record, "day");
                    if (day < window.StartDay.Value || day >= window.EndExclusiveDay.Value) return null;
                }
            }
            return id;
        }

        private static DateTime ReadDay(OuraRecord record, string field)
        {
            DateTime day;
            if (!DateTime.TryParseExact(record.Read(field) as string, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out day))
                throw new OuraException("Oura returned an invalid day. The export has stopped.");
            return day;
        }
    }
}
