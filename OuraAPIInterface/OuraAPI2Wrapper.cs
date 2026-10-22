using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OuraAPIInterface
{
    /// <summary>Compatibility facade over the shared, cancellable API transport and collection reader.</summary>
    public static class OuraAPI2Wrapper
    {
        private static string ouraToken;
        private static readonly HttpClient Http = OuraHttpClient.Create();
        public static Func<bool, string> AccessTokenProvider { get; set; }
        public static Func<bool, string, CancellationToken, Task<string>> AccessTokenProviderAsync { get; set; }

        public static string APIToken() => Volatile.Read(ref ouraToken);
        public static void APIToken(string token) => Volatile.Write(ref ouraToken, token);
        public static string BaseURL() => "https://api.ouraring.com";

        private static Task<string> ResolveTokenAsync(bool refresh, string rejected, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            var asynchronous = AccessTokenProviderAsync;
            if (asynchronous != null) return asynchronous(refresh, rejected, cancellation);
            var synchronous = AccessTokenProvider;
            return Task.FromResult(synchronous != null ? synchronous(refresh) : APIToken());
        }

        public static UserInfoResponse PerformAuthenticationV2(string apiToken)
        {
            var transport = new OuraApiTransport(Http, force => apiToken);
            return Deserialize<UserInfoResponse>(transport.GetAsync(BaseURL() + "/v2/usercollection/personal_info", CancellationToken.None).GetAwaiter().GetResult());
        }

        public static UserInfoResponse PerformAuthenticationV2() => MakeRequest<UserInfoResponse>(BaseURL() + "/v2/usercollection/personal_info");

        public static DailyActivity.DailyActivityResponseV2 PerformDailyActivityV2(DateTime startDate, DateTime endDate) =>
            new DailyActivity.DailyActivityResponseV2 { data = FetchTypedAsync<DailyActivity.Datum>("daily_activity", startDate, endDate, CancellationToken.None).GetAwaiter().GetResult() };

        public static DailyReadiness.DailyReadinessResponseV2 PerformDailyReadinessV2(DateTime startDate, DateTime endDate) =>
            new DailyReadiness.DailyReadinessResponseV2 { data = FetchTypedAsync<DailyReadiness.Datum>("daily_readiness", startDate, endDate, CancellationToken.None).GetAwaiter().GetResult() };

        public static DailySleep.DailySleepResponseV2 PerformDailySleepV2(DateTime startDate, DateTime endDate) =>
            new DailySleep.DailySleepResponseV2 { data = FetchTypedAsync<DailySleep.Datum>("daily_sleep", startDate, endDate, CancellationToken.None).GetAwaiter().GetResult() };

        public static DailySpo2.DailySpo2ResponseV2 PerformDailySpo2V2(DateTime startDate, DateTime endDate) =>
            new DailySpo2.DailySpo2ResponseV2 { data = FetchTypedAsync<DailySpo2.Datum>("daily_spo2", startDate, endDate, CancellationToken.None).GetAwaiter().GetResult() };

        public static HeartRates.HeartRateResponseV2 PerformHearRateV2(DateTime startDate, DateTime endDate) => PerformHeartRateV2(startDate, endDate);

        public static HeartRates.HeartRateResponseV2 PerformHeartRateV2(DateTime startDate, DateTime endDate) =>
            new HeartRates.HeartRateResponseV2 { data = FetchTypedAsync<HeartRates.Datum>("heartrate", startDate, endDate, CancellationToken.None).GetAwaiter().GetResult() };

        public static Sessions.SessionsResponseV2 PerformSessionsV2(DateTime startDate, DateTime endDate) =>
            new Sessions.SessionsResponseV2 { data = FetchTypedAsync<Sessions.Datum>("session", startDate, endDate, CancellationToken.None).GetAwaiter().GetResult() };

        public static Sleep.SleepResponseV2 PerformSleepV2(DateTime startDate, DateTime endDate) =>
            new Sleep.SleepResponseV2 { data = FetchTypedAsync<Sleep.Datum>("sleep", startDate, endDate, CancellationToken.None).GetAwaiter().GetResult() };

        public static EnhancedTags.EnhancedTagsResponseV2 PerformEnhancedTagV2(DateTime startDate, DateTime endDate) =>
            new EnhancedTags.EnhancedTagsResponseV2 { data = FetchTypedAsync<EnhancedTags.Datum>("enhanced_tag", startDate, endDate, CancellationToken.None).GetAwaiter().GetResult() };

        public static EnhancedTags.EnhancedTagSingleResponseV2 PerformEnhancedTagSingleV2(string tagId)
        {
            if (String.IsNullOrWhiteSpace(tagId)) throw new ArgumentException("A tag identifier is required.", nameof(tagId));
            return MakeRequest<EnhancedTags.EnhancedTagSingleResponseV2>(BaseURL() + "/v2/usercollection/enhanced_tag/" + Uri.EscapeDataString(tagId));
        }

        public static Tags.TagResponseV2 PerformTagV2(DateTime startDate, DateTime endDate) =>
            new Tags.TagResponseV2 { data = FetchTypedAsync<Tags.Datum>("tag", startDate, endDate, CancellationToken.None).GetAwaiter().GetResult() };

        public static Workouts.WorkoutsResponseV2 PerformWorkoutV2(DateTime startDate, DateTime endDate) =>
            new Workouts.WorkoutsResponseV2 { data = FetchTypedAsync<Workouts.Datum>("workout", startDate, endDate, CancellationToken.None).GetAwaiter().GetResult() };

        public static async Task<List<T>> FetchTypedAsync<T>(string endpoint, DateTime start, DateTime end, CancellationToken cancellation,
            Action<int> pageProgress = null, OuraFetchOptions options = null)
        {
            var records = await OuraCollectionClient.FetchAsync(OuraCollections.Get(endpoint), start, end, cancellation, pageProgress,
                options: options).ConfigureAwait(false);
            var result = new List<T>(records.Count);
            try
            {
                foreach (var record in records)
                {
                    cancellation.ThrowIfCancellationRequested();
                    result.Add(record.Json.ToObject<T>());
                }
            }
            catch (JsonException) { throw new OuraException("Oura returned data this version could not read. Update the add-in or try again later."); }
            return result;
        }

        public static T MakeRequest<T>(string requestUrl) => Deserialize<T>(MakeRequestString(requestUrl));
        public static string MakeRequestString(string requestUrl) => MakeRequestString(requestUrl, CancellationToken.None);
        public static string MakeRequestString(string requestUrl, CancellationToken cancellation) => MakeRequestStringAsync(requestUrl, cancellation).GetAwaiter().GetResult();
        public static Task<string> MakeRequestStringAsync(string requestUrl, CancellationToken cancellation) =>
            new OuraApiTransport(Http, ResolveTokenAsync).GetAsync(requestUrl, cancellation);

        private static T Deserialize<T>(string json)
        {
            try { return OuraJson.ParseObject(json).ToObject<T>(); }
            catch (JsonException) { throw new OuraException("Oura returned data this version could not read. Update the add-in or try again later."); }
        }

        public static List<OuraCombinedObjectV2> GetAllOuraDataV2ForDateRange(DateTime startDate, DateTime endDate) =>
            GetAllOuraDataV2ForDateRangeAsync(startDate, endDate, CancellationToken.None).GetAwaiter().GetResult();

        public static async Task<List<OuraCombinedObjectV2>> GetAllOuraDataV2ForDateRangeAsync(DateTime start, DateTime end,
            CancellationToken cancellation, Action<int> pageProgress = null, OuraFetchOptions options = null)
        {
            OuraArguments.ValidateDateRange(start, end);
            options = options ?? new OuraFetchOptions();
            if ((end.Date - start.Date).TotalDays + 1 > options.MaximumRows)
                throw new OuraException("The selected days will not fit in the worksheet area. Use a shorter date range.");
            int completedPages = 0;
            Action<int> progress = ignored => pageProgress?.Invoke(++completedPages);
            // Sequential reads limit per-token load. All four must succeed before a combined row is returned.
            var dailySleep = await FetchTypedAsync<DailySleep.Datum>("daily_sleep", start, end, cancellation, progress, options).ConfigureAwait(false);
            var sleep = await FetchTypedAsync<Sleep.Datum>("sleep", start, end, cancellation, progress).ConfigureAwait(false);
            var activity = await FetchTypedAsync<DailyActivity.Datum>("daily_activity", start, end, cancellation, progress, options).ConfigureAwait(false);
            var readiness = await FetchTypedAsync<DailyReadiness.Datum>("daily_readiness", start, end, cancellation, progress, options).ConfigureAwait(false);
            return Combine(start, end, sleep, dailySleep, activity, readiness, cancellation);
        }

        internal static List<OuraCombinedObjectV2> Combine(DateTime start, DateTime end, IEnumerable<Sleep.Datum> sleep,
            IEnumerable<DailySleep.Datum> dailySleep, IEnumerable<DailyActivity.Datum> activity,
            IEnumerable<DailyReadiness.Datum> readiness, CancellationToken cancellation)
        {
            OuraArguments.ValidateDateRange(start, end);
            if (sleep == null || dailySleep == null || activity == null || readiness == null)
                throw new ArgumentNullException("collections");
            cancellation.ThrowIfCancellationRequested();
            OuraExportLimits.Validate((end.Date - start.Date).Days + 1, 1);
            var sleepByDay = sleep.GroupBy(s => s.day).ToDictionary(g => g.Key, g => MainSleepForDay(g, g.Key), StringComparer.Ordinal);
            var dailyByDay = dailySleep.GroupBy(s => s.day).ToDictionary(g => g.Key, g => g.OrderBy(s => s.timestamp).Last(), StringComparer.Ordinal);
            var activityByDay = activity.GroupBy(s => s.day).ToDictionary(g => g.Key, g => g.OrderBy(s => s.timestamp).Last(), StringComparer.Ordinal);
            var readinessByDay = readiness.GroupBy(s => s.day).ToDictionary(g => g.Key, g => g.OrderBy(s => s.timestamp).Last(), StringComparer.Ordinal);
            var result = new List<OuraCombinedObjectV2>();
            for (var day = start.Date; day <= end.Date; day = day.AddDays(1))
            {
                cancellation.ThrowIfCancellationRequested();
                string key = day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                Sleep.Datum s;
                DailySleep.Datum ds;
                DailyActivity.Datum a;
                DailyReadiness.Datum r;
                sleepByDay.TryGetValue(key, out s);
                dailyByDay.TryGetValue(key, out ds);
                activityByDay.TryGetValue(key, out a);
                readinessByDay.TryGetValue(key, out r);
                var combined = new OuraCombinedObjectV2(key);
                combined.UpdateFrom(s, ds, r, a);
                result.Add(combined);
            }
            return result;
        }

        internal static Sleep.Datum MainSleepForDay(IEnumerable<Sleep.Datum> sleeps, string day)
        {
            if (sleeps == null) throw new ArgumentNullException(nameof(sleeps));
            return sleeps.Where(s => s != null && s.day == day && s.type != "deleted" && s.type != "rest")
                .OrderByDescending(s => s.type == "long_sleep")
                .ThenByDescending(s => s.total_sleep_duration ?? 0)
                .ThenByDescending(s => ParseTimestamp(s.bedtime_end)).FirstOrDefault();
        }

        private static DateTimeOffset ParseTimestamp(string value)
        {
            DateTimeOffset parsed;
            return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed) ? parsed : DateTimeOffset.MinValue;
        }
    }
}
