using Microsoft.Office.Tools.Ribbon;
using Newtonsoft.Json;
using OuraAPIInterface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExcelOuraVSTOAddIn
{
    public partial class OuraRibbonBar
    {
        private bool exportInProgress;
        private int connectionRevision;

        private void OuraRibbonBar_Load(object sender, RibbonUIEventArgs args)
        {
            OuraConnection.ConfigureApi();
            UpdateConnectionState();
        }

        private void btnGetOuraData_Click(object sender, RibbonControlEventArgs args) =>
            ExecuteSafely(() => RunExport(OuraFields.CurrentFields, OuraExcelSettings.Default.Fields,
                json => OuraExcelSettings.Default.Fields = json, "Sleep, Activity and Readiness",
                (start, end, token, progress, options) => OuraAPI2Wrapper.GetAllOuraDataV2ForDateRangeAsync(start, end, token, progress, options)));

        private void btnDailyspo2_Click(object sender, RibbonControlEventArgs args) =>
            ExecuteSafely(() => RunExport(OuraFields.CurrentSpo2Fields, OuraExcelSettings.Default.Spo2Fields,
                json => OuraExcelSettings.Default.Spo2Fields = json, "SpO2",
                (start, end, token, progress, options) => OuraAPI2Wrapper.FetchTypedAsync<OuraAPIInterface.DailySpo2.Datum>("daily_spo2", start, end, token, progress, options)));

        private void btnWorkouts_Click(object sender, RibbonControlEventArgs args) =>
            ExecuteSafely(() => RunExport(OuraFields.CurrentWorkoutFields, OuraExcelSettings.Default.WorkoutFields,
                json => OuraExcelSettings.Default.WorkoutFields = json, "Workouts",
                (start, end, token, progress, options) => OuraAPI2Wrapper.FetchTypedAsync<OuraAPIInterface.Workouts.Datum>("workout", start, end, token, progress, options)));

        private void btnTags_Click(object sender, RibbonControlEventArgs args) =>
            ExecuteSafely(() => RunExport(OuraFields.CurrentTagFields, OuraExcelSettings.Default.TagFields,
                json => OuraExcelSettings.Default.TagFields = json, "Tags",
                (start, end, token, progress, options) => OuraAPI2Wrapper.FetchTypedAsync<OuraAPIInterface.EnhancedTags.Datum>("enhanced_tag", start, end, token, progress, options)));


        private void btnConfigureOuraAPIKey_Click(object sender, RibbonControlEventArgs args)
        {
            ExecuteSafely(() =>
            {
                using (var form = new ConfigureOuraToken())
                {
                    form.ConnectionChanged += (changed, eventArgs) => UpdateConnectionState();
                    form.ShowDialog();
                }
                UpdateConnectionState();
            });
        }

        private static void ExecuteSafely(Action action)
        {
            try { action(); }
            catch (Exception ex) { MessageBox.Show(OuraUserMessages.For(ex), "Oura"); }
        }

        private async void UpdateConnectionState()
        {
            int revision = ++connectionRevision;
            SetExportsEnabled(false);
            try
            {
                bool connected = await OuraConnection.IsConnectedAsync();
                if (revision == connectionRevision) SetExportsEnabled(connected && !exportInProgress);
            }
            catch (Exception ex)
            {
                if (revision == connectionRevision) MessageBox.Show(OuraUserMessages.For(ex), "Oura connection");
            }
        }

        private void SetExportsEnabled(bool enabled)
        {
            foreach (var button in exportButtons) button.Enabled = enabled;
        }

        private void RunExport<T>(Func<List<OuraFields>> defaults, string saved, Action<string> saveFields, string title,
            Func<DateTime, DateTime, CancellationToken, Action<int>, OuraFetchOptions, Task<List<T>>> fetch,
            bool useDateRange = true)
        {
            if (exportInProgress) return;
            exportInProgress = true;
            SetExportsEnabled(false);
            try
            {
                using (var form = CreateExportForm(defaults, saved, title, useDateRange))
                {
                    if (form.ShowDialog() != DialogResult.OK) return;
                    var selected = form.FormFields.Where(f => f.FieldSelected).OrderBy(f => f.FieldOrder).ToList();
                    if (selected.Count == 0) throw new OuraException("Select at least one field to export.");
                    using (var target = new ExcelExportTarget(Globals.ThisAddIn.Application))
                    {
                        int maximumRows = target.MaximumDataRows(selected.Count, form.IncludeHeaders);
                        SavePreferences(form, saveFields);
                        var start = form.StartDate;
                        var end = form.EndDate;
                        bool headers = form.IncludeHeaders;
                        var table = OuraProgressDialog.Run("Retrieving " + title, async (token, progress) =>
                        {
                            var rows = await fetch(start, end, token, pages => progress.Report(pages), new OuraFetchOptions(maximumRows)).ConfigureAwait(false);
                            if (rows.Count == 0) return null;
                            return OuraExportTable.Build(rows, selected, headers, token);
                        });
                        if (table == null)
                        {
                            MessageBox.Show("No " + title + " data was returned. Availability depends on the selected dates, your ring and approved permissions.", "Oura export");
                            return;
                        }
                        string warning = target.Write(table, selected, headers, headers && form.IncludeDescriptions);
                        if (warning != null) MessageBox.Show(warning, "Oura export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(OuraUserMessages.For(ex), "Oura export", MessageBoxButtons.OK,
                    ex is OperationCanceledException ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            }
            finally
            {
                exportInProgress = false;
                UpdateConnectionState();
            }
        }

        private static OuraSetupTransferForm CreateExportForm(Func<List<OuraFields>> defaults, string saved, string title, bool useDateRange)
        {
            var settings = OuraExcelSettings.Default;
            var form = new OuraSetupTransferForm
            {
                FormFields = OuraFieldSelections.Merge(defaults(), saved),
                DefaultsProvider = defaults,
                FormHeader = "Export " + title,
                UseDateRange = useDateRange,
                IncludeHeaders = settings.IncludeHeaders,
                IncludeDescriptions = settings.IncludeDescriptions,
                StartDate = settings.StartDate,
                EndDate = settings.EndDate
            };
            if (settings.FormSize.Width >= form.MinimumSize.Width && settings.FormSize.Height >= form.MinimumSize.Height &&
                settings.FormSize.Width <= 3000 && settings.FormSize.Height <= 2000)
                form.Size = settings.FormSize;
            var rectangle = new Rectangle(settings.FormLocation, form.Size);
            if (settings.FormLocation != Point.Empty && Screen.AllScreens.Any(screen =>
                Rectangle.Intersect(screen.WorkingArea, rectangle).Width >= 100 && Rectangle.Intersect(screen.WorkingArea, rectangle).Height >= 80))
            {
                form.StartPosition = FormStartPosition.Manual;
                form.Location = settings.FormLocation;
            }
            return form;
        }

        private static void SavePreferences(OuraSetupTransferForm form, Action<string> saveFields)
        {
            var settings = OuraExcelSettings.Default;
            settings.IncludeHeaders = form.IncludeHeaders;
            settings.IncludeDescriptions = form.IncludeDescriptions;
            settings.FormSize = form.Size;
            settings.FormLocation = form.Location;
            if (form.UseDateRange)
            {
                settings.StartDate = form.StartDate;
                settings.EndDate = form.EndDate.Date == DateTime.Today ? DateTime.MinValue : form.EndDate;
            }
            saveFields(JsonConvert.SerializeObject(form.FormFields));
            try { settings.Save(); }
            catch (Exception ex) when (ex is ConfigurationErrorsException || ex is IOException || ex is UnauthorizedAccessException)
            {
                MessageBox.Show("This export can continue, but its preferences could not be saved. Check your Windows profile storage.",
                    "Oura preferences", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
