using OuraAPIInterface.DailyActivity;
using OuraAPIInterface.Sleep;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OuraAPIInterface
{
    /// <summary>
    /// Response object for the /v2/usercollection/personal_info request.
    /// Restored here (this class previously lived in the now-removed, unused V1
    /// OuraResponse.cs file, but is used by the active V2 PerformAuthenticationV2 call).
    /// </summary>
    [DataContract]
    public class UserInfoResponse
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [DataMember(Name = "age")]
        public string Age { get; set; }
        [DataMember(Name = "weight")]
        public string Weight { get; set; }
        [DataMember(Name = "height")]
        public string Height { get; set; }
        // FIX: Oura's v2 personal_info endpoint calls this field "biological_sex", not
        // "gender" (that was the v1 field name). Map the field the API actually sends...
        [DataMember(Name = "biological_sex")]
        public string BiologicalSex { get; set; }
        [DataMember(Name = "email")]
        public string Email { get; set; }

        // ...and keep exposing "Gender" so existing callers (e.g. ConfigureOuraToken.cs)
        // don't need to change, and so this still degrades gracefully if Oura ever reverts
        // to (or adds back) a "gender" field.
        public string Gender => BiologicalSex;
    }

    /// <summary>
    /// Oura has separate calls for Sleep, Activity and Readiness data.
    /// To allow us to enter all information for a particular day, or set of days,
    /// into an Excel row, combine them into a single object
    /// </summary>
    [DataContract]
    public class OuraCombinedObjectV2
    {
        public string SummaryDate { get; set; }

        // Sleep Variables
        public string s_id { get; set; }
        public double? s_average_breath { get; set; }
        public double? s_average_heart_rate { get; set; }
        public double? s_average_hrv { get; set; }
        public double? s_awake_time { get; set; }
        public string s_bedtime_end { get; set; }
        public string s_bedtime_start { get; set; }
        public double? s_deep_sleep_duration { get; set; }
        public double? s_efficiency { get; set; }
        public List<double?> s_heart_rate_items { get; set; }
        public double? s_heart_rate_interval { get; set; }
        public double? s_hrv_interval { get; set; }
        public List<double?> s_hrv_items { get; set; }
        public double? s_latency { get; set; }
        public double? s_light_sleep_duration { get; set; }
        public bool? s_low_battery_alert { get; set; }
        public double? s_lowest_heart_rate { get; set; }
        public string s_movement_30_sec { get; set; }
        public int? s_period { get; set; }
        //public Readiness s_readiness { get; set; }
        public double? s_readiness_score_delta { get; set; }
        public double? s_rem_sleep_duration { get; set; }
        public int? s_restless_periods { get; set; }
        public string s_sleep_phase_5_min { get; set; }
        public double? s_sleep_score_delta { get; set; }
        public string s_sleep_algorithm_version { get; set; }
        public double? s_time_in_bed { get; set; }
        public double? s_total_sleep_duration { get; set; }
        public string s_type { get; set; }

        // DailySleep Variables
        public double? ds_score { get; set; }
        public double? ds_efficiency { get; set; }
        public double? ds_latency { get; set; }
        public double? ds_timing { get; set; }
        public double? ds_restfulness { get; set; }
        public double? ds_rem_sleep { get; set; }
        public double? ds_deep_sleep { get; set; }
        public double? ds_total_sleep { get; set; }
        public DateTime? ds_timestamp { get; set; }

        // Activity Variables
        public string act_class_5_min { get; set; }
        public double? act_score { get; set; }
        public double? act_active_calories { get; set; }
        public double? act_average_met_minutes { get; set; }
        public double? act_equivalent_walking_distance { get; set; }
        public double? act_high_activity_met_minutes { get; set; }
        public double? act_high_activity_time { get; set; }
        public double? act_inactivity_alerts { get; set; }
        public double? act_low_activity_met_minutes { get; set; }
        public double? act_low_activity_time { get; set; }
        public double? act_medium_activity_met_minutes { get; set; }
        public double? act_medium_activity_time { get; set; }
        //public Met act_met { get; set; }
        public double? act_meters_to_target { get; set; }
        public double? act_non_wear_time { get; set; }
        public double? act_resting_time { get; set; }
        public double? act_sedentary_met_minutes { get; set; }
        public double? act_sedentary_time { get; set; }
        public double? act_steps { get; set; }
        public double? act_target_calories { get; set; }
        public double? act_target_meters { get; set; }
        public double? act_total_calories { get; set; }
        //public string day { get; set; }
        public DateTime? act_timestamp { get; set; }
        // Activity Contributors
        public double? act_meet_daily_targets { get; set; }
        public double? act_move_every_hour { get; set; }
        public double? act_recovery_time { get; set; }
        public double? act_stay_active { get; set; }
        public double? act_training_frequency { get; set; }
        public double? act_training_volume { get; set; }
        public List<double?> act_metItems { get; set; }

        // Readiness Variables
        public double? rr_Score { get; set; }
        public DateTime? rr_Timestamp { get; set; }
        public double? rr_temperature_deviation { get; set; }
        public double? rr_temperature_trend_deviation { get; set; }
        public double? rr_body_temperature { get; set; }
        public int? rr_activity_balance { get; set; }
        public int? rr_hrv_balance { get; set; }
        public int? rr_previous_day_activity { get; set; }
        public int? rr_previous_night { get; set; }
        public int? rr_recovery_index { get; set; }
        public int? rr_resting_heart_rate { get; set; }
        public int? rr_sleep_balance { get; set; }

        public OuraCombinedObjectV2(string summaryDate)
        {
            SummaryDate = summaryDate;
        }

        public string BedtimeStartFormatLocal() => WallTime(s_bedtime_start);
        public string BedtimeEndFormatLocal() => WallTime(s_bedtime_end);

        private static string WallTime(string value)
        {
            if (String.IsNullOrEmpty(value)) return "";
            DateTimeOffset timestamp;
            if (!DateTimeOffset.TryParse(value, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out timestamp))
                throw new OuraException("Oura returned an invalid sleep timestamp. The export has stopped.");
            // Preserve the clock time at the recorded location, including after travelling.
            return timestamp.DateTime.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Take one day's worth of data from each call, and copy them into this CombinedObject
        /// for reporting and processing. It is likely to get one day with only activity data and
        /// no other, so be prepared if ANY responses don't exist
        /// </summary>
        /// <param name="sr">SleepResponse web response object</param>
        /// <param name="rr">ReadinessResponse web response object</param>
        /// <param name="ar">ActivityResponse web response object</param>
        public void UpdateFrom(Sleep.Datum sr, DailySleep.Datum dsr, DailyReadiness.Datum rr, DailyActivity.Datum ar)
        {
            if (sr != null)
            {
                //this.SummaryDate = sr.SummaryDate;
                this.s_bedtime_start = sr.bedtime_start;
                this.s_bedtime_end = sr.bedtime_end;
                this.s_latency = sr.latency;
                this.s_total_sleep_duration = sr.total_sleep_duration;
                this.s_awake_time = sr.awake_time;
                this.s_light_sleep_duration = sr.light_sleep_duration;
                this.s_rem_sleep_duration = sr.rem_sleep_duration;
                this.s_deep_sleep_duration = sr.deep_sleep_duration;
                this.s_restless_periods = sr.restless_periods;
                this.s_efficiency = sr.efficiency;
                this.s_lowest_heart_rate = sr.lowest_heart_rate;
                this.s_average_heart_rate = sr.average_heart_rate;
                this.s_average_breath = sr.average_breath;
                if (sr.heart_rate != null && sr.heart_rate.items != null)
                {
                    this.s_heart_rate_items = sr.heart_rate.items;
                    this.s_heart_rate_interval = sr.heart_rate.interval;
                }
                if (sr.hrv != null && sr.hrv.items != null)
                {
                    this.s_hrv_items = sr.hrv.items;
                    this.s_hrv_interval = sr.hrv.interval;
                }
                this.s_average_hrv = sr.average_hrv;
                this.s_time_in_bed = sr.time_in_bed;
                this.s_low_battery_alert = sr.low_battery_alert;
                this.s_period = sr.period;
                this.s_sleep_phase_5_min = sr.sleep_phase_5_min;
                this.s_sleep_score_delta = sr.sleep_score_delta;
                this.s_movement_30_sec = sr.movement_30_sec;
                //this.BodeTemperature = sr.readiness.contributors.body_temperature;
                //this.TemperatureDeviation = sr.readiness.temperature_deviation;
                //this.TemperatureTrendDeviation = sr.readiness.temperature_trend_deviation;
                this.s_type = sr.type;
                this.s_id = sr.id;
                this.s_readiness_score_delta = sr.readiness_score_delta;
                this.s_sleep_algorithm_version = sr.sleep_algorithm_version;

            }

            if (dsr != null)
            {
                this.ds_score = dsr.score;
                this.ds_efficiency = dsr.contributors?.efficiency;
                this.ds_latency = dsr.contributors?.latency;
                this.ds_timing = dsr.contributors?.timing;
                this.ds_restfulness = dsr.contributors?.restfulness;
                this.ds_rem_sleep = dsr.contributors?.rem_sleep;
                this.ds_deep_sleep = dsr.contributors?.deep_sleep;

                this.ds_total_sleep = dsr.contributors?.total_sleep;
                this.ds_timestamp = dsr.timestamp;
            }

            if (rr != null)
            {
                this.rr_Score = rr.score;
                this.rr_Timestamp = rr.timestamp;
                this.rr_temperature_deviation = rr.temperature_deviation;
                this.rr_temperature_trend_deviation = rr.temperature_trend_deviation;
                this.rr_body_temperature = rr.contributors?.body_temperature;
                this.rr_activity_balance = rr.contributors?.activity_balance;
                this.rr_hrv_balance = rr.contributors?.hrv_balance;
                this.rr_previous_day_activity = rr.contributors?.previous_day_activity;
                this.rr_previous_night = rr.contributors?.previous_night;
                this.rr_recovery_index = rr.contributors?.recovery_index;
                this.rr_resting_heart_rate = rr.contributors?.resting_heart_rate;
                this.rr_sleep_balance = rr.contributors?.sleep_balance;

                // CHECK THESE AREN"T ALSO IN SLEEP
                //this.TemperatureDeviation = rr.temperature_deviation;
                //this.TemperatureTrendDeviation = rr.temperature_trend_deviation;
                //this.ReadinessRestModeState = rr.;
            }
            if (ar != null)
            {
                this.act_active_calories = ar.active_calories;
                this.act_total_calories = ar.total_calories;
                this.act_class_5_min = ar.class_5_min;
                this.act_steps = ar.steps;
                this.act_non_wear_time = ar.non_wear_time;
                this.act_resting_time = ar.resting_time;
                this.act_sedentary_time = ar.sedentary_time;
                this.act_low_activity_time = ar.low_activity_time;
                this.act_medium_activity_time = ar.medium_activity_time;
                this.act_high_activity_time = ar.high_activity_time;
                this.act_inactivity_alerts = ar.inactivity_alerts;
                this.act_average_met_minutes = ar.average_met_minutes;
                if (ar.met != null && ar.met.items != null)
                {
                    this.act_metItems = ar.met.items;
                }
                this.act_sedentary_met_minutes = ar.sedentary_met_minutes;
                this.act_low_activity_met_minutes = ar.low_activity_met_minutes;
                this.act_medium_activity_met_minutes = ar.medium_activity_met_minutes;
                this.act_high_activity_met_minutes = ar.high_activity_met_minutes;
                this.act_target_calories = ar.target_calories;
                this.act_target_meters = ar.target_meters;
                this.act_meters_to_target = ar.meters_to_target;
                this.act_equivalent_walking_distance = ar.equivalent_walking_distance;
                this.act_score = ar.score;
                this.act_timestamp = ar.timestamp;
                this.act_meet_daily_targets = ar.contributors?.meet_daily_targets;
                this.act_move_every_hour = ar.contributors?.move_every_hour;
                this.act_recovery_time = ar.contributors?.recovery_time;
                this.act_stay_active = ar.contributors?.stay_active;
                this.act_training_frequency = ar.contributors?.training_frequency;
                this.act_training_volume = ar.contributors?.training_volume;
            }
        }
    }
    namespace DailyActivity
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Contributors
        {
            public double? meet_daily_targets { get; set; }
            public double? move_every_hour { get; set; }
            public double? recovery_time { get; set; }
            public double? stay_active { get; set; }
            public double? training_frequency { get; set; }
            public double? training_volume { get; set; }
        }

        public class Datum
        {
            public string class_5_min { get; set; }
            public double? score { get; set; }
            public double active_calories { get; set; }
            public double average_met_minutes { get; set; }
            public Contributors contributors { get; set; }
            public double equivalent_walking_distance { get; set; }
            public double high_activity_met_minutes { get; set; }
            public double high_activity_time { get; set; }
            public double inactivity_alerts { get; set; }
            public double low_activity_met_minutes { get; set; }
            public double low_activity_time { get; set; }
            public double medium_activity_met_minutes { get; set; }
            public double medium_activity_time { get; set; }
            public Met met { get; set; }
            public double meters_to_target { get; set; }
            public double non_wear_time { get; set; }
            public double resting_time { get; set; }
            public double sedentary_met_minutes { get; set; }
            public double sedentary_time { get; set; }
            public double steps { get; set; }
            public double target_calories { get; set; }
            public double target_meters { get; set; }
            public double total_calories { get; set; }
            public string day { get; set; }
            public DateTime timestamp { get; set; }
        }

        public class Met
        {
            public double interval { get; set; }
            public List<double?> items { get; set; }
            public DateTime timestamp { get; set; }
        }

        public class DailyActivityResponseV2
        {
            public List<Datum> data { get; set; }
            public string next_token { get; set; }
        }
    }
    namespace DailyReadiness
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Contributors
        {
            public int? activity_balance { get; set; }
            public int? body_temperature { get; set; }
            public int? hrv_balance { get; set; }
            public int? previous_day_activity { get; set; }
            public int? previous_night { get; set; }
            public int? recovery_index { get; set; }
            public int? resting_heart_rate { get; set; }
            public int? sleep_balance { get; set; }
        }

        public class Datum
        {
            public Contributors contributors { get; set; }
            public string day { get; set; }
            public int? score { get; set; }
            public double? temperature_deviation { get; set; }
            public double? temperature_trend_deviation { get; set; }
            public DateTime timestamp { get; set; }
        }

        public class DailyReadinessResponseV2
        {
            public List<Datum> data { get; set; }
            public string next_token { get; set; }
        }
    }

    namespace DailySleep
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Contributors
        {
            public int? deep_sleep { get; set; }
            public int? efficiency { get; set; }
            public int? latency { get; set; }
            public int? rem_sleep { get; set; }
            public int? restfulness { get; set; }
            public int? timing { get; set; }
            public int? total_sleep { get; set; }
        }

        public class Datum
        {
            public Contributors contributors { get; set; }
            public string day { get; set; }
            public int? score { get; set; }
            public DateTime timestamp { get; set; }
        }

        public class DailySleepResponseV2
        {
            public List<Datum> data { get; set; }
            public string next_token { get; set; }
        }
    }

    namespace DailySpo2
    {
        public class Datum
        {
            public string id { get; set; }
            public string day { get; set; }
            public Spo2Percentage spo2_percentage { get; set; }

            public double? spo2PercentageAverage()
            {
                return spo2_percentage?.average;
            }
        }
        public class Spo2Percentage
        {
            public double average { get; set; }
        }
        public class DailySpo2ResponseV2
        {
            public List<Datum> data { get; set; }
            public string next_token { get; set; }
        }
    }

    namespace EnhancedTags
    {
        public class Datum
        {
            public string id { get; set; }
            public string tag_type_code { get; set; }
            public string start_time { get; set; }
            public string end_time { get; set; }
            public string start_day { get; set; }
            public string end_day { get; set; }
            public string comment { get; set; }
        }

        public class EnhancedTagsResponseV2
        {
            public List<Datum> data { get; set; }
            public string next_token { get; set; }
        }

        public class EnhancedTagSingleResponseV2
        {
            public string id { get; set; }
            public string tag_type_code { get; set; }
            public string start_time { get; set; }
            public string end_time { get; set; }
            public string start_day { get; set; }
            public string end_day { get; set; }
            public string comment { get; set; }
        }
    }

    namespace Sleep
    {
        public class Contributors
        {
            public int? activity_balance { get; set; }
            public int? body_temperature { get; set; }
            public int? hrv_balance { get; set; }
            public int? previous_day_activity { get; set; }
            public int? previous_night { get; set; }
            public int? recovery_index { get; set; }
            public int? resting_heart_rate { get; set; }
            public int? sleep_balance { get; set; }
        }

        public class Datum
        {
            public string id { get; set; }
            public double? average_breath { get; set; }
            public double? average_heart_rate { get; set; }
            public double? average_hrv { get; set; }
            public double? awake_time { get; set; }
            public string bedtime_end { get; set; }
            public string bedtime_start { get; set; }
            public string day { get; set; }
            public double? deep_sleep_duration { get; set; }
            public double? efficiency { get; set; }
            public HeartRate heart_rate { get; set; }
            public Hrv hrv { get; set; }
            public double? latency { get; set; }
            public double? light_sleep_duration { get; set; }
            public bool low_battery_alert { get; set; }
            public double? lowest_heart_rate { get; set; }
            public string movement_30_sec { get; set; }
            public int period { get; set; }
            public Readiness readiness { get; set; }
            public double? readiness_score_delta { get; set; }
            public double? rem_sleep_duration { get; set; }
            public int? restless_periods { get; set; }
            public string sleep_phase_5_min { get; set; }
            public double? sleep_score_delta { get; set; }
            public string sleep_algorithm_version { get; set; }
            public double? time_in_bed { get; set; }
            public double? total_sleep_duration { get; set; }
            public string type { get; set; }
        }

        public class HeartRate
        {
            public double interval { get; set; }
            public List<double?> items { get; set; }
            public string timestamp { get; set; }
        }

        public class Hrv
        {
            public double interval { get; set; }
            public List<double?> items { get; set; }
            public string timestamp { get; set; }
        }

        public class Readiness
        {
            public Contributors contributors { get; set; }
            public int? score { get; set; }
            public double? temperature_deviation { get; set; }
            public double? temperature_trend_deviation { get; set; }
        }
        public class SleepResponseV2
        {
            public List<Datum> data { get; set; }
            public string next_token { get; set; }
        }
    }
    namespace Workouts
    {
        public class Datum
        {
            public string id { get; set; }
            public string activity { get; set; }
            public double? calories { get; set; }
            public string day { get; set; }
            public double? distance { get; set; }
            public DateTime end_datetime { get; set; }
            public string intensity { get; set; }
            public object label { get; set; }
            public string source { get; set; }
            public DateTime start_datetime { get; set; }
        }

        public class WorkoutsResponseV2
        {
            public List<Datum> data { get; set; }
            public string next_token { get; set; }
        }
    }
    namespace Tags
    {
        public class Datum
        {
            public string id { get; set; }
            public string day { get; set; }
            public string text { get; set; }
            public DateTime timestamp { get; set; }
            public List<string> tags { get; set; }
        }

        public class TagResponseV2
        {
            public List<Datum> data { get; set; }
            public string next_token { get; set; }
        }
    }

    namespace HeartRates
    {
        public class Datum
        {
            public int bpm { get; set; }
            public string source { get; set; }
            public DateTime timestamp { get; set; }
        }

        public class HeartRateResponseV2
        {
            public List<Datum> data { get; set; }
            public string next_token { get; set; }
        }
    }
    namespace Sessions
    {
        public class Datum
        {
            public string id { get; set; }
            public string day { get; set; }
            public DateTime start_datetime { get; set; }
            public DateTime end_datetime { get; set; }
            public string type { get; set; }
            public HeartRate heart_rate { get; set; }
            public HeartRateVariability heart_rate_variability { get; set; }
            public string mood { get; set; }
            public MotionCount motion_count { get; set; }
        }

        public class HeartRate
        {
            public double interval { get; set; }
            public List<object> items { get; set; }
            public DateTime timestamp { get; set; }
        }

        public class HeartRateVariability
        {
            public double interval { get; set; }
            public List<object> items { get; set; }
            public DateTime timestamp { get; set; }
        }

        public class MotionCount
        {
            public double interval { get; set; }
            public List<double?> items { get; set; }
            public DateTime timestamp { get; set; }
        }

        public class SessionsResponseV2
        {
            public List<Datum> data { get; set; }
            public string next_token { get; set; }
        }

    }
}
