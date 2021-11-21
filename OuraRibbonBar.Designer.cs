
using System;
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
//using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Excel;
using System.Windows.Forms;
using OuraAPIInterface;

namespace ExcelOuraVSTOAddIn
{
    partial class OuraRibbonBar : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public OuraRibbonBar()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabOura = this.Factory.CreateRibbonTab();
            this.group1 = this.Factory.CreateRibbonGroup();
            this.btnGetOuraData = this.Factory.CreateRibbonButton();
            this.tabOura.SuspendLayout();
            this.group1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabOura
            // 
            this.tabOura.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.tabOura.Groups.Add(this.group1);
            this.tabOura.Label = "Oura Commands";
            this.tabOura.Name = "tabOura";
            // 
            // group1
            // 
            this.group1.Items.Add(this.btnGetOuraData);
            this.group1.Label = "Oura Commands";
            this.group1.Name = "group1";
            // 
            // btnGetOuraData
            // 
            this.btnGetOuraData.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnGetOuraData.Description = "Retrieve your metrics from the Oura cloud environment";
            this.btnGetOuraData.Image = global::ExcelOuraVSTOAddIn.Properties.Resources.OuraIcon;
            this.btnGetOuraData.Label = "Get Oura Data";
            this.btnGetOuraData.Name = "btnGetOuraData";
            this.btnGetOuraData.ScreenTip = "Retrieve your metrics from the Oura cloud environment";
            this.btnGetOuraData.ShowImage = true;
            this.btnGetOuraData.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnGetOuraData_Click);
            // 
            // OuraRibbonBar
            // 
            this.Name = "OuraRibbonBar";
            this.RibbonType = "Microsoft.Excel.Workbook";
            this.Tabs.Add(this.tabOura);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.OuraRibbonBar_Load);
            this.tabOura.ResumeLayout(false);
            this.tabOura.PerformLayout();
            this.group1.ResumeLayout(false);
            this.group1.PerformLayout();
            this.ResumeLayout(false);

        }

        //private void Button1_Click(object sender, Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs e)
        //{

        //    OuraConfigurationForm form = new OuraConfigurationForm();
        //    DialogResult result = form.ShowDialog();
        //    if (result != DialogResult.OK)
        //    {
        //        return;
        //    }

        //    //UserInfoResponse response = OuraAPIWrapper.PerformAuthentication();
        //    //if (response != null)
        //    //{
        //    Excel.Worksheet activeWorksheet = ((Excel.Worksheet)Globals.ThisAddIn.Application.ActiveSheet);
        //    //Excel.Range firstRow = activeWorksheet.get_Range("A1");
        //    //firstRow.EntireRow.Insert(Excel.XlInsertShiftDirection.xlShiftDown);
        //    //Excel.Range newFirstRow = activeWorksheet.get_Range("A1");
        //    //newFirstRow.Value2 = "This text was added by using code";
        //    Excel.Range activeCell = Globals.ThisAddIn.Application.ActiveCell;
        //    int startColumn = activeCell.Column;
        //    int startRow = activeCell.Row;
        //    int currentRow = startRow;

        //    Excel.Range allCells = Globals.ThisAddIn.Application.Cells;

        //    //// String sleepString = OuraAPIWrapper.PerformSleepSummaryRequestString(Convert.ToDateTime("2021-08-30"), DateTime.Today);
        //    String activityString = OuraAPIWrapper.PerformActivitySummaryRequestString(form.StartDate(), form.EndDate());
        //    String activityResponse2 = OuraAPIWrapper.PerformActivitySummaryRequest2(form.StartDate(), form.EndDate());

        //    ActivitySummaryResponse activityResponse = OuraAPIWrapper.PerformActivitySummaryRequest(form.StartDate(), form.EndDate());

        //    SleepSummaryResponse sleepResponse = OuraAPIWrapper.PerformSleepSummaryRequest(form.StartDate(),form.EndDate());

        //    if (form.IncludeHeader())
        //    {
        //        WriteLineToExcel(activeCell, "Summary Date", "Deep Sleep", "REM Sleep", "Light Sleep", "Time Awake", "Sleep Total", "", "Bedtime Start", "Bedtime End", "Bedtime Start", "Bedtime End", "Steps");
        //        currentRow++;
        //        activeCell = allCells.Item[currentRow, startColumn];
        //    }
        //    if (sleepResponse != null)
        //    {
        //        foreach (SleepResponse resp in sleepResponse.Sleep)
        //        {
        //            WriteLineToExcel(activeCell, resp.SummaryDate, resp.Deep, resp.REM, resp.Light, resp.Awake, resp.Total, "", resp.BedtimeStart, resp.BedtimeEnd, resp.BedtimeStartFormatLocal(), resp.BedtimeEndFormatLocal()); //, resp.BedtimeEndDT(), resp.BedtimeStartDT());
        //            currentRow++;
        //            activeCell = allCells.Item[currentRow, startColumn];
        //        }
        //    }

        //    ReadinessSummaryResponse readinessResponse = OuraAPIWrapper.PerformReadinessSummaryRequest(form.StartDate(), form.EndDate());
        //    List<OuraCombinedObject> ouraObjects = new List<OuraCombinedObject>();

        //    if (activityResponse != null && readinessResponse != null && sleepResponse != null)
        //    {

        //        for (int i = 0; i < sleepResponse.Sleep.Length; i++)
        //        {
        //            SleepResponse sleep = sleepResponse.Sleep[i];
        //            ActivityResponse activity = activityResponse.Activity[i];
        //            ReadinessResponse readiness = readinessResponse.Readiness[i];
        //            //if (sleep.SummaryDate == activity.SummaryDate && sleep.SummaryDate == readiness.SummaryDate)
        //            //{
        //                OuraCombinedObject oObj = new OuraCombinedObject();
        //                oObj.UpdateFrom(sleep, readiness, activity);
        //                ouraObjects.Add(oObj);
        //            //} else
        //            //{
        //            //    Console.WriteLine("Failed");
        //            //}
        //        }

        //        foreach (OuraCombinedObject obj in ouraObjects)
        //        {
        //            WriteLineToExcel(activeCell, obj.SummaryDate, obj.Deep, obj.REM, obj.Light, obj.Awake, obj.SleepTotal, obj.BedtimeStart, obj.BedtimeEnd, obj.BedtimeStartFormatLocal(), obj.BedtimeEndFormatLocal(), obj.Steps); //, resp.BedtimeEndDT(), resp.BedtimeStartDT());
        //            currentRow++;
        //            activeCell = allCells.Item[currentRow, startColumn];                                                                                                                                                                                                               //        currentRow++;                                                                                                                                                                                                                                                                                      //        activeCell = allCells.Item[currentRow, startColumn];
        //        }
        //    }
            
        //    //String activityString = OuraAPIWrapper.PerformActivitySummaryRequestString(Convert.ToDateTime("2021-09-02"), DateTime.Today);
        //    //Console.WriteLine("---=== Activity ===---");
        //    //foreach (ActivityResponse act in activityResponse.Activity)
        //    //{
        //    //    Console.WriteLine("{0}", act.steps);
        //    //}

        //    //String readinessString = OuraAPIWrapper.PerformReadinessSummaryRequestString(Convert.ToDateTime("2021-09-02"), DateTime.Today);
        //    //foreach (ReadinessResponse read in readinessResponse.Readiness)
        //    //{
        //    //    Console.WriteLine("Readiness: {0},{1}", read.SummaryDate, read.scoreTemperature);
        //    //}
        //    //}
        //    //Console.WriteLine("Finished");
        //    //throw new System.NotImplementedException();
        //}

        //private void WriteLineToExcel(Excel.Range currentCell, params object[] arg) 
        //{
        //    foreach(object anItem in arg)
        //    {
        //        currentCell.Value = anItem;
        //        currentCell = currentCell.Next;
        //    }
        //}

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tabOura;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group1;
        //internal Microsoft.Office.Tools.Ribbon.RibbonGroup group2;
        //internal Microsoft.Office.Tools.Ribbon.RibbonTab tab2;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnGetOuraData;
    }

    partial class ThisRibbonCollection
    {
        internal OuraRibbonBar OuraRibbonBar
        {
            get { return this.GetRibbon<OuraRibbonBar>(); }
        }
    }
}
