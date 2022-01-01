using Microsoft.Office.Tools.Ribbon;
using Newtonsoft.Json;
using OuraAPIInterface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelOuraVSTOAddIn
{
    public partial class OuraRibbonBar
    {
        /// <summary>
        /// Pre-Initialize the add-in. If there is no OuraAPIKey configured the add-in won't work,
        /// so warn about this and disable the add-in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OuraRibbonBar_Load(object sender, RibbonUIEventArgs e)
        {
            // Only show the Get Heart Rate data button when we are in the debugger
            // At least until working out if it's worth keeping
            if (System.Diagnostics.Debugger.IsAttached)
            {
                btnGetOuraHeartRates.Visible = true;
            }

            // Verify that the Oura API Token has been configured, if not request them to get one and configure it
            if (String.IsNullOrEmpty(ConfigurationManager.AppSettings["OuraAPIKey"]))
            {
                //
                // To use this add-in, you need to expose a Personal Access Token from the Oura Cloud Dashboard
                // from the following location - https://cloud.ouraring.com/personal-access-tokens
                // Once you've created the token, update the key in "app.Config" / "ExcelOuraVSTOAddIn.dll.config" to reflect this value
                //
                btnGetOuraData.Enabled = false;
                String message = "Oura Personal Access Token not configured. \n" +
                    "Go to the 'Configure Oura Token' command and configure this as instructed.\n" +
                    "Oura Excel integration disabled.";
                MessageBox.Show(message, "Excel Oura Add-In Disabled");
                return;
            }
        }

        /// <summary>
        /// Initiate the Add-In once the user clicked on the icon on the ribbon bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGetOuraData_Click(object sender, RibbonControlEventArgs e)
        {
            // Get configuration information from the user
            OuraSetupTransferForm form = new OuraSetupTransferForm();

            // Load user settings
            form.IncludeHeaders = OuraExcelSettings.Default.IncludeHeaders;
            form.IncludeDescriptions = OuraExcelSettings.Default.IncludeDescriptions;
            if (OuraExcelSettings.Default.FormSize != System.Drawing.Size.Empty)
                form.Size = OuraExcelSettings.Default.FormSize;
            if (OuraExcelSettings.Default.FormLocation != System.Drawing.Point.Empty) // And it is visible on the screen??  && OuraExcelSettings.Default.FormSize < Screen.
                form.Location = OuraExcelSettings.Default.FormLocation;

            if (!String.IsNullOrEmpty(OuraExcelSettings.Default.Fields))
            {
                // Import the fields list and update the modifiable bits of the system fields
                // Don't trust that what is in the configuration file matches the system settings
                List<OuraFields> localFields = JsonConvert.DeserializeObject<List<OuraFields>>(OuraExcelSettings.Default.Fields);
                if (!(localFields is null) && (localFields.Count > 0))
                {
                    foreach (OuraFields aField in localFields)
                    {
                        OuraFields f = OuraFields.CurrentFields().FirstOrDefault(i => i.FieldName == aField.FieldName);
                        if (f != null)
                        {
                            f.CustomLabel = aField.CustomLabel;
                            f.FieldOrder = aField.FieldOrder;
                            f.FieldSelected = aField.FieldSelected;
                        }
                    }
                }
            }

            // TODO: Mixed minds about storing the Start Date versus just letting it default to 10 days before today.
            // Just running a few times on a date a few years back, but perhaps not normal use?
            form.StartDate = OuraExcelSettings.Default.StartDate;
            form.EndDate = OuraExcelSettings.Default.EndDate;   // Only set if the last time wasn't using the current day

            // Display the Configuration form
            DialogResult result = form.ShowDialog();

            if (result != DialogResult.OK)
            {
                // User chose to Cancel
                return;
            }
            else
            {
                // Store the selected values in settings for the next use
                OuraExcelSettings.Default.IncludeHeaders = form.IncludeHeaders;
                OuraExcelSettings.Default.IncludeDescriptions = form.IncludeDescriptions;

                OuraExcelSettings.Default.FormSize = form.Size;
                OuraExcelSettings.Default.FormLocation = form.Location;
                OuraExcelSettings.Default.Fields = JsonConvert.SerializeObject(OuraFields.CurrentFields());
                OuraExcelSettings.Default.StartDate = form.StartDate;

                // If the end date was the current date, don't store the enddate
                // that way next time it will default to that day.
                if (form.EndDate != DateTime.Today)
                    OuraExcelSettings.Default.EndDate = form.EndDate;

                OuraExcelSettings.Default.Save();
            }

            // Initialize Excel fields
            Excel.Worksheet activeWorksheet = ((Excel.Worksheet)Globals.ThisAddIn.Application.ActiveSheet);
            Excel.Range activeCell = Globals.ThisAddIn.Application.ActiveCell;
            int startColumn = activeCell.Column;
            int startRow = activeCell.Row;
            int currentRow = startRow;
            Excel.Range allCells = Globals.ThisAddIn.Application.Cells;

            // Request data from Oura for the selected date range
            SleepSummaryResponse sleepResponse = OuraAPIWrapper.PerformSleepSummaryRequest(form.StartDate, form.EndDate);
            ActivitySummaryResponse activityResponse = OuraAPIWrapper.PerformActivitySummaryRequest(form.StartDate, form.EndDate);
            ReadinessSummaryResponse readinessResponse = OuraAPIWrapper.PerformReadinessSummaryRequest(form.StartDate, form.EndDate);

            // Consolidate all the Oura Data into a single object so we can expose data across what is collected side by side
            List<OuraCombinedObject> ouraObjects = new List<OuraCombinedObject>();

            // If one or more of the service requests didn't work, just quit as there is something bigger going wrong
            if (activityResponse != null && readinessResponse != null && sleepResponse != null)
            {
                // Every day will have some amount of activity information, even if just woke up,
                // but there may not be sleep and readiness data. As such there will likely be a day
                // with only activity data.
                for (int i = 0; i < activityResponse.Activity.Length; i++)
                {
                    // Arrays (i) are '0' based, so check if the count == i
                    SleepResponse sleep = (i == sleepResponse.Sleep.Count() ? null : sleepResponse.Sleep[i]);
                    ActivityResponse activity = (i == activityResponse.Activity.Count() ? null : activityResponse.Activity[i]);
                    ReadinessResponse readiness = (i == readinessResponse.Readiness.Count() ? null : readinessResponse.Readiness[i]);

                    // Hopefully removing this check won't cause issues as the last item in the activity array will have no sleep
                    // or readiness result, and with this check it would be ignored.

                    //if (sleep.SummaryDate == activity.SummaryDate && sleep.SummaryDate == readiness.SummaryDate)
                    //{
                    OuraCombinedObject oObj = new OuraCombinedObject();
                    oObj.UpdateFrom(sleep, readiness, activity);
                    ouraObjects.Add(oObj);
                    //}
                }

                // Get the list of fields the user requested in order they were shown.
                IEnumerable<OuraFields> fieldlist = OuraFields.CurrentFields().Where(c => c.FieldSelected).OrderBy(i => i.FieldOrder);

                // If they want to show the headers, iterate through the fields and if there is a custom label
                // display it, otherwise display the field name
                if (form.IncludeHeaders)
                {
                    foreach (OuraFields f in fieldlist)
                    {
                        // Only pass a description if the user wants to see the descriptions
                        if (String.IsNullOrEmpty(f.CustomLabel))
                            WriteCellToExcelHeader(ref activeCell, f.FieldName, (form.IncludeDescriptions ? f.FieldDescription : null));
                        else
                            WriteCellToExcelHeader(ref activeCell, f.CustomLabel, (form.IncludeDescriptions ? f.FieldDescription : null));
                    }
                    currentRow++;

                    // Reset it back to the start of the row one row down
                    activeCell = allCells.Item[currentRow, startColumn];
                }

                // Iterate through the returned data and insert the results into the Excel row
                foreach (OuraCombinedObject obj in ouraObjects)
                {
                    foreach (OuraFields f in fieldlist)
                    {
                        if (f.Accessor == OuraFields.AccessorType.Method)
                            WriteCellToExcel(ref activeCell, dynamicExecuteMethod(obj, f.MethodName));
                        else
                            WriteCellToExcel(ref activeCell, dynamicExecuteProperty(obj, f.MethodName));
                    }

                    currentRow++;
                    activeCell = allCells.Item[currentRow, startColumn];                                                                                                                                                                                                               //        currentRow++;                                                                                                                                                                                                                                                                                      //        activeCell = allCells.Item[currentRow, startColumn];
                }
            }
            else
            {
                MessageBox.Show("A problem occurred while attempting to retrieve your Oura metrics, add-in closing.");
                return;
            }
        }


        private void btnGetOuraHeartRates_Click(object sender, RibbonControlEventArgs e)
        {
            DateTime startDate = OuraExcelSettings.Default.StartDate;
            DateTime endDate = DateTime.Now;
            //OuraExcelSettings.Default.EndDate;   // Only set if the last time wasn't using the current day

            // Initialize Excel fields
            Excel.Worksheet activeWorksheet = ((Excel.Worksheet)Globals.ThisAddIn.Application.ActiveSheet);
            Excel.Range activeCell = Globals.ThisAddIn.Application.ActiveCell;
            int startColumn = activeCell.Column;
            int startRow = activeCell.Row;
            int currentRow = startRow;
            Excel.Range allCells = Globals.ThisAddIn.Application.Cells;

            // Request data from Oura for the selected date range
            SleepSummaryResponse sleepResponse = OuraAPIWrapper.PerformSleepSummaryRequest(startDate, endDate);

            if (sleepResponse != null)
            {
                foreach (SleepResponse sr in sleepResponse.Sleep)
                {
                    WriteCellToExcel(ref activeCell, sr.SummaryDate);
                    foreach (int i in sr.HR5Min)
                        WriteCellToExcel(ref activeCell, i);

                    currentRow++;
                    activeCell = allCells.Item[currentRow, startColumn];                                                                                                                                                                                                               //        currentRow++;                                                                                                                                                                                                                                                                                      
                }
            }
        }
        private void btnConfigureOuraAPIKey_Click(object sender, RibbonControlEventArgs e)
        {
            ConfigureOuraToken form = new ConfigureOuraToken();
            DialogResult result = form.ShowDialog();
            if (result == DialogResult.OK)
            {
                // Verify that the Oura API Token has been configured, if not request them to get one and configure it
                if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["OuraAPIKey"]))
                {
                    btnGetOuraData.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Write a specific value to the current cell in Excel, then move to the
        /// next cell along
        /// </summary>
        /// <param name="currentCell">reference to current cell</param>
        /// <param name="value">Value to insert</param>
        private void WriteCellToExcel(ref Excel.Range currentCell, object value)
        {
            currentCell.Value = value;
            currentCell = currentCell.Next;
        }

        private void WriteCellToExcelHeader(ref Excel.Range currentCell, object value, string description)
        {
            currentCell.Value2 = value;
            currentCell.Font.Bold = true;
            if (!String.IsNullOrEmpty(description))
            {
                //currentCell.AddComment(description);  // Comment appears bolded, note does not
                currentCell.NoteText(description);
            }
            currentCell = currentCell.Next;

        }

        /// <summary>
        /// Write the array of arguments into Excel, starting from the currently selected cell.
        /// </summary>
        /// <param name="currentCell">reference to current cell</param>
        /// <param name="arg"></param>
        private void WriteLineToExcel(Excel.Range currentCell, params object[] arg)
        {
            foreach (object anItem in arg)
            {
                currentCell.Value = anItem;
                currentCell = currentCell.Next;
            }
        }

        /// <summary>
        /// Method to take a property name as a string and execute it against the OuraCombinedObject
        /// we have a handle to.
        /// </summary>
        /// <param name="obj">Instance of OuraCombinedObject</param>
        /// <param name="functionName">Name of Property to execute</param>
        /// <returns>String holding the results of the executed property</returns>
        private string dynamicExecuteProperty(OuraCombinedObject obj, string functionName)
        {
            Type type = typeof(OuraCombinedObject);
            PropertyInfo property = type.GetProperty(functionName, BindingFlags.Public | BindingFlags.Instance);

            // This SHOULD only happen if there is something wrong with the OuraFields singleton
            if (property == null)
                return "FAILURE";

            object result = property.GetValue(obj);
            if (result == null)
                return string.Empty;
            return result.ToString();
        }

        /// <summary>
        /// Method to take a method name as a string and execute it against the OuraCombinedObject
        /// we have a handle to.
        /// </summary>
        /// <param name="obj">Instance of OuraCombinedObject</param>
        /// <param name="functionName">Name of Method to execute</param>
        /// <returns>String holding the results of the executed method</returns>
        private string dynamicExecuteMethod(OuraCombinedObject obj, string functionName)
        {
            Type type = typeof(OuraCombinedObject);
            MethodInfo method = type.GetMethod(functionName, BindingFlags.Public | BindingFlags.Instance);

            // This SHOULD only happen if there is something wrong with the OuraFields singleton
            if (method == null)
                return "FAILURE";

            object result = method.Invoke(obj, null);
            if (result == null)
                return string.Empty;
            return result.ToString();
        }
    }
}
