using ExcelOuraVSTOAddIn;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OuraAPIInterface
{
    public enum OuraDateMode { Daily, Timestamp, All, Single }

    public sealed class OuraRecord
    {
        internal JObject Json { get; }
        public OuraRecord(JObject json) : this(json, false) { }
        internal OuraRecord(JObject json, bool takeOwnership)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            Json = takeOwnership ? json : (JObject)json.DeepClone();
        }
        public object Read(string path)
        {
            if (String.IsNullOrWhiteSpace(path)) throw new ArgumentException("A field path is required.", nameof(path));
            JToken token = Json;
            foreach (string segment in path.Split('.'))
            {
                if (segment.Length == 0 || !segment.All(c => Char.IsLetterOrDigit(c) || c == '_'))
                    throw new ArgumentException("Invalid field path.", nameof(path));
                token = (token as JObject)?[segment];
            }
            return Value(token);
        }
        private static object Value(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null) return null;
            var scalar = token as JValue;
            if (scalar != null) return scalar.Value;
            var array = token as JArray;
            if (array != null) return array.Select(Value).ToArray();
            return token.ToString(Formatting.None);
        }
    }

    public sealed class OuraCollection
    {
        public string Endpoint { get; }
        public string Label { get; }
        public string Group { get; }
        public string Scope { get; }
        public OuraDateMode DateMode { get; }
        private readonly List<OuraFields> fields = new List<OuraFields>();
        internal OuraCollection(string endpoint, string label, string group, string scope, OuraDateMode dates = OuraDateMode.Daily)
        { Endpoint = endpoint; Label = label; Group = group; Scope = scope; DateMode = dates; }
        internal OuraCollection Field(string path, string label, string description, bool selected = true)
        {
            fields.Add(new OuraFields(label, Label, path, fields.Count + 1, true, OuraFields.AccessorType.Property, description, selected));
            return this;
        }
        public List<OuraFields> Fields() => fields.Select(f => new OuraFields(f.FieldName, f.OuraSection, f.MethodName, f.FieldOrder, true, f.Accessor, f.FieldDescription, f.FieldSelected)).ToList();
    }

    public static class OuraCollections
    {
        public static readonly IReadOnlyList<OuraCollection> All = new List<OuraCollection> {
            new OuraCollection("heartrate", "Heart Rate", "Oura Commands", "heartrate", OuraDateMode.Timestamp)
                .Field("timestamp", "Time Stamp", "Sample time including its UTC offset; date selection uses this computer's local calendar days.")
                .Field("bpm", "BPM", "Heart rate in beats per minute.").Field("source", "Source", "Measurement source, such as awake, sleep or workout."),
            new OuraCollection("session", "Sessions", "Oura Commands", "session")
                .Field("day", "Day", "Oura session day.").Field("start_datetime", "Start", "Session start with UTC offset.")
                .Field("end_datetime", "End", "Session end with UTC offset.").Field("type", "Type", "Session type.").Field("mood", "Mood", "Optional user-selected mood.")
                .Field("heart_rate.items", "Heart Rate Samples", "Semicolon-separated beats per minute; missing samples are empty.")
                .Field("heart_rate_variability.items", "HRV Samples", "Semicolon-separated HRV samples in milliseconds.")
                .Field("heart_rate.interval", "HR Interval (seconds)", "Seconds between heart rate samples.")
                .Field("heart_rate.timestamp", "HR Sample Start", "Timestamp of the first heart rate sample.")
                .Field("heart_rate_variability.interval", "HRV Interval (seconds)", "Seconds between HRV samples.")
                .Field("heart_rate_variability.timestamp", "HRV Sample Start", "Timestamp of the first HRV sample.")
                .Field("motion_count.items", "Motion Samples", "Semicolon-separated motion counts.", false)
                .Field("motion_count.interval", "Motion Interval (seconds)", "Seconds between motion samples.", false)
                .Field("motion_count.timestamp", "Motion Sample Start", "Timestamp of the first motion sample.", false).Field("id", "ID", "Oura session identifier.", false),
            new OuraCollection("sleep", "Sleep Periods", "Oura Commands", "daily")
                .Field("day", "Day", "Oura sleep day; each sleep/nap is a separate row.").Field("type", "Type", "Sleep/nap/rest classification; all periods are retained.")
                .Field("bedtime_start", "Bedtime Start", "Start with UTC offset.").Field("bedtime_end", "Bedtime End", "End with UTC offset.")
                .Field("total_sleep_duration", "Total Sleep (seconds)", "Sleep duration in seconds.")
                .Field("deep_sleep_duration", "Deep Sleep (seconds)", "Deep sleep in seconds.").Field("rem_sleep_duration", "REM Sleep (seconds)", "REM sleep in seconds.")
                .Field("light_sleep_duration", "Light Sleep (seconds)", "Light sleep in seconds.").Field("awake_time", "Awake (seconds)", "Awake duration in seconds.")
                .Field("average_heart_rate", "Average Heart Rate", "API sleep average in beats per minute.").Field("average_hrv", "Average HRV (ms)", "Average heart rate variability.")
                .Field("average_breath", "Average Breath", "Breaths per minute.").Field("efficiency", "Efficiency", "Sleep efficiency rating, 1–100.")
                .Field("latency", "Latency (seconds)", "Time before falling asleep.").Field("sleep_algorithm_version", "Algorithm Version", "Algorithm that generated this record.")
                .Field("id", "ID", "Oura sleep-period identifier."),
            new OuraCollection("daily_stress", "Daily Stress", "Wellbeing", "stress")
                .Field("day", "Day", "Oura day.").Field("day_summary", "Summary", "Stress/restoration summary.")
                .Field("stress_high", "High Stress (seconds)", "Time in high stress in seconds, not minutes.")
                .Field("recovery_high", "High Recovery (seconds)", "Time in high recovery in seconds."),
            new OuraCollection("daily_resilience", "Resilience", "Wellbeing", "daily")
                .Field("day", "Day", "Oura day.").Field("level", "Level", "Resilience level.")
                .Field("contributors.sleep_recovery", "Sleep Recovery", "Sleep recovery contribution, 0–100.")
                .Field("contributors.daytime_recovery", "Daytime Recovery", "Daytime recovery contribution, 0–100.")
                .Field("contributors.stress", "Stress", "Stress contribution, 0–100."),
            new OuraCollection("sleep_time", "Bedtime Guidance", "Wellbeing", "daily")
                .Field("day", "Day", "Oura day.").Field("recommendation", "Recommendation", "Suggested bedtime action.").Field("status", "Status", "Sleep-time status.")
                .Field("optimal_bedtime.start_offset", "Window Start (seconds)", "Seconds from local midnight; offsets may extend outside the calendar day.")
                .Field("optimal_bedtime.end_offset", "Window End (seconds)", "Seconds from local midnight.")
                .Field("optimal_bedtime.day_tz", "Timezone Offset (seconds)", "Offset from GMT for this day; preserve alongside the window offsets."),
            new OuraCollection("rest_mode_period", "Rest Mode", "Wellbeing", "daily")
                .Field("start_day", "Start Day", "Date rest mode began.").Field("end_day", "End Day", "Date rest mode ended; blank for an ongoing period.")
                .Field("start_time", "Start Time", "Optional start with UTC offset.").Field("end_time", "End Time", "Optional end with UTC offset.")
                .Field("episodes", "Episodes", "Semicolon-separated JSON episodes, including tags and timestamps.").Field("id", "ID", "Rest-mode period identifier.", false),
            new OuraCollection("daily_cardiovascular_age", "Cardiovascular Age", "Heart Health", "heart_health")
                .Field("day", "Day", "Oura day.").Field("vascular_age", "Vascular Age (years)", "Oura cardiovascular age estimate in years.")
                .Field("pulse_wave_velocity", "Pulse Wave Velocity (m/s)", "Pulse wave velocity in metres per second."),
            new OuraCollection("vO2_max", "VO2 Max", "Heart Health", "heart_health")
                .Field("day", "Day", "Estimate day.").Field("timestamp", "Timestamp", "Estimate creation time with UTC offset.")
                .Field("vo2_max", "VO2 Max", "Oura VO2 max estimate (mL/kg/min)."),
            new OuraCollection("ring_configuration", "Ring Details", "Ring and Account", "ring_configuration", OuraDateMode.All)
                .Field("id", "Ring ID", "Ring configuration identifier; all paired configurations are returned.")
                .Field("hardware_type", "Hardware", "Hardware generation/type.").Field("design", "Design", "Ring design.")
                .Field("color", "Colour", "Ring colour.").Field("size", "Size", "US ring size.").Field("firmware_version", "Firmware", "Firmware version if provided.")
                .Field("set_up_at", "Set Up At", "Setup timestamp including UTC offset."),
            new OuraCollection("ring_battery_level", "Battery History", "Ring and Account", "ring_configuration", OuraDateMode.Timestamp)
                .Field("timestamp", "Timestamp", "Sample time including UTC offset.").Field("level", "Battery (%)", "Battery percentage, 0–100.")
                .Field("charging", "Charging", "True if charging; blank if unknown.").Field("in_charger", "In Charger", "True if in charger; blank if unknown."),
            new OuraCollection("personal_info", "Account Profile", "Ring and Account", "personal email", OuraDateMode.Single)
                .Field("id", "ID", "Oura user identifier.").Field("email", "Email", "Email address if approved.").Field("age", "Age", "Age in years if provided.")
                .Field("biological_sex", "Biological Sex", "Optional profile value.").Field("height", "Height (metres)", "Profile height in metres.")
                .Field("weight", "Weight (kg)", "Profile weight in kilograms.")
        }.AsReadOnly();
        private static readonly IReadOnlyList<OuraCollection> SupportingCollections = new List<OuraCollection> {
            new OuraCollection("daily_activity", "Daily Activity", "", "daily"),
            new OuraCollection("daily_readiness", "Daily Readiness", "", "daily"),
            new OuraCollection("daily_sleep", "Daily Sleep", "", "daily"),
            new OuraCollection("daily_spo2", "SpO2", "", "spo2"),
            new OuraCollection("workout", "Workouts", "", "workout"),
            new OuraCollection("enhanced_tag", "Tags", "", "tag"),
            new OuraCollection("tag", "Legacy Tags", "", "tag")
        }.AsReadOnly();

        internal static bool IsKnown(OuraCollection collection) => collection != null && (All.Contains(collection) || SupportingCollections.Contains(collection));

        public static OuraCollection Get(string endpoint)
        {
            var collection = All.Concat(SupportingCollections).SingleOrDefault(c => c.Endpoint == endpoint);
            if (collection == null) throw new ArgumentException("Unknown Oura export.", nameof(endpoint));
            return collection;
        }
        public static string RequestedScopes => String.Join(" ", new[] { "daily", "heartrate", "workout", "tag", "session", "spo2" }
            .Concat(All.SelectMany(c => c.Scope.Split(' '))).Distinct(StringComparer.Ordinal));
    }
}
