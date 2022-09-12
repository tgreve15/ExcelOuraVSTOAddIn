using System;
using System.Collections.Generic;
//using System.Configuration;

namespace ExcelOuraVSTOAddIn
{
    //[SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class OuraFields
    {
        private static List<OuraFields> defaultFields;

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
            List<OuraFields> localFields = new List<OuraFields>();
            localFields.Add(new OuraFields("Summary Date", "Sleep", "SummaryDate" ,1, "Date when the measured period started."));
            localFields.Add(new OuraFields("Sleep Period Id", "Sleep", "SleepPeriodId", 2, "Index of the sleep period among sleep periods with the same summary_date, where 0 = first sleep period of the day."));
            localFields.Add(new OuraFields("Is Longest", "Sleep", "IsLongest" ,3 ));
            localFields.Add(new OuraFields("Sleep Timezone", "Sleep", "SleepTimezone",4, "Timezone offset from UTC as minutes. For example, EEST (Eastern European Summer Time, +3h) is 180. PST (Pacific Standard Time, -8h) is -480. "));
            localFields.Add(new OuraFields("Bedtime Start", "Sleep", "BedtimeStart",5, "Local time when the sleep period started"));
            localFields.Add(new OuraFields("Bedtime End", "Sleep", "BedtimeEnd",6, "Local time when the sleep period ended"));
            localFields.Add(new OuraFields("Bedtime Start (Absolute)", "Sleep", "BedtimeStartFormatLocal",7, true, AccessorType.Method, "Time when the sleep period started, ignoring timezones"));
            localFields.Add(new OuraFields("Bedtime End (Absolute)", "Sleep", "BedtimeEndFormatLocal",8, true, AccessorType.Method, "Time when the sleep period ended, ignoring timezones"));
            localFields.Add(new OuraFields("Sleep Score", "Sleep", "SleepScore",9, "Represents overall sleep quality during the sleep period. It is calculated as a weighted average of sleep score contributors that represent one aspect of sleep quality each. "));
            localFields.Add(new OuraFields("Score Total", "Sleep", "ScoreTotal" ,10, "Represents total sleep time's (see sleep.total) contribution for sleep quality. The value depends on age of the user - the younger, the more sleep is needed for good score. The weight of sleep.score_total in sleep score calculation is 0.35."));
            localFields.Add(new OuraFields("Score Disturbances", "Sleep", "ScoreDisturbances", 11, "Represents sleep disturbances' contribution for sleep quality. Three separate measurements are used to calculate this contributor value: 1. Wake - up count - the more wake - ups, the lower the score.; 2. Got - up count - the more got - ups, the lower the score.; 3. Restless sleep(sleep.restless) - the more motion detected during sleep, the lower the score."));
            localFields.Add(new OuraFields("Score Efficiency", "Sleep", "ScoreEfficiency", 12, "Represents sleep efficiency's (see sleep.efficiency) contribution for sleep quality. The higher efficiency, the higher score. The weight of sleep.score_efficiency in sleep score calculation is 0.10."));
            localFields.Add(new OuraFields("Score Latency", "Sleep", "ScoreLatency", 13, "Represents sleep onset latency's (see sleep.onset_latency) contribution for sleep quality. A latency of about 15 minutes gives best score. Latency longer than that many indicate problems falling asleep, whereas a very short latency may be a sign of sleep debt. The weight of sleep.score_latency in sleep score calculation is 0.10."));
            localFields.Add(new OuraFields("Score REM", "Sleep", "ScoreREM", 14, "Represents REM sleep time's (see sleep.rem) contribution for sleep quality. The value depends on age of the user - the younger, the more sleep REM is needed for good score. The weight of sleep.score_rem in sleep score calculation is 0.10."));
            localFields.Add(new OuraFields("Score Deep", "Sleep", "ScoreDeep", 15, "Represents deep (N3) sleep time's (see sleep.deep) contribution for sleep quality. The value depends on age of the user - the younger, the more sleep is needed for good score. The weight of sleep.score_deep in sleep score calculation is 0.10."));
            localFields.Add(new OuraFields("Score Alignment", "Sleep", "ScoreAlignment", 16, "Represents circadian alignment's contribution for sleep score. Sleep midpoint time (sleep.midpoint_time) between 12PM and 3AM gives highest score. The more the midpoint time deviates from that range, the lower the score. The weigh of sleep.score_alignment in sleep score calculation is 0.10."));
            localFields.Add(new OuraFields("Sleep Total", "Sleep", "SleepTotal", 17, "Total amount of sleep registered during the sleep period (sleep.total = sleep.rem + sleep.light + sleep.deep)."));
            localFields.Add(new OuraFields("Duration", "Sleep", "Duration", 18, "Total duration of the sleep period (sleep.duration = sleep.bedtime_end - sleep.bedtime_start)."));
            localFields.Add(new OuraFields("Awake Time", "Sleep", "Awake", 19, "Total amount of awake time registered during the sleep period."));
            localFields.Add(new OuraFields("Light Sleep", "Sleep", "Light", 20, "Total amount of light (N1 or N2) sleep registered during the sleep period."));
            localFields.Add(new OuraFields("REM Sleep", "Sleep", "REM", 21, "Total amount of REM sleep registered during the sleep period."));
            localFields.Add(new OuraFields("Deep Sleep", "Sleep", "Deep", 22, "Total amount of deep (N3) sleep registered during the sleep period."));
            localFields.Add(new OuraFields("Onset Latency", "Sleep", "OnsetLatency", 23, "Detected latency from bedtime_start to the beginning of the first five minutes of persistent sleep."));
            localFields.Add(new OuraFields("Restless", "Sleep", "Restless", 24, "Restlessness of the sleep time, i.e. percentage of sleep time when the user was moving."));
            localFields.Add(new OuraFields("Efficiency", "Sleep", "Efficiency", 25, "Sleep efficiency is the percentage of the sleep period spent asleep (100% * sleep.total / sleep.duration)."));
            localFields.Add(new OuraFields("Midpoint Time", "Sleep", "MidpointTime", 26, "The time in seconds from the start of sleep to the midpoint of sleep. The midpoint ignores awake periods."));
            localFields.Add(new OuraFields("HR Lowest", "Sleep", "HRLowest", 27, "The lowest heart rate (5 minutes sliding average) registered during the sleep period."));
            localFields.Add(new OuraFields("HR Average", "Sleep", "HRAverage", 28, "The average heart rate registered during the sleep period."));
            localFields.Add(new OuraFields("RMSSD", "Sleep", "RMSSD", 29, "The average HRV calculated with rMSSD method."));
            localFields.Add(new OuraFields("Breath Average", "Sleep", "BreathAverage", 30, "Average respiratory rate in breaths per minute."));
            localFields.Add(new OuraFields("Temperature Delta", "Sleep", "TemperatureDelta", 31, "Skin temperature deviation from the long-term temperature average."));
            localFields.Add(new OuraFields("Hypnogram 5 Min", "Sleep", "Hypnogram5Min", 32, false, AccessorType.Property));
            localFields.Add(new OuraFields("HR 5 Min", "Sleep", "HR5Min", 33, false, AccessorType.Property));
            localFields.Add(new OuraFields("RMSSD 5 Min", "Sleep", "RMSSD5Min", 34, false, AccessorType.Property));
            localFields.Add(new OuraFields("Temperature Deviation", "Sleep", "TemperatureDeviation", 35));
            localFields.Add(new OuraFields("Temperature Trend Deviation", "Sleep", "TemperatureTrendDeviation", 36));
            localFields.Add(new OuraFields("Bedtime Start Delta", "Sleep", "BedtimeStartDelta", 37));
            localFields.Add(new OuraFields("Bedtime End Delta", "Sleep", "BedtimeEndDelta", 38));
            localFields.Add(new OuraFields("Midpoint At Delta", "Sleep", "MidpointAtDelta", 39));
            localFields.Add(new OuraFields("Timestamp", "Sleep", "Timestamp", 40));
            localFields.Add(new OuraFields("Timestamp (Absolute)", "Sleep", "TimestampFormatLocal", 41, true, AccessorType.Method));

            localFields.Add(new OuraFields("Type", "Sleep", "Type", 42, "Possible sleep periods - 'sleep'\'long_sleep'\'late_nap'\'rest'"));
            localFields.Add(new OuraFields("Average Breath Variation", "Sleep", "AverageBreathVariation", 43));
            localFields.Add(new OuraFields("Got Up Count", "Sleep", "GotUpCount", 44));
            localFields.Add(new OuraFields("Wake Up Count", "Sleep", "WakeUpCount", 45));
            localFields.Add(new OuraFields("Lowest Heart Rate Time Offset", "Sleep", "LowestHeartRateTimeOffset", 46));

            // Activity Fields
            localFields.Add(new OuraFields("Activity Timezone", "Activity", "ActivityTimezone", 50));
            localFields.Add(new OuraFields("Day Start", "Activity", "DayStart", 51, "UTC time when the activity day began. Oura activity day is usually from 4AM to 4AM local time."));
            localFields.Add(new OuraFields("Day End", "Activity", "DayEnd", 52, "UTC time when the activity day ended. Oura activity day is usually from 4AM to 4AM local time."));
            localFields.Add(new OuraFields("Day Start (local)", "Activity", "DayStartFormatLocal", 53, true, AccessorType.Method, "Absolute time when the activity day began ignoring timezones. Oura activity day is usually from 4AM to 4AM local time."));
            localFields.Add(new OuraFields("Day End (local)", "Activity", "DayEndFormatLocal", 54, true, AccessorType.Method, "Absolute time when the activity day ended ignoring timezones. Oura activity day is usually from 4AM to 4AM local time."));
            localFields.Add(new OuraFields("Calorie Active", "Activity", "CalActive", 55, "Energy consumption caused by the physical activity of the day in kilocalories."));
            localFields.Add(new OuraFields("Calorie Total", "Activity", "CalTotal", 56, "Total energy consumption during the day including Basal Metabolic Rate in kilocalories."));
            localFields.Add(new OuraFields("Class 5 min", "Activity", "Class5min", 57, false, AccessorType.Property));
            localFields.Add(new OuraFields("Steps", "Activity", "Steps", 58, "Total number of steps registered during the day."));
            localFields.Add(new OuraFields("Daily Movement", "Activity", "DailyMovement", 59, "Daily physical activity as equal meters i.e. amount of walking needed to get the same amount of activity."));
            localFields.Add(new OuraFields("Non Wear", "Activity", "NonWear", 60, "Number of minutes during the day when the user was not wearing the ring. Can be used as a proxy for data accuracy, i.e. how well the measured physical activity represents actual total activity of the ring user."));
            localFields.Add(new OuraFields("Rest", "Activity", "Rest", 61, "Number of minutes during the day spent resting i.e. sleeping or lying down (average MET level of the minute is below 1.05)."));
            localFields.Add(new OuraFields("Inactive", "Activity", "Inactive", 62, "Number of inactive minutes (sitting or standing still, average MET level of the minute between 1.05 and 2) during the day."));
            localFields.Add(new OuraFields("Low Activity", "Activity", "Low", 63, "Number of minutes during the day with low intensity activity (e.g. household work, average MET level of the minute between 2 and age dependent limit)."));
            localFields.Add(new OuraFields("Medium Activity", "Activity", "Medium", 64, "Number of minutes during the day with medium intensity activity (e.g. walking). The upper and lower MET level limits for medium intensity activity depend on user's age and gender."));
            localFields.Add(new OuraFields("High Activity", "Activity", "High", 65, "Number of minutes during the day with high intensity activity (e.g. running). The lower MET level limit for high intensity activity depends on user's age and gender."));
            localFields.Add(new OuraFields("Inactivity Alerts", "Activity", "InactivityAlerts", 66, "Number of continuous inactive periods of 60 minutes or more during the day."));
            localFields.Add(new OuraFields("Average Met", "Activity", "AverageMet", 67, "Average MET level during the whole day."));
            localFields.Add(new OuraFields("Met 1 min", "Activity", "Met1min", 68, false, AccessorType.Property));
            localFields.Add(new OuraFields("Met Min Inactive", "Activity", "MetMinInactive", 69, "Total MET minutes accumulated during inactive minutes of the day."));
            localFields.Add(new OuraFields("Met Min Low", "Activity", "MetMinLow", 70, "Total MET minutes accumulated during low intensity activity minutes of the day."));
            localFields.Add(new OuraFields("Met Min Medium", "Activity", "MetMinMedium", 71, "Total MET minutes accumulated during medium intensity activity minutes of the day."));
            localFields.Add(new OuraFields("Met Min High", "Activity", "MetMinHigh", 72, "Total MET minutes accumulated during high intensity activity minutes of the day."));
            localFields.Add(new OuraFields("Target Calories", "Activity", "TargetCalories", 73));
            localFields.Add(new OuraFields("Target KM", "Activity", "TargetKM", 74));
            localFields.Add(new OuraFields("Target Miles", "Activity", "TargetMiles", 75));
            localFields.Add(new OuraFields("To Target KM", "Activity", "ToTargetKM", 76));
            localFields.Add(new OuraFields("To Target Miles", "Activity", "ToTargetMiles", 77));
            localFields.Add(new OuraFields("Activity Score", "Activity", "ActivityScore", 78, "Provides an estimate how well recent physical activity has matched ring user's needs. It is calculated as a weighted average of activity score contributors that represent one aspect of suitability of the activity each. The contributor values are also available as separate parameters."));
            localFields.Add(new OuraFields("Score Meet Daily Targets", "Activity", "ScoreMeetDailyTargets", 79, "This activity score contributor indicates how often the ring user has reached his/her daily activity target during seven last days (100 = six or seven times, 95 = five times)."));
            localFields.Add(new OuraFields("Score Move Every Hour", "Activity", "ScoreMoveEveryHour", 80, "This activity score contributor indicates how well the ring user has managed to avoid long periods of inactivity (sitting or standing still) during last 24 hours. The contributor includes number of continuous inactive periods of 60 minutes or more (excluding sleeping). The more long inactive periods, the lower contributor value."));
            localFields.Add(new OuraFields("Score Recovery Time", "Activity", "ScoreRecoveryTime", 81, "This activity score contributor indicates if the user has got enough recovery time during last seven days."));
            localFields.Add(new OuraFields("Score Stay Active", "Activity", "ScoreStayActive", 82, "This activity score contributor indicates how well the ring user has managed to avoid of inactivity (sitting or standing still) during last 24 hours. The more inactivity, the lower contributor value."));
            localFields.Add(new OuraFields("Score Training Frequency", "Activity", "ScoreTrainingFrequency", 83, "This activity score contributor indicates how regularly the ring user has had physical exercise the ring user has got during last seven days. The contributor value is 100 when the user has got more than 100 minutes of medium or high intensity activity on at least four days during past seven days. The contributor value is 95 when the user has got more than 100 minutes of medium or high intensity activity on at least three days during past seven days."));
            localFields.Add(new OuraFields("Score Training Volume", "Activity", "ScoreTrainingVolume", 84, "This activity score contributor indicates how much physical exercise the ring user has got during last seven days. The contributor value is 100 when thes sum of weekly MET minutes is over 2000.The contributor value is 95 when the sum of weekly MET minutes is over 750.There is a weighting function so that the effect of each day gradually disappears."));
            localFields.Add(new OuraFields("Activity Rest Mode State", "Activity", "ActivityRestModeState", 85, "Indicates whether Rest Mode was enabled or recently enabled. The Rest Mode state can be one of five states:"));
            localFields.Add(new OuraFields("Activity Total", "Activity", "ActivityTotal", 86));

            localFields.Add(new OuraFields("Readiness Period Id", "Readiness", "ReadinessPeriodId", 95, "Index of the sleep period among sleep periods with the same summary_date, where 0 = first sleep period of the day. Each readinesss calculation is associated with a sleep period."));
            localFields.Add(new OuraFields("Readiness Score", "Readiness", "ReadinessScore", 96));
            localFields.Add(new OuraFields("Score Activity Balance", "Readiness", "ScoreActivityBalance", 97));
            localFields.Add(new OuraFields("Score HRV Balance", "Readiness", "ScoreHRVBalance", 98));
            localFields.Add(new OuraFields("Score Previous Day", "Readiness", "ScorePreviousDay", 99));
            localFields.Add(new OuraFields("Score Previous Night", "Readiness", "ScorePreviousNight", 100));
            localFields.Add(new OuraFields("Score Recovery Index", "Readiness", "ScoreRecoveryIndex", 101));
            localFields.Add(new OuraFields("Score Resting HR", "Readiness", "ScoreRestingHR", 102));
            localFields.Add(new OuraFields("Score Sleep Balance", "Readiness", "ScoreSleepBalance", 103));
            localFields.Add(new OuraFields("Score Temperature", "Readiness", "ScoreTemperature", 104));
            localFields.Add(new OuraFields("Readiness Rest Mode State", "Readiness", "ReadinessRestModeState", 105, "Indicates whether Rest Mode was enabled or recently enabled. The Rest Mode state can be one of five states: 0: Off; 1: Entering Rest Mode; 2: Rest Mode; 3: Entering recovery; 4: Recovering"));

            defaultFields = localFields;
        }

        /// <summary>
        /// Clear the singleton interface to force it to re-initialize on next request
        /// </summary>
        public static void ResetFields()
        {
            defaultFields = null;
        }

        /// <summary>
        /// Create Static singleton interface for the OuraFields definitions
        /// </summary>
        /// <returns></returns>
        public static List<OuraFields> CurrentFields()
        {
            if (defaultFields == null)
            {
                SetupDefaultFields();
            }
            return defaultFields;
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
        public OuraFields(string fieldName, string ouraSection, string methodName, int fieldOrder, bool accessible, AccessorType aType, string desc)
        {
            this.FieldName = fieldName;
            this.OuraSection = ouraSection;
            this.MethodName = methodName;
            this.Accessible = accessible;
            this.Accessor = aType;
            this.FieldOrder = fieldOrder;
            this.FieldDescription = desc;
        }


        // Call default constructor with some defaults
        private OuraFields(string fieldName, string ouraSection, string methodName, int fieldOrder, bool accessible, AccessorType aType) 
            : this(fieldName, ouraSection, methodName, fieldOrder, accessible, aType, null)
        {
        }

        // Call default constructor with some defaults
        private OuraFields(string fieldName, string ouraSection, string methodName, int fieldOrder, string desc) 
            : this(fieldName, ouraSection, methodName, fieldOrder, true, AccessorType.Property, desc)
        {
        }

        // Call default constructor with some defaults
        private OuraFields(string fieldName, string ouraSection, string methodName, int fieldOrder) 
            : this(fieldName, ouraSection, methodName, fieldOrder, true, AccessorType.Property, null)
        {
        }
    }
}
