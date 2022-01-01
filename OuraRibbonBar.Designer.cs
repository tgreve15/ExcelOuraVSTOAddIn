
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
using Microsoft.Office.Tools.Ribbon;

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
            this.grpOura = this.Factory.CreateRibbonGroup();
            this.btnGetOuraData = this.Factory.CreateRibbonButton();
            this.btnGetOuraHeartRates = this.Factory.CreateRibbonButton();
            this.btnConfigureOuraAPIKey = this.Factory.CreateRibbonButton();
            this.tabOura.SuspendLayout();
            this.grpOura.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabOura
            // 
            this.tabOura.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.tabOura.Groups.Add(this.grpOura);
            this.tabOura.Label = "Oura Commands";
            this.tabOura.Name = "tabOura";
            // 
            // grpOura
            // 
            this.grpOura.Items.Add(this.btnGetOuraData);
            this.grpOura.Items.Add(this.btnGetOuraHeartRates);
            this.grpOura.Items.Add(this.btnConfigureOuraAPIKey);
            this.grpOura.Label = "Oura Commands";
            this.grpOura.Name = "grpOura";
            // 
            // btnGetOuraData
            // 
            this.btnGetOuraData.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnGetOuraData.Description = "Retrieve your metrics from the Oura cloud environment";
            this.btnGetOuraData.Image = global::ExcelOuraVSTOAddIn.Properties.Resources.OuraDarkSolid;
            this.btnGetOuraData.Label = "Get Oura Data";
            this.btnGetOuraData.Name = "btnGetOuraData";
            this.btnGetOuraData.ScreenTip = "Retrieve your metrics from the Oura cloud environment";
            this.btnGetOuraData.ShowImage = true;
            this.btnGetOuraData.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnGetOuraData_Click);
            // 
            // btnGetOuraHeartRates
            // 
            this.btnGetOuraHeartRates.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnGetOuraHeartRates.Description = "Retrieve your heart rate metrics from the Oura cloud environment";
            this.btnGetOuraHeartRates.Image = global::ExcelOuraVSTOAddIn.Properties.Resources.OuraDarkSolid;
            this.btnGetOuraHeartRates.Label = "Get Heart Rate";
            this.btnGetOuraHeartRates.Name = "btnGetOuraHeartRates";
            this.btnGetOuraHeartRates.ScreenTip = "Retrieve your heart rate from the Oura cloud environment";
            this.btnGetOuraHeartRates.ShowImage = true;
            this.btnGetOuraHeartRates.Visible = false;
            this.btnGetOuraHeartRates.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnGetOuraHeartRates_Click);
            // 
            // btnConfigureOuraAPIKey
            // 
            this.btnConfigureOuraAPIKey.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnConfigureOuraAPIKey.Description = "Configure your Oura Personal Access Token to access your data from the Oura Dashb" +
    "oard";
            this.btnConfigureOuraAPIKey.Image = global::ExcelOuraVSTOAddIn.Properties.Resources.Settings;
            this.btnConfigureOuraAPIKey.Label = "Configure Oura Token";
            this.btnConfigureOuraAPIKey.Name = "btnConfigureOuraAPIKey";
            this.btnConfigureOuraAPIKey.ScreenTip = "Configure your Oura Personal Access Token to access your data from the Oura Dashb" +
    "oard";
            this.btnConfigureOuraAPIKey.ShowImage = true;
            this.btnConfigureOuraAPIKey.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnConfigureOuraAPIKey_Click);
            // 
            // OuraRibbonBar
            // 
            this.Name = "OuraRibbonBar";
            this.RibbonType = "Microsoft.Excel.Workbook";
            this.Tabs.Add(this.tabOura);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.OuraRibbonBar_Load);
            this.tabOura.ResumeLayout(false);
            this.tabOura.PerformLayout();
            this.grpOura.ResumeLayout(false);
            this.grpOura.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tabOura;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup grpOura;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnGetOuraData;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnGetOuraHeartRates;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnConfigureOuraAPIKey;
    }

    partial class ThisRibbonCollection
    {
        internal OuraRibbonBar OuraRibbonBar
        {
            get { return this.GetRibbon<OuraRibbonBar>(); }
        }
    }
}
