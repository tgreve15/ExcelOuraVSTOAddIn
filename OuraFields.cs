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
        /// open the interface since the interface will allow you to modify the order results
        /// are returned to Excel
        /// </summary>
        private static void SetupDefaultFields()
        {
            List<OuraFields> localFields = new List<OuraFields>();
            localFields.Add(new OuraFields("Summary Date", "Sleep", "SummaryDate" ,1));
            localFields.Add(new OuraFields("Sleep Period Id", "Sleep", "SleepPeriodId", 2));
            localFields.Add(new OuraFields("Is Longest", "Sleep", "IsLongest" ,3 ));
            localFields.Add(new OuraFields("Sleep Timezone", "Sleep", "SleepTimezone",4));
            localFields.Add(new OuraFields("Bedtime Start", "Sleep", "BedtimeStart",5));
            localFields.Add(new OuraFields("Bedtime End", "Sleep", "BedtimeEnd",6));
            localFields.Add(new OuraFields("Bedtime Start (Local)", "Sleep", "BedtimeStartFormatLocal",7, true, AccessorType.Method));
            localFields.Add(new OuraFields("Bedtime End (Local)", "Sleep", "BedtimeEndFormatLocal",8, true, AccessorType.Method));
            localFields.Add(new OuraFields("Sleep Score", "Sleep", "SleepScore",9));
            localFields.Add(new OuraFields("Score Total", "Sleep", "ScoreTotal" ,10));
            localFields.Add(new OuraFields("Score Disturbances", "Sleep", "ScoreDisturbances", 11));
            localFields.Add(new OuraFields("Score Efficiency", "Sleep", "ScoreEfficiency", 12));
            localFields.Add(new OuraFields("Score Latency", "Sleep", "ScoreLatency", 13));
            localFields.Add(new OuraFields("Score REM", "Sleep", "ScoreREM", 14));
            localFields.Add(new OuraFields("Score Deep", "Sleep", "ScoreDeep", 15));
            localFields.Add(new OuraFields("Score Alignment", "Sleep", "ScoreAlignment", 16));
            localFields.Add(new OuraFields("Sleep Total", "Sleep", "SleepTotal", 17));
            localFields.Add(new OuraFields("Duration", "Sleep", "Duration", 18));
            localFields.Add(new OuraFields("Awake Time", "Sleep", "Awake", 19));
            localFields.Add(new OuraFields("Light Time", "Sleep", "Light", 20));
            localFields.Add(new OuraFields("REM Sleep", "Sleep", "REM", 21));
            localFields.Add(new OuraFields("Deep Sleep", "Sleep", "Deep", 22));
            localFields.Add(new OuraFields("Onset Latency", "Sleep", "OnsetLatency", 23));
            localFields.Add(new OuraFields("Restless", "Sleep", "Restless", 24));
            localFields.Add(new OuraFields("Efficiency", "Sleep", "Efficiency", 25));
            localFields.Add(new OuraFields("Midpoint Time", "Sleep", "MidpointTime", 26));
            localFields.Add(new OuraFields("HR Lowest", "Sleep", "HRLowest", 27));
            localFields.Add(new OuraFields("HR Average", "Sleep", "HRAverage", 28));
            localFields.Add(new OuraFields("RMSSD", "Sleep", "RMSSD", 29));
            localFields.Add(new OuraFields("Breath Average", "Sleep", "BreathAverage", 30));
            localFields.Add(new OuraFields("Temperature Delta", "Sleep", "TemperatureDelta", 31));
            localFields.Add(new OuraFields("Hypnogram 5 Min", "Sleep", "Hypnogram5Min", 32, false, AccessorType.Property));
            localFields.Add(new OuraFields("HR 5 Min", "Sleep", "HR5Min", 33, false, AccessorType.Property));
            localFields.Add(new OuraFields("RMSSD 5 Min", "Sleep", "RMSSD5Min", 34, false, AccessorType.Property));
            localFields.Add(new OuraFields("Temperature Deviation", "Sleep", "TemperatureDeviation", 35));
            localFields.Add(new OuraFields("Temperature Trend Deviation", "Sleep", "TemperatureTrendDeviation", 36));
            localFields.Add(new OuraFields("Bedtime Start Delta", "Sleep", "BedtimeStartDelta", 37));
            localFields.Add(new OuraFields("Bedtime End Delta", "Sleep", "BedtimeEndDelta", 38));
            localFields.Add(new OuraFields("Midpoint At Delta", "Sleep", "MidpointAtDelta", 39));

            localFields.Add(new OuraFields("Activity Timezone", "Activity", "ActivityTimezone", 50));
            localFields.Add(new OuraFields("Day Start", "Activity", "DayStart", 51));
            localFields.Add(new OuraFields("Day End", "Activity", "DayEnd", 52));
            localFields.Add(new OuraFields("Day Start (local)", "Activity", "DayStartFormatLocal", 53, true, AccessorType.Method));
            localFields.Add(new OuraFields("Day End (local)", "Activity", "DayEndFormatLocal", 54, true, AccessorType.Method));
            localFields.Add(new OuraFields("Cal Active", "Activity", "CalActive", 55));
            localFields.Add(new OuraFields("Cal Total", "Activity", "CalTotal", 56));
            localFields.Add(new OuraFields("Class 5 min", "Activity", "Class5min", 57, false, AccessorType.Property));
            localFields.Add(new OuraFields("Steps", "Activity", "Steps", 58));
            localFields.Add(new OuraFields("Daily Movement", "Activity", "DailyMovement", 59));
            localFields.Add(new OuraFields("Non Wear", "Activity", "NonWear", 60));
            localFields.Add(new OuraFields("Rest", "Activity", "Rest", 61));
            localFields.Add(new OuraFields("Inactive", "Activity", "Inactive", 62));
            localFields.Add(new OuraFields("Low Activity", "Activity", "Low", 63));
            localFields.Add(new OuraFields("Medium Activity", "Activity", "Medium", 64));
            localFields.Add(new OuraFields("High Activity", "Activity", "High", 65));
            localFields.Add(new OuraFields("Inactivity Alerts", "Activity", "InactivityAlerts", 66));
            localFields.Add(new OuraFields("Average Met", "Activity", "AverageMet", 67));
            localFields.Add(new OuraFields("Met 1 min", "Activity", "Met1min", 68, false, AccessorType.Property));
            localFields.Add(new OuraFields("Met Min Inactive", "Activity", "MetMinInactive", 69));
            localFields.Add(new OuraFields("Met Min Low", "Activity", "MetMinLow", 70));
            localFields.Add(new OuraFields("Met Min Medium", "Activity", "MetMinMedium", 71));
            localFields.Add(new OuraFields("Met Min High", "Activity", "MetMinHigh", 72));
            localFields.Add(new OuraFields("Target Calories", "Activity", "TargetCalories", 73));
            localFields.Add(new OuraFields("Target KM", "Activity", "TargetKM", 74));
            localFields.Add(new OuraFields("Target Miles", "Activity", "TargetMiles", 75));
            localFields.Add(new OuraFields("To Target KM", "Activity", "ToTargetKM", 76));
            localFields.Add(new OuraFields("To Target Miles", "Activity", "ToTargetMiles", 77));
            localFields.Add(new OuraFields("Activity Score", "Activity", "ActivityScore", 78));
            localFields.Add(new OuraFields("Score Meet Daily Targets", "Activity", "ScoreMeetDailyTargets", 79));
            localFields.Add(new OuraFields("Score Move Every Hour", "Activity", "ScoreMoveEveryHour", 80));
            localFields.Add(new OuraFields("Score Recovery Time", "Activity", "ScoreRecoveryTime", 81));
            localFields.Add(new OuraFields("Score Stay Active", "Activity", "ScoreStayActive", 82));
            localFields.Add(new OuraFields("Score Training Frequency", "Activity", "ScoreTrainingFrequency", 83));
            localFields.Add(new OuraFields("Score Training Volume", "Activity", "ScoreTrainingVolume", 84));
            localFields.Add(new OuraFields("Activity Rest Mode State", "Activity", "ActivityRestModeState", 85));
            localFields.Add(new OuraFields("Activity Total", "Activity", "ActivityTotal", 86));

            localFields.Add(new OuraFields("Readiness Period Id", "Readiness", "ReadinessPeriodId", 95));
            localFields.Add(new OuraFields("Readiness Score", "Readiness", "ReadinessScore", 96));
            localFields.Add(new OuraFields("Score Activity Balance", "Readiness", "ScoreActivityBalance", 97));
            localFields.Add(new OuraFields("Score HRV Balance", "Readiness", "ScoreHRVBalance", 98));
            localFields.Add(new OuraFields("Score Previous Day", "Readiness", "ScorePreviousDay", 99));
            localFields.Add(new OuraFields("Score Previous Night", "Readiness", "ScorePreviousNight", 100));
            localFields.Add(new OuraFields("Score Recovery Index", "Readiness", "ScoreRecoveryIndex", 101));
            localFields.Add(new OuraFields("Score Resting HR", "Readiness", "ScoreRestingHR", 102));
            localFields.Add(new OuraFields("Score Sleep Balance", "Readiness", "ScoreSleepBalance", 103));
            localFields.Add(new OuraFields("Score Temperature", "Readiness", "ScoreTemperature", 104));
            localFields.Add(new OuraFields("Readiness Rest Mode State", "Readiness", "ReadinessRestModeState", 105));

            defaultFields = localFields;
        }

        /// <summary>
        /// Clear the singular interface to force it to re-initialize on next request
        /// </summary>
        public static void ResetFields()
        {
            defaultFields = null;
        }

        /// <summary>
        /// Create Static singular interface for the OuraFields definitions
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

        /// <summary>
        /// Constructor for OuraFields allowing greater configuration
        /// </summary>
        /// <param name="fieldName">Name and Title for the field</param>
        /// <param name="ouraSection">Which section of Oura is this from? Sleep, Readiness, Activity</param>
        /// <param name="methodName">Name of the method/property on OuraCombinedObject to get this fields data</param>
        /// <param name="fieldOrder">Position to appear in the UI</param>
        /// <param name="accessible">Show this field in the UI?</param>
        /// <param name="aType">Type of accessor on OuraCombinedObject - Method or Property</param>
        public OuraFields(string fieldName, string ouraSection, string methodName, int fieldOrder, bool accessible, AccessorType aType)
        {
            FieldName = fieldName;
            OuraSection = ouraSection;
            MethodName = methodName;
            Accessible = accessible;
            Accessor = aType;
            this.FieldOrder = fieldOrder;
        }

        /// <summary>
        /// default Constructor for OuraFields with the minimum information required to be passed, 
        /// making assumptions for the other fields
        /// </summary>
        /// <param name="fieldName">Name and Title for the field</param>
        /// <param name="ouraSection">Which section of Oura is this from? Sleep, Readiness, Activity</param>
        /// <param name="methodName">Name of the method/property on OuraCombinedObject to get this fields data</param>
        /// <param name="fieldOrder">Position to appear in the UI</param>
        private OuraFields(string fieldName, string ouraSection, string methodName, int fieldOrder)
        {
            FieldName = fieldName;
            OuraSection = ouraSection;
            MethodName = methodName;
            Accessible = true;
            Accessor = AccessorType.Property;
            this.FieldOrder = fieldOrder;
        }
    }
}
