using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OuraAPIInterface
{

    // The majority of classes below were constructed with assistance from the
    // following web site. Once I had the data returned as a JSON string, just
    // plugged it into the website, then manipulated the results as desired 
    // https://json2csharp.com

    /// <summary>
    /// Oura has separate calls for Sleep, Activity and Readiness data. 
    /// To allow us to enter all information for a particular day, or set of days, 
    /// into an Excel row, combine them into a single object
    /// </summary>
    [DataContract]
    public class OuraCombinedObject
    {
        public string SummaryDate { get; set; }
        // Sleep Variables
        public double? SleepPeriodId { get; set; }
        public double? IsLongest { get; set; }
        public double? SleepTimezone { get; set; }
        public String BedtimeStart { get; set; }
        public String BedtimeEnd { get; set; }
        public double? SleepScore { get; set; }
        public double? ScoreTotal { get; set; }
        public double? ScoreDisturbances { get; set; }
        public double? ScoreEfficiency { get; set; }
        public double? ScoreLatency { get; set; }
        public double? ScoreREM { get; set; }
        public double? ScoreDeep { get; set; }
        public double? ScoreAlignment { get; set; }
        public double? SleepTotal { get; set; }
        public double? Duration { get; set; }
        public double? Awake { get; set; }
        public double? Light { get; set; }
        public double? REM { get; set; }
        public double? Deep { get; set; }
        public double? OnsetLatency { get; set; }
        public double? Restless { get; set; }
        public double? Efficiency { get; set; }
        public double? MidpointTime { get; set; }
        public double? HRLowest { get; set; }
        public double? HRAverage { get; set; }
        public double? RMSSD { get; set; }
        public double? BreathAverage { get; set; }
        public double? TemperatureDelta { get; set; }
        public string Hypnogram5Min { get; set; }
        public List<int> HR5Min { get; set; }
        public List<int> RMSSD5Min { get; set; }
        public double? TemperatureDeviation { get; set; }
        public double? TemperatureTrendDeviation { get; set; }
        public double? BedtimeStartDelta { get; set; }
        public double? BedtimeEndDelta { get; set; }
        public double? MidpointAtDelta { get; set; }
        public String Timestamp { get; set; }
        public String Type { get; set; }
        public double? AverageBreathVariation { get; set; }
        public double? GotUpCount { get; set; }
        public double? WakeUpCount { get; set; }
        public double? LowestHeartRateTimeOffset { get; set; }

        // Activity Variables
        public double? ActivityTimezone { get; set; }
        public String DayStart { get; set; }
        public String DayEnd { get; set; }
        public double? CalActive { get; set; }
        public double? CalTotal { get; set; }
        public string Class5min { get; set; }
        public double? Steps { get; set; }
        public double? DailyMovement { get; set; }
        public double? NonWear { get; set; }
        public double? Rest { get; set; }
        public double? Inactive { get; set; }
        public double? Low { get; set; }
        public double? Medium { get; set; }
        public double? High { get; set; }
        public double? InactivityAlerts { get; set; }
        public double? AverageMet { get; set; }
        public List<double> Met1min { get; set; }
        public double? MetMinInactive { get; set; }
        public double? MetMinLow { get; set; }
        public double? MetMinMedium { get; set; }
        public double? MetMinHigh { get; set; }
        public double? TargetCalories { get; set; }
        public double? TargetKM { get; set; }
        public double? TargetMiles { get; set; }
        public double? ToTargetKM { get; set; }
        public double? ToTargetMiles { get; set; }
        public double? ActivityScore { get; set; }
        public double? ScoreMeetDailyTargets { get; set; }
        public double? ScoreMoveEveryHour { get; set; }
        public double? ScoreRecoveryTime { get; set; }
        public double? ScoreStayActive { get; set; }
        public double? ScoreTrainingFrequency { get; set; }
        public double? ScoreTrainingVolume { get; set; }
        public double? ActivityRestModeState { get; set; }
        public double? ActivityTotal { get; set; }

        // Readiness Variables
        public double? ReadinessPeriodId { get; set; }
        public double? ReadinessScore { get; set; }
        public double? ScoreActivityBalance { get; set; }
        public double? ScoreHRVBalance { get; set; }
        public double? ScorePreviousDay { get; set; }
        public double? ScorePreviousNight { get; set; }
        public double? ScoreRecoveryIndex { get; set; }
        public double? ScoreRestingHR { get; set; }
        public double? ScoreSleepBalance { get; set; }
        public double? ScoreTemperature { get; set; }
        public double? ReadinessRestModeState { get; set; }

        public OuraCombinedObject(string summaryDate)
        {
            SummaryDate = summaryDate;
        }

        public String BedtimeStartFormatLocal()
        {
            // TG: I tried converting to a DateTime object, which will work for most,
            // but since I have lived in multiple timezones since using Oura all earlier 
            // dates were relative to my current location, not my location at the time.
            // As such, just return a string for Excel to handle, excluding timezone adjustment
            // Example values
            //      2019-05-06T10:58:01+10:00 -> 2019-05-06 10:58:01
            //      2019-02-21T12:39:28-08:00 -> 2019-02-21 12:39:28
            //      2018-10-28T12:37:27-07:00 -> 2018-10-28 12:37:27
            //  return Convert.ToDateTime(BedtimeStart).ToLocalTime();
            String localDate = BedtimeStart;
            if (String.IsNullOrEmpty(localDate))
                return "";

            localDate = localDate.Substring(0, 19); // Timezone related information all at same place
            return localDate.Replace("T", " ");
        }

        public String BedtimeEndFormatLocal()
        {
            String localDate = BedtimeEnd;
            if (String.IsNullOrEmpty(localDate))
                return "";

            localDate = localDate.Substring(0, 19);
            return localDate.Replace("T", " ");
        }

        /// <summary>
        /// TimeStamp - only appears to have the date, no time component
        /// even though there is space in the return for time and potentially timezone
        /// </summary>
        /// <returns>Date as a string (yyyy-MM-dd)</returns>
        public String TimestampFormatLocal()
        {
            String localDate = Timestamp;
            if (String.IsNullOrEmpty(localDate))
                return "";

            return localDate.Substring(0, 10);
        }

        /// <summary>
        /// Activity>DayStart Property
        /// </summary>
        public String DayStartFormatLocal()
        {
            String localDate = DayStart;
            if (String.IsNullOrEmpty(localDate))
                return "";

            localDate = localDate.Substring(0, 19); // Timezone related information all at same place
            return localDate.Replace("T", " ");
        }

        /// <summary>
        /// Activity>DayEnd Property
        /// </summary>
        public String DayEndFormatLocal()
        {
            String localDate = DayEnd;
            if (String.IsNullOrEmpty(localDate))
                return "";

            localDate = localDate.Substring(0, 19); // Timezone related information all at same place
            return localDate.Replace("T", " ");
        }

        /// <summary>
        /// Take one day's worth of data from each call, and copy them into this CombinedObject
        /// for reporting and processing. It is likely to get one day with only activity data and
        /// no other, so be prepared if ANY responses don't exist
        /// </summary>
        /// <param name="sr">SleepResponse web response object</param>
        /// <param name="rr">ReadinessResponse web response object</param>
        /// <param name="ar">ActivityResponse web response object</param>
        public void UpdateFrom(SleepResponse sr, ReadinessResponse rr, ActivityResponse ar)
        {
            if (sr != null)
            {
                //this.SummaryDate = sr.SummaryDate;
                this.SleepPeriodId = sr.PeriodId;
                this.IsLongest = sr.IsLongest;
                this.SleepTimezone = sr.Timezone;
                this.BedtimeStart = sr.BedtimeStart;
                this.BedtimeEnd = sr.BedtimeEnd;
                this.SleepScore = sr.Score;
                this.ScoreTotal = sr.ScoreTotal;
                this.ScoreDisturbances = sr.ScoreDisturbances;
                this.ScoreEfficiency = sr.ScoreEfficiency;
                this.ScoreLatency = sr.ScoreLatency;
                this.ScoreREM = sr.ScoreREM;
                this.ScoreDeep = sr.ScoreDeep;
                this.ScoreAlignment = sr.ScoreAlignment;
                this.SleepTotal = sr.Total;
                this.Duration = sr.Duration;
                this.Awake = sr.Awake;
                this.Light = sr.Light;
                this.REM = sr.REM;
                this.Deep = sr.Deep;
                this.OnsetLatency = sr.OnsetLatency;
                this.Restless = sr.Restless;
                this.Efficiency = sr.Efficiency;
                this.MidpointTime = sr.MidpointTime;
                this.HRLowest = sr.HRLowest;
                this.HRAverage = sr.HRAverage;
                this.RMSSD = sr.RMSSD;
                this.BreathAverage = sr.BreathAverage;
                this.TemperatureDelta = sr.TemperatureDelta;
                this.Hypnogram5Min = sr.Hypnogram5Min;
                this.HR5Min = sr.HR5Min;
                this.RMSSD5Min = sr.RMSSD5Min;

                this.TemperatureDeviation = sr.TemperatureTrendDeviation;
                this.TemperatureTrendDeviation = sr.TemperatureTrendDeviation;
                this.BedtimeStartDelta = sr.BedtimeStartDelta;
                this.BedtimeEndDelta = sr.BedtimeStartDelta;
                this.MidpointAtDelta = sr.MidpointAtDelta;

                this.Timestamp = sr.Timestamp;
                this.Type = sr.Type;
                this.AverageBreathVariation = sr.AverageBreathVariation;
                this.GotUpCount = sr.GotUpCount;
                this.WakeUpCount = sr.WakeUpCount;
                this.LowestHeartRateTimeOffset = sr.LowestHeartRateTimeOffset;
            }
            if (rr != null)
            {
                //if (String.IsNullOrEmpty(this.SummaryDate))
                //    this.SummaryDate = rr.SummaryDate;
                this.ReadinessPeriodId = rr.PeriodId;
                this.ReadinessScore = rr.Score;
                this.ScoreActivityBalance = rr.ScoreActivityBalance;
                this.ScoreHRVBalance = rr.ScoreHRVBalance;
                this.ScorePreviousDay = rr.ScorePreviousDay;
                this.ScorePreviousNight = rr.ScorePreviousNight;
                this.ScoreRecoveryIndex = rr.ScoreRecoveryIndex;
                this.ScoreRestingHR = rr.ScoreRestingHR;
                this.ScoreSleepBalance = rr.ScoreSleepBalance;
                this.ScoreTemperature = rr.ScoreTemperature;
                this.ReadinessRestModeState = rr.RestModeState;
            }
            if (ar != null)
            {
                //if (String.IsNullOrEmpty(this.SummaryDate))
                //    this.SummaryDate = ar.SummaryDate;
                this.ActivityTimezone = ar.Timezone;
                this.DayStart = ar.DayStart;
                this.DayEnd = ar.DayEnd;
                this.CalActive = ar.CalActive;
                this.CalTotal = ar.CalTotal;
                this.Class5min = ar.Class5min;
                this.Steps = ar.Steps;
                this.DailyMovement = ar.DailyMovement;
                this.NonWear = ar.NonWear;
                this.Rest = ar.Rest;
                this.Inactive = ar.Inactive;
                this.Low = ar.Low;
                this.Medium = ar.Medium;
                this.High = ar.High;
                this.InactivityAlerts = ar.InactivityAlerts;
                this.AverageMet = ar.AverageMet;
                this.Met1min = ar.Met1min;
                this.MetMinInactive = ar.MetMinInactive;
                this.MetMinLow = ar.MetMinLow;
                this.MetMinMedium = ar.MetMinMedium;
                this.MetMinHigh = ar.MetMinHigh;
                this.TargetCalories = ar.TargetCalories;
                this.TargetKM = ar.TargetKM;
                this.TargetMiles = ar.TargetMiles;
                this.ToTargetKM = ar.ToTargetKM;
                this.ToTargetMiles = ar.ToTargetMiles;
                this.ActivityScore = ar.Score;
                this.ScoreMeetDailyTargets = ar.ScoreMeetDailyTargets;
                this.ScoreMoveEveryHour = ar.ScoreMoveEveryHour;
                this.ScoreRecoveryTime = ar.ScoreRecoveryTime;
                this.ScoreStayActive = ar.ScoreStayActive;
                this.ScoreTrainingFrequency = ar.ScoreTrainingFrequency;
                this.ScoreTrainingVolume = ar.ScoreTrainingVolume;
                this.ActivityRestModeState = ar.RestModeState;
                this.ActivityTotal = ar.Total;
            }
        }
    }

    /// <summary>
    /// Response object for User Information Request
    /// </summary>
    [DataContract]
    public class UserInfoResponse
    {
        [DataMember(Name = "age")]
        public string Age { get; set; }
        [DataMember(Name = "weight")]
        public string Weight { get; set; }
        [DataMember(Name = "height")]
        public string Height { get; set; }
        [DataMember(Name = "gender")]
        public string Gender { get; set; }
        [DataMember(Name = "email")]
        public string Email { get; set; }
    }

    /// <summary>
    /// Root class for all Sleep Summary data returned
    /// </summary>
    [DataContract]
    public class SleepSummaryResponse
    {
        [DataMember(Name = "sleep")]
        public SleepResponse[] Sleep { get; set; }
    }

    /// <summary>
    /// Details returned for each day in a sleep summary request
    /// </summary>
    [DataContract]
    public class SleepResponse
    {
        [DataMember(Name = "summary_date")]
        public string SummaryDate { get; set; }
        [DataMember(Name = "period_id")]
        public double? PeriodId { get; set; }
        [DataMember(Name = "is_longest")]
        public double? IsLongest { get; set; }
        [DataMember(Name = "timezone")]
        public double? Timezone { get; set; }
        [DataMember(Name = "bedtime_start")]
        public String BedtimeStart { get; set; }
        [DataMember(Name = "bedtime_end")]
        public String BedtimeEnd { get; set; }
        [DataMember(Name = "breath_average")]
        public double? BreathAverage { get; set; }
        [DataMember(Name = "duration")]
        public double? Duration { get; set; }
        [DataMember(Name = "total")]
        public double? Total { get; set; }
        [DataMember(Name = "awake")]
        public double? Awake { get; set; }
        [DataMember(Name = "rem")]
        public double? REM { get; set; }
        [DataMember(Name = "deep")]
        public double? Deep { get; set; }
        [DataMember(Name = "light")]
        public double? Light { get; set; }
        [DataMember(Name = "midpoint_time")]
        public double? MidpointTime { get; set; }
        [DataMember(Name = "efficiency")]
        public double? Efficiency { get; set; }
        [DataMember(Name = "restless")]
        public double? Restless { get; set; }
        [DataMember(Name = "onset_latency")]
        public double? OnsetLatency { get; set; }
        [DataMember(Name = "hr_5min")]
        public List<int> HR5Min { get; set; }
        [DataMember(Name = "hr_average")]
        public double? HRAverage { get; set; }
        [DataMember(Name = "hr_lowest")]
        public double? HRLowest { get; set; }
        [DataMember(Name = "hypnogram_5min")]
        public string Hypnogram5Min { get; set; }
        [DataMember(Name = "rmssd")]
        public double? RMSSD { get; set; }
        [DataMember(Name = "rmssd_5min")]
        public List<int> RMSSD5Min { get; set; }
        [DataMember(Name = "score")]
        public double? Score { get; set; }
        [DataMember(Name = "score_total")]
        public double? ScoreTotal { get; set; }
        [DataMember(Name = "score_disturbances")]
        public double? ScoreDisturbances { get; set; }
        [DataMember(Name = "score_efficiency")]
        public double? ScoreEfficiency { get; set; }
        [DataMember(Name = "score_latency")]
        public double? ScoreLatency { get; set; }
        [DataMember(Name = "score_rem")]
        public double? ScoreREM { get; set; }
        [DataMember(Name = "score_deep")]
        public double? ScoreDeep { get; set; }
        [DataMember(Name = "score_alignment")]
        public double? ScoreAlignment { get; set; }
        [DataMember(Name = "temperature_deviation")]
        public double? TemperatureDeviation { get; set; }
        [DataMember(Name = "temperature_trend_deviation")]
        public double? TemperatureTrendDeviation { get; set; }
        [DataMember(Name = "bedtime_start_delta")]
        public double? BedtimeStartDelta { get; set; }
        [DataMember(Name = "bedtime_end_delta")]
        public double? BedtimeEndDelta { get; set; }
        [DataMember(Name = "midpoint_at_delta")]
        public double? MidpointAtDelta { get; set; }
        [DataMember(Name = "temperature_delta")]
        public double? TemperatureDelta { get; set; }

        /// <summary>
        /// Following fields are new and doesn't appear in the list
        /// </summary>
        [DataMember(Name = "timestamp")]
        public String Timestamp { get; set; }
        [DataMember(Name = "type")]
        public String Type { get; set; }
        [DataMember(Name = "average_breath_variation")]
        public double? AverageBreathVariation { get; set; }
        [DataMember(Name = "got_up_count")]
        public double? GotUpCount { get; set; }
        [DataMember(Name = "wake_up_count")]
        public double? WakeUpCount { get; set; }
        [DataMember(Name = "lowest_heart_rate_time_offset")]
        public double? LowestHeartRateTimeOffset { get; set; }

        public String BedtimeStartFormatLocal()
        {
            // TG: I tried converting to a DateTime object, which will work for most,
            // but since I have lived in multiple timezones since using Oura all earlier 
            // dates were relative to my current location, not my location at the time.
            // As such, just return a string for Excel to handle excluding timezone adjustment
            // Example values
            //      2019-05-06T10:58:01+10:00 -> 2019-05-06 10:58:01
            //      2019-02-21T12:39:28-08:00 -> 2019-02-21 12:39:28
            //      2018-10-28T12:37:27-07:00 -> 2018-10-28 12:37:27
            //  return Convert.ToDateTime(BedtimeStart).ToLocalTime();
            String localDate = BedtimeStart;
            if (String.IsNullOrEmpty(localDate))
                return "";

            localDate = localDate.Substring(0, 19); // Timezone related information all at same place
            return localDate.Replace("T", " ");
        }

        public String BedtimeEndFormatLocal()
        {
            String localDate = BedtimeEnd;
            if (String.IsNullOrEmpty(localDate))
                return "";

            localDate = localDate.Substring(0, 19);
            return localDate.Replace("T", " ");
        }

        /// <summary>
        /// TimeStamp - only appears to have the date, no time component
        /// even though there is space in the return for time and potentially timezone
        /// </summary>
        /// <returns>Date as a string (yyyy-MM-dd)</returns>
        public String TimestampFormatLocal()
        {
            String localDate = Timestamp;
            if (String.IsNullOrEmpty(localDate))
                return "";

            return localDate.Substring(0, 10);
        }
    }

    /// <summary>
    /// Details returned for each day in an Activity summary request
    /// </summary>
    [DataContract]
    public class ActivityResponse
    {
        [DataMember(Name = "summary_date")]
        public string SummaryDate { get; set; }
        [DataMember(Name = "timezone")]
        public double? Timezone { get; set; }
        [DataMember(Name = "day_start")]
        public String DayStart { get; set; }
        [DataMember(Name = "day_end")]
        public String DayEnd { get; set; }
        [DataMember(Name = "cal_active")]
        public double? CalActive { get; set; }
        [DataMember(Name = "cal_total")]
        public double? CalTotal { get; set; }
        [DataMember(Name = "class_5min")]
        public string Class5min { get; set; }
        [DataMember(Name = "steps")]
        public double? Steps { get; set; }
        [DataMember(Name = "daily_movement")]
        public double? DailyMovement { get; set; }
        [DataMember(Name = "non_wear")]
        public double? NonWear { get; set; }
        [DataMember(Name = "rest")]
        public double? Rest { get; set; }
        [DataMember(Name = "inactive")]
        public double? Inactive { get; set; }
        [DataMember(Name = "low")]
        public double? Low { get; set; }
        [DataMember(Name = "medium")]
        public double? Medium { get; set; }
        [DataMember(Name = "high")]
        public double? High { get; set; }
        [DataMember(Name = "inactivity_alerts")]
        public double? InactivityAlerts { get; set; }
        [DataMember(Name = "average_met")]
        public double? AverageMet { get; set; }
        [DataMember(Name = "met_1min")]
        public List<double> Met1min { get; set; }
        [DataMember(Name = "met_min_inactive")]
        public double? MetMinInactive { get; set; }
        [DataMember(Name = "met_min_low")]
        public double? MetMinLow { get; set; }
        [DataMember(Name = "met_min_medium")]
        public double? MetMinMedium { get; set; }
        [DataMember(Name = "met_min_high")]
        public double? MetMinHigh { get; set; }
        [DataMember(Name = "target_calories")]
        public double? TargetCalories { get; set; }
        [DataMember(Name = "target_km")]
        public double? TargetKM { get; set; }
        [DataMember(Name = "target_miles")]
        public double? TargetMiles { get; set; }
        [DataMember(Name = "to_target_km")]
        public double? ToTargetKM { get; set; }
        [DataMember(Name = "to_target_miles")]
        public double? ToTargetMiles { get; set; }
        [DataMember(Name = "score")]
        public double? Score { get; set; }
        [DataMember(Name = "score_meet_daily_targets")]
        public double? ScoreMeetDailyTargets { get; set; }
        [DataMember(Name = "score_move_every_hour")]
        public double? ScoreMoveEveryHour { get; set; }
        [DataMember(Name = "score_recovery_time")]
        public double? ScoreRecoveryTime { get; set; }
        [DataMember(Name = "score_stay_active")]
        public double? ScoreStayActive { get; set; }
        [DataMember(Name = "score_training_frequency")]
        public double? ScoreTrainingFrequency { get; set; }
        [DataMember(Name = "score_training_volume")]
        public double? ScoreTrainingVolume { get; set; }
        [DataMember(Name = "rest_mode_state")]
        public double? RestModeState { get; set; }
        [DataMember(Name = "total")]
        public double? Total { get; set; }

        /// <summary>
        /// Activity>DayStart Property
        /// </summary>
        public String DayStartFormatLocal()
        {
            String localDate = DayStart;
            if (String.IsNullOrEmpty(localDate))
                return "";

            localDate = localDate.Substring(0, 19); // Timezone related information all at same place
            return localDate.Replace("T", " ");
        }

        /// <summary>
        /// Activity>DayEnd Property
        /// </summary>
        public String DayEndFormatLocal()
        {
            String localDate = DayEnd;
            if (String.IsNullOrEmpty(localDate))
                return "";

            localDate = localDate.Substring(0, 19); // Timezone related information all at same place
            return localDate.Replace("T", " ");
        }
    }

    /// <summary>
    /// Root object for Activity Summary Requests
    /// </summary>
    [DataContract]
    public class ActivitySummaryResponse
    {
        [DataMember(Name = "activity")]
        public ActivityResponse[] Activity { get; set; }
    }

    /// <summary>
    /// Details returned for each day in a Readiness summary request
    /// </summary>
    [DataContract]
    public class ReadinessResponse
    {
        [DataMember(Name = "summary_date")]
        public string SummaryDate { get; set; }
        [DataMember(Name = "period_id")]
        public double? PeriodId { get; set; }
        [DataMember(Name = "score")]
        public double? Score { get; set; }
        [DataMember(Name = "score_activity_balance")]
        public double? ScoreActivityBalance { get; set; }
        [DataMember(Name = "score_hrv_balance")]
        public double? ScoreHRVBalance { get; set; }
        [DataMember(Name = "score_previous_day")]
        public double? ScorePreviousDay { get; set; }
        [DataMember(Name = "score_previous_night")]
        public double? ScorePreviousNight { get; set; }
        [DataMember(Name = "score_recovery_index")]
        public double? ScoreRecoveryIndex { get; set; }
        [DataMember(Name = "score_resting_hr")]
        public double? ScoreRestingHR { get; set; }
        [DataMember(Name = "score_sleep_balance")]
        public double? ScoreSleepBalance { get; set; }
        [DataMember(Name = "score_temperature")]
        public double? ScoreTemperature { get; set; }
        [DataMember(Name = "rest_mode_state")]
        public double? RestModeState { get; set; }
    }

    /// <summary>
    /// Root object for Readiness Summary Requests
    /// </summary>
    [DataContract]
    public class ReadinessSummaryResponse
    {
        [DataMember(Name = "readiness")]
        public ReadinessResponse[] Readiness { get; set; }
    }
}
