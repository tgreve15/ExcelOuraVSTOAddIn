using System;
using System.Collections.Generic;
using System.Linq;

namespace ExcelOuraVSTOAddIn
{
    //[SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class OuraFields
    {
        private static List<OuraFields> defaultFields;
        private static List<OuraFields> defaultTagFields;
        private static List<OuraFields> defaultWorkoutFields;
        private static List<OuraFields> defaultSpo2Fields;

        /// <summary>
        /// Create a field list of all possible values that can be returned by the primary 
        /// Oura web requests. Each mapped to the Oura sector it belongs to, the name of the 
        /// property/method used to get the data from the OuraCombinedObject, and for some
        /// indicate they aren't to be shown on the interface (due to complexity of results
        /// for the intended output).
        /// Note: Order id's provided as otherwise they appear in a big mess when you
        /// open the interface since the interface will allow you to modify the order that results
        /// are returned to Excel, so sorts them in the UI
        /// </summary>
        private static void SetupDefaultFields()
        {
            // NOTE (fixed): every MethodName below is verified against the actual public
            // properties/methods of OuraCombinedObjectV2 (see OuraResponsesV2.cs). The
            // previous version of this list referenced a stale, pre-V2 API naming scheme
            // (e.g. "BedtimeStart", "SleepScore", "CalActive", "Steps") that no longer
            // exists on OuraCombinedObjectV2. Because dynamicExecuteProperty/dynamicExecuteMethod
            // in OuraRibbonBar.cs silently return the literal string "FAILURE" for any
            // unresolved member, every Sleep and Activity field except the handful that
            // happened to still match (the Readiness "rr_*" fields) wrote "FAILURE" into
            // Excel instead of real data. This rewrite fixes that mapping.
            List<OuraFields> localFields = new List<OuraFields>();
            localFields.Add(new OuraFields("Summary Date", "Sleep", "SummaryDate", 1, "Date when the measured period started.", true));

            // Sleep (per-period) fields - source: /v2/usercollection/sleep
            localFields.Add(new OuraFields("Sleep Period Id", "Sleep", "s_id", 2, false, AccessorType.Property, "Unique identifier for the sleep period."));
            localFields.Add(new OuraFields("Bedtime Start", "Sleep", "s_bedtime_start", 5, "Local time when the sleep period started.", true));
            localFields.Add(new OuraFields("Bedtime End", "Sleep", "s_bedtime_end", 6, "Local time when the sleep period ended.", true));
            localFields.Add(new OuraFields("Bedtime Start (Absolute)", "Sleep", "BedtimeStartFormatLocal", 7, true, AccessorType.Method, "Time when the sleep period started, ignoring timezones."));
            localFields.Add(new OuraFields("Bedtime End (Absolute)", "Sleep", "BedtimeEndFormatLocal", 8, true, AccessorType.Method, "Time when the sleep period ended, ignoring timezones."));
            localFields.Add(new OuraFields("Total Sleep Duration", "Sleep", "s_total_sleep_duration", 17, "Total amount of sleep registered during the sleep period, in seconds (rem + light + deep).", true));
            localFields.Add(new OuraFields("Time In Bed", "Sleep", "s_time_in_bed", 18, "Total duration spent in bed, in seconds."));
            localFields.Add(new OuraFields("Awake Time", "Sleep", "s_awake_time", 19, "Total amount of awake time registered during the sleep period, in seconds.", true));
            localFields.Add(new OuraFields("Light Sleep Duration", "Sleep", "s_light_sleep_duration", 20, "Total amount of light sleep registered during the sleep period, in seconds.", true));
            localFields.Add(new OuraFields("REM Sleep Duration", "Sleep", "s_rem_sleep_duration", 21, "Total amount of REM sleep registered during the sleep period, in seconds.", true));
            localFields.Add(new OuraFields("Deep Sleep Duration", "Sleep", "s_deep_sleep_duration", 22, "Total amount of deep (N3) sleep registered during the sleep period, in seconds.", true));
            localFields.Add(new OuraFields("Restless Periods", "Sleep", "s_restless_periods", 24, "Number of restless periods during the sleep period."));
            localFields.Add(new OuraFields("Efficiency", "Sleep", "s_efficiency", 25, "Sleep efficiency: percentage of the sleep period actually spent asleep."));
            localFields.Add(new OuraFields("Lowest Heart Rate", "Sleep", "s_lowest_heart_rate", 27, "The lowest heart rate (5 minute sliding average) registered during the sleep period."));
            localFields.Add(new OuraFields("Average Heart Rate", "Sleep", "s_average_heart_rate", 28, "The average heart rate registered during the sleep period."));
            localFields.Add(new OuraFields("Average Breath", "Sleep", "s_average_breath", 30, "Average respiratory rate in breaths per minute."));
            localFields.Add(new OuraFields("Average HRV", "Sleep", "s_average_hrv", 31, "Average heart rate variability during the sleep period."));
            localFields.Add(new OuraFields("Heart Rate (5 min items)", "Sleep", "s_heart_rate_items", 32, false, AccessorType.Property, "Raw 5-minute heart rate samples for the sleep period."));
            localFields.Add(new OuraFields("HRV (5 min items)", "Sleep", "s_hrv_items", 33, false, AccessorType.Property, "Raw 5-minute HRV samples for the sleep period."));
            localFields.Add(new OuraFields("Latency", "Sleep", "s_latency", 34, "Time it took to fall asleep, in seconds."));
            localFields.Add(new OuraFields("Movement 30 Sec", "Sleep", "s_movement_30_sec", 35, false, AccessorType.Property, "30-second movement classification string (1=no motion, 2=restless, 3=tossing/turning, 4=active)."));
            localFields.Add(new OuraFields("Sleep Phase 5 Min", "Sleep", "s_sleep_phase_5_min", 36, false, AccessorType.Property, "5-minute sleep phase classification string (1=deep, 2=light, 3=REM, 4=awake)."));
            localFields.Add(new OuraFields("Readiness Score Delta", "Sleep", "s_readiness_score_delta", 37, "Effect on the readiness score caused by this sleep period."));
            localFields.Add(new OuraFields("Sleep Score Delta", "Sleep", "s_sleep_score_delta", 38, "Effect on the sleep score caused by this sleep period."));
            localFields.Add(new OuraFields("Sleep Algorithm Version", "Sleep", "s_sleep_algorithm_version", 39, false, AccessorType.Property, "Version of the sleep algorithm used to calculate this data."));
            localFields.Add(new OuraFields("Low Battery Alert", "Sleep", "s_low_battery_alert", 40, "Flag indicating a low ring battery alert occurred during the sleep period."));
            localFields.Add(new OuraFields("Sleep Type", "Sleep", "s_type", 42, "Possible sleep periods - 'sleep', 'long_sleep', 'late_nap', 'rest'."));

            // Daily Sleep score - source: /v2/usercollection/daily_sleep
            localFields.Add(new OuraFields("Daily Sleep Score", "Sleep", "ds_score", 43, "Daily sleep score (0-100), a weighted average of the sleep score contributors below.", true));
            localFields.Add(new OuraFields("Daily Sleep Timestamp", "Sleep", "ds_timestamp", 44, "Timestamp the daily sleep score was calculated for."));
            localFields.Add(new OuraFields("Score Contributor: Efficiency", "Sleep", "ds_efficiency", 45, "Contribution of sleep efficiency to the daily sleep score, range [1,100]."));
            localFields.Add(new OuraFields("Score Contributor: Latency", "Sleep", "ds_latency", 46, "Contribution of sleep latency to the daily sleep score, range [1,100]."));
            localFields.Add(new OuraFields("Score Contributor: Timing", "Sleep", "ds_timing", 47, "Contribution of sleep timing to the daily sleep score, range [1,100]."));
            localFields.Add(new OuraFields("Score Contributor: Restfulness", "Sleep", "ds_restfulness", 48, "Contribution of restfulness to the daily sleep score, range [1,100]."));
            localFields.Add(new OuraFields("Score Contributor: REM Sleep", "Sleep", "ds_rem_sleep", 49, "Contribution of REM sleep to the daily sleep score, range [1,100]."));
            localFields.Add(new OuraFields("Score Contributor: Deep Sleep", "Sleep", "ds_deep_sleep", 50, "Contribution of deep sleep to the daily sleep score, range [1,100]."));
            localFields.Add(new OuraFields("Score Contributor: Total Sleep", "Sleep", "ds_total_sleep", 51, "Contribution of total sleep to the daily sleep score, range [1,100]."));

            // Activity Fields - source: /v2/usercollection/daily_activity
            localFields.Add(new OuraFields("Activity Score", "Activity", "act_score", 55, "Daily activity score (0-100), estimating how well recent activity matched the ring user's needs.", true));
            localFields.Add(new OuraFields("Activity Timestamp", "Activity", "act_timestamp", 56, "Timestamp the daily activity score was calculated for."));
            localFields.Add(new OuraFields("Calorie Active", "Activity", "act_active_calories", 57, "Energy consumption caused by physical activity during the day, in kilocalories.", true));
            localFields.Add(new OuraFields("Calorie Total", "Activity", "act_total_calories", 58, "Total energy consumption during the day, including Basal Metabolic Rate, in kilocalories.", true));
            localFields.Add(new OuraFields("Class 5 Min", "Activity", "act_class_5_min", 59, false, AccessorType.Property, "5-minute activity classification string for the day."));
            localFields.Add(new OuraFields("Steps", "Activity", "act_steps", 60, "Total number of steps registered during the day.", true));
            localFields.Add(new OuraFields("Non Wear Time", "Activity", "act_non_wear_time", 61, "Minutes during the day the ring was not being worn, in seconds. Can be used as a proxy for data accuracy."));
            localFields.Add(new OuraFields("Resting Time", "Activity", "act_resting_time", 62, "Time spent resting (sleeping or lying down) during the day, in seconds."));
            localFields.Add(new OuraFields("Sedentary Time", "Activity", "act_sedentary_time", 63, "Time spent sedentary (sitting or standing still) during the day, in seconds."));
            localFields.Add(new OuraFields("Low Activity Time", "Activity", "act_low_activity_time", 64, "Time spent in low intensity activity during the day, in seconds."));
            localFields.Add(new OuraFields("Medium Activity Time", "Activity", "act_medium_activity_time", 65, "Time spent in medium intensity activity during the day, in seconds."));
            localFields.Add(new OuraFields("High Activity Time", "Activity", "act_high_activity_time", 66, "Time spent in high intensity activity during the day, in seconds."));
            localFields.Add(new OuraFields("Inactivity Alerts", "Activity", "act_inactivity_alerts", 67, "Number of continuous inactive periods of 60 minutes or more during the day."));
            localFields.Add(new OuraFields("Average MET Minutes", "Activity", "act_average_met_minutes", 68, "Average MET level during the whole day."));
            localFields.Add(new OuraFields("MET Items (per minute)", "Activity", "act_metItems", 69, false, AccessorType.Property, "Raw per-minute MET level samples for the day."));
            localFields.Add(new OuraFields("Sedentary MET Minutes", "Activity", "act_sedentary_met_minutes", 70, "Total MET minutes accumulated during sedentary minutes of the day."));
            localFields.Add(new OuraFields("Low Activity MET Minutes", "Activity", "act_low_activity_met_minutes", 71, "Total MET minutes accumulated during low intensity activity minutes of the day."));
            localFields.Add(new OuraFields("Medium Activity MET Minutes", "Activity", "act_medium_activity_met_minutes", 72, "Total MET minutes accumulated during medium intensity activity minutes of the day."));
            localFields.Add(new OuraFields("High Activity MET Minutes", "Activity", "act_high_activity_met_minutes", 73, "Total MET minutes accumulated during high intensity activity minutes of the day."));
            localFields.Add(new OuraFields("Target Calories", "Activity", "act_target_calories", 74, "Daily calorie burn target."));
            localFields.Add(new OuraFields("Target Meters", "Activity", "act_target_meters", 75, "Daily equivalent walking distance target, in meters."));
            localFields.Add(new OuraFields("Meters To Target", "Activity", "act_meters_to_target", 76, "Remaining distance, in meters, to reach the daily target."));
            localFields.Add(new OuraFields("Equivalent Walking Distance", "Activity", "act_equivalent_walking_distance", 77, "Equivalent walking distance for the day's activity, in meters."));
            localFields.Add(new OuraFields("Score Contributor: Meet Daily Targets", "Activity", "act_meet_daily_targets", 78, "How often the daily activity target has been reached over the last seven days."));
            localFields.Add(new OuraFields("Score Contributor: Move Every Hour", "Activity", "act_move_every_hour", 79, "How well long periods of inactivity have been avoided over the last 24 hours."));
            localFields.Add(new OuraFields("Score Contributor: Recovery Time", "Activity", "act_recovery_time", 80, "Whether sufficient recovery time has been had over the last seven days."));
            localFields.Add(new OuraFields("Score Contributor: Stay Active", "Activity", "act_stay_active", 81, "How well inactivity has been avoided over the last 24 hours."));
            localFields.Add(new OuraFields("Score Contributor: Training Frequency", "Activity", "act_training_frequency", 82, "How regularly physical exercise has occurred over the last seven days."));
            localFields.Add(new OuraFields("Score Contributor: Training Volume", "Activity", "act_training_volume", 83, "How much physical exercise has occurred over the last seven days."));

            // Readiness fields
            localFields.Add(new OuraFields("Readiness Score", "Readiness", "rr_Score", 96, "Daily readiness score.", true));
            localFields.Add(new OuraFields("Activity Balance", "Readiness", "rr_activity_balance", 97, "Contribution of cumulative activity balance in range [1, 100]."));
            localFields.Add(new OuraFields("Heart Rate Variability(HRV) Balance", "Readiness", "rr_hrv_balance", 98, "Contribution of heart rate variability balance in range [1, 100]."));
            localFields.Add(new OuraFields("Previous Day Activity", "Readiness", "rr_previous_day_activity", 99, "Contribution of previous day's activity in range [1, 100]."));
            localFields.Add(new OuraFields("Previous Night", "Readiness", "rr_previous_night", 100, "Contribution of previous night's sleep in range [1, 100]."));
            localFields.Add(new OuraFields("Recovery Index", "Readiness", "rr_recovery_index", 101, "Contribution of recovery index in range [1, 100]."));
            localFields.Add(new OuraFields("Resting Heart Rate", "Readiness", "rr_resting_heart_rate", 102, "Contribution of resting heart rate in range [1, 100]."));
            localFields.Add(new OuraFields("Sleep Balance", "Readiness", "rr_sleep_balance", 103, "Contribution of sleep balance in range [1, 100]."));
            localFields.Add(new OuraFields("Temperature Deviation", "Readiness", "rr_temperature_deviation", 104, "Temperature deviation in degrees Celsius."));
            localFields.Add(new OuraFields("Temperature Trend Deviation", "Readiness", "rr_temperature_trend_deviation", 105, "Temperature trend deviation in degrees Celsius."));
            localFields.Add(new OuraFields("Body Temperature", "Readiness", "rr_body_temperature", 106, "Contribution of body temperature in range [1, 100]."));
            localFields.Add(new OuraFields("Readiness Timestamp", "Readiness", "rr_Timestamp", 107, "Timestamp of the daily readiness."));
            defaultFields = localFields;

    }


        private static void SetupDefaultWorkoutFields()
        {
            List<OuraFields> localFields = new List<OuraFields>();
            localFields.Add(new OuraFields("Id", "Workouts", "id", 1, false, AccessorType.Property, "Id of the workout.", false));
            localFields.Add(new OuraFields("day", "Workouts", "day", 2, "Day when the workout occurred.", true));
            localFields.Add(new OuraFields("Start Date/Time", "Workouts", "start_datetime", 3, "Timestamp indicating when the workout started.", true));
            localFields.Add(new OuraFields("End Date/Time", "Workouts", "end_datetime", 4, "Timestamp indicating when the workout ended.", true));
            localFields.Add(new OuraFields("Activity", "Workouts", "activity", 5, "Type of the workout activity.", true));
            localFields.Add(new OuraFields("Distance", "Workouts", "distance", 6, "Distance traveled in meters during the workout.", true));
            localFields.Add(new OuraFields("Calories", "Workouts", "calories", 7, "Energy burned in kilocalories during the workout."));
            localFields.Add(new OuraFields("Intensity", "Workouts", "intensity", 8, "Possible workout intensities.", true));
            localFields.Add(new OuraFields("Source", "Workouts", "source", 9, "Possible workout sources.", true));
            defaultWorkoutFields = localFields;

        }

        private static void SetupDefaultTagFields()
        {
            List<OuraFields> localFields = new List<OuraFields>();
            localFields.Add(new OuraFields("Id", "Tags", "id", 1, false,AccessorType.Property, "Id of the recorded tag.", false));
            localFields.Add(new OuraFields("Start Day", "Tags", "start_day", 2, "Day of the tag (if no duration) or the start day of the tag (with duration).", true));
            localFields.Add(new OuraFields("Start Time", "Tags", "start_time", 3, "Timestamp of the tag (if no duration) or the start time of the tag (with duration).", true));
            localFields.Add(new OuraFields("End Day", "Tags", "end_day", 4, "Day of the tag's end for events with duration or `NULL` if there is no duration.", true));
            localFields.Add(new OuraFields("End Time", "Tags", "end_time", 5, "Timestamp of the tag's end for events with duration or `NULL` if there is no duration.", true));
            localFields.Add(new OuraFields("Tag Type Code", "Tags", "tag_type_code", 6, "The unique code of the selected tag type, `NULL` for text-only tags, or `custom` for custom tag types.", true));
            localFields.Add(new OuraFields("Comment", "Tags", "comment", 7, "Additional freeform text on the tag.", true));
            defaultTagFields = localFields;
        }

        private static void SetupDefaultSpo2Fields()
        {
            List<OuraFields> localFields = new List<OuraFields>();
            localFields.Add(new OuraFields("Id", "Spo2", "id", 1, false, AccessorType.Property, "Id of the recorded Spo2.", false));
            localFields.Add(new OuraFields("Day", "Spo2", "day", 1, "Day of the recording", true));
            localFields.Add(new OuraFields("Average Percentage", "Spo2", "spo2PercentageAverage", 1,true, AccessorType.Method, "Average oxygen saturation (SpO2) throughout the night.", true));
            defaultSpo2Fields = localFields;
        }

        private static readonly object DefaultsLock = new object();

        public static void ResetFields()
        {
            lock (DefaultsLock)
            {
                defaultFields = defaultTagFields = defaultSpo2Fields = defaultWorkoutFields = null;
            }
        }

        public static List<OuraFields> CurrentFields()
        {
            lock (DefaultsLock)
            {
                if (defaultFields == null) SetupDefaultFields();
                return defaultFields.Select(OuraAPIInterface.OuraFieldSelections.Clone).ToList();
            }
        }

        public static List<OuraFields> CurrentTagFields()
        {
            lock (DefaultsLock)
            {
                if (defaultTagFields == null) SetupDefaultTagFields();
                return defaultTagFields.Select(OuraAPIInterface.OuraFieldSelections.Clone).ToList();
            }
        }

        public static List<OuraFields> CurrentSpo2Fields()
        {
            lock (DefaultsLock)
            {
                if (defaultSpo2Fields == null) SetupDefaultSpo2Fields();
                return defaultSpo2Fields.Select(OuraAPIInterface.OuraFieldSelections.Clone).ToList();
            }
        }

        public static List<OuraFields> CurrentWorkoutFields()
        {
            lock (DefaultsLock)
            {
                if (defaultWorkoutFields == null) SetupDefaultWorkoutFields();
                return defaultWorkoutFields.Select(OuraAPIInterface.OuraFieldSelections.Clone).ToList();
            }
        }
        public enum AccessorType { Method, Property };  // identify if field definition is to a property or method

        public string FieldName { get; set; }       // Name of the field, also default label displayed in the header
        public string OuraSection { get; set; }     // Sleep, Readiness, Activity
        public string MethodName { get; set; }      // Name of the Method or Property used to access data for this field
        public bool Accessible { get; set; }        // Should this field be displayed on the UI? 
        public AccessorType Accessor { get; set; }  // Is this a Method or Property? Assist for execution
        public int FieldOrder { get; set; }         // What position should this field appear in the ListView?
        public string CustomLabel { get; set; }     // User defined label to display in Excel for the field
        public bool FieldSelected { get; set; }     // Show this field as selected in the UI? Set if checked previously
        public string FieldDescription { get; set; }// Description information from the Oura API Documentation

        /// <summary>
        /// Constructor for OuraFields allowing greatest configuration
        /// </summary>
        /// <param name="fieldName">Name and Title for the field</param>
        /// <param name="ouraSection">Which section of Oura is this from? Sleep, Readiness, Activity</param>
        /// <param name="methodName">Name of the method/property on OuraCombinedObject to get this fields data</param>
        /// <param name="fieldOrder">Position to appear in the UI</param>
        /// <param name="accessible">Show this field in the UI?</param>
        /// <param name="aType">Type of accessor on OuraCombinedObject - Method or Property</param>
        /// <param name="desc">Description of the field</param>
        /// <param name="fieldSelected">Is the field checked by default?</param>
        public OuraFields(string fieldName, string ouraSection, string methodName, int fieldOrder, bool accessible, AccessorType aType, string desc, bool fieldSelected)
        {
            this.FieldName = fieldName;
            this.OuraSection = ouraSection;
            this.MethodName = methodName;
            this.Accessible = accessible;
            this.Accessor = aType;
            this.FieldOrder = fieldOrder;
            this.FieldDescription = desc;
            this.FieldSelected = fieldSelected;
        }

        public OuraFields(string fieldName, string ouraSection, string methodName, int fieldOrder, bool accessible, AccessorType aType, string desc)
            : this(fieldName, ouraSection, methodName, fieldOrder, accessible, aType, desc, false)
        { }

        // Call default constructor with some defaults
        private OuraFields(string fieldName, string ouraSection, string methodName, int fieldOrder, bool accessible, AccessorType aType)
            : this(fieldName, ouraSection, methodName, fieldOrder, accessible, aType, null, false)
        {
        }
        private OuraFields(string fieldName, string ouraSection, string methodName, int fieldOrder, bool accessible, AccessorType aType, bool fieldSelected)
    : this(fieldName, ouraSection, methodName, fieldOrder, accessible, aType, null, fieldSelected)
        {
        }
        // Call default constructor with some defaults
        private OuraFields(string fieldName, string ouraSection, string methodName, int fieldOrder, string desc)
            : this(fieldName, ouraSection, methodName, fieldOrder, true, AccessorType.Property, desc, false)
        {
        }

        private OuraFields(string fieldName, string ouraSection, string methodName, int fieldOrder, string desc, bool fieldSelected)
    : this(fieldName, ouraSection, methodName, fieldOrder, true, AccessorType.Property, desc, fieldSelected)
        {
        }

        // Call default constructor with some defaults
        private OuraFields(string fieldName, string ouraSection, string methodName, int fieldOrder)
            : this(fieldName, ouraSection, methodName, fieldOrder, true, AccessorType.Property, null, false)
        {
        }
    }
}
