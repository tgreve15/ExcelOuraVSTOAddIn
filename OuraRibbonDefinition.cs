using OuraAPIInterface;
using System;
using System.Collections.Generic;

namespace ExcelOuraVSTOAddIn
{
    internal enum OuraRibbonCommand { DailyData, SpO2, Workouts, Tags, Collection, Connect }

    internal sealed class OuraRibbonButtonDefinition
    {
        internal string Name { get; }
        internal string Label { get; }
        internal string Icon { get; }
        internal string ScreenTip { get; }
        internal string SuperTip { get; }
        internal bool Large { get; }
        internal OuraRibbonCommand Command { get; }
        internal string Endpoint { get; }
        internal bool RequiresConnection => Command != OuraRibbonCommand.Connect;

        internal OuraRibbonButtonDefinition(string name, string label, string icon, OuraRibbonCommand command,
            string screenTip, string superTip, bool large = false, string endpoint = null)
        {
            Name = name;
            Label = label;
            Icon = icon;
            Command = command;
            ScreenTip = screenTip;
            SuperTip = superTip;
            Large = large;
            Endpoint = endpoint;
        }
    }

    internal sealed class OuraRibbonGroupDefinition
    {
        internal string Name { get; }
        internal string Label { get; }
        internal IReadOnlyList<OuraRibbonButtonDefinition> Buttons { get; }

        internal OuraRibbonGroupDefinition(string name, string label, params OuraRibbonButtonDefinition[] buttons)
        {
            Name = name;
            Label = label;
            Buttons = Array.AsReadOnly(buttons);
        }
    }

    /// <summary>The single ordered definition of every Oura tab, group, button, icon and tooltip.</summary>
    internal static class OuraRibbonDefinition
    {
        internal const string TabName = "tabOura";
        internal const string TabLabel = "Oura";
        private const string DatedExportTip = "Choose fields and dates, then export your approved Oura data at the selected cell.";
        private const string UndatedExportTip = "Choose fields and export all current records at the selected cell; dates do not apply.";

        internal static IReadOnlyList<OuraRibbonGroupDefinition> Groups { get; } = Array.AsReadOnly(new[]
        {
            new OuraRibbonGroupDefinition("grpOura", "Oura Commands",
                new OuraRibbonButtonDefinition("btnGetOuraData", "Get Oura Data", "oura", OuraRibbonCommand.DailyData,
                    "Retrieve sleep, activity and readiness", DatedExportTip, true),
                Export("session", "sessions", large: true, name: "btnSessions"),
                new OuraRibbonButtonDefinition("btnWorkouts", "Workouts", "workouts", OuraRibbonCommand.Workouts,
                    "Export recorded workouts", DatedExportTip, true),
                new OuraRibbonButtonDefinition("btnTags", "Tags", "tags", OuraRibbonCommand.Tags,
                    "Export your Oura tags", DatedExportTip, true),
                Export("heartrate", "heart-rate", false, "btnGetOuraHeartRates", "Get Heart Rate"),
                new OuraRibbonButtonDefinition("btnDailyspo2", "Daily SpO2", "spo2", OuraRibbonCommand.SpO2,
                    "Export daily oxygen saturation", DatedExportTip),
                Export("sleep", "sleep")),
            new OuraRibbonGroupDefinition("grpOuraWellbeing", "Wellbeing",
                Export("daily_stress", "stress"),
                Export("daily_resilience", "resilience"),
                Export("sleep_time", "bedtime"),
                Export("rest_mode_period", "rest-mode")),
            new OuraRibbonGroupDefinition("grpOuraHeartHealth", "Heart Health",
                Export("daily_cardiovascular_age", "cardiovascular-age"),
                Export("vO2_max", "vo2-max")),
            new OuraRibbonGroupDefinition("grpOuraRingandAccount", "Ring and Account",
                Export("ring_configuration", "ring-details"),
                Export("ring_battery_level", "battery"),
                Export("personal_info", "profile"),
                new OuraRibbonButtonDefinition("btnConfigureOuraAPIKey", "Connect Oura", "oura", OuraRibbonCommand.Connect,
                    "Connect your Oura account", "Sign in with Oura, manage your application credentials, or disconnect this computer.", true))
        });

        private static OuraRibbonButtonDefinition Export(string endpoint, string icon, bool large = false,
            string name = null, string label = null)
        {
            var collection = OuraCollections.Get(endpoint);
            return new OuraRibbonButtonDefinition(name ?? "btnOura_" + endpoint, label ?? collection.Label, icon,
                OuraRibbonCommand.Collection, "Export " + collection.Label,
                collection.DateMode == OuraDateMode.All || collection.DateMode == OuraDateMode.Single ? UndatedExportTip : DatedExportTip,
                large, endpoint);
        }
    }
}
