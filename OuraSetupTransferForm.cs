using System;
using OuraAPIInterface;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ExcelOuraVSTOAddIn
{
    public partial class OuraSetupTransferForm : Form
    {
        public OuraSetupTransferForm()
        {
            InitializeComponent();
            AcceptButton = btnOk;
            // The click handler sets OK only after validation succeeds.
            btnOk.DialogResult = DialogResult.None;
        }

        public Func<List<OuraFields>> DefaultsProvider { get; set; }
        public bool UseDateRange { get; set; } = true;
        public bool IncludeHeaders { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public bool IncludeDescriptions { get; set; }
        public string FormHeader { get; set; }
        public List<OuraFields> FormFields { get; set; }

        private void OuraConfigurationForm_Load(object sender, EventArgs e)
        {
            dtmStartDate.Enabled = dtmEndDate.Enabled = UseDateRange;
            dtmStartDate.MaxDate = DateTime.Today;
            dtmEndDate.MaxDate = DateTime.Today;
            dtmEndDate.Value = ClampDate(EndDate, DateTime.Today);
            dtmStartDate.Value = ClampDate(StartDate, DateTime.Today.AddDays(-14));
            chkIncludeHeaders.Checked = IncludeHeaders;
            chkDescription.Checked = IncludeDescriptions;
            chkDescription.Enabled = IncludeHeaders;    // If IncludeHeaders is checked, enable Description
            lblHeader.Text = UseDateRange ? FormHeader : FormHeader + " (dates do not apply)";
            InitializeListView();
        }

        // Do Everything to load the list view with available fields
        private void InitializeListView()
        {
            foreach (OuraFields aField in FormFields.OrderBy(i => i.FieldOrder))
            {
                // For more complex fields, don't show them - configured with Accessible = false
                if (!aField.Accessible)
                    continue;

                ListViewItem i = new ListViewItem(aField.FieldName) { Tag = aField };

                i.SubItems.Add(aField.OuraSection);
                i.SubItems.Add(aField.CustomLabel);
                i.SubItems.Add(aField.FieldDescription);
                if (!String.IsNullOrEmpty(aField.FieldDescription))
                {
                    i.ToolTipText = string.Format("{0}: {1}", aField.FieldName, aField.FieldDescription);
                }
                else
                {
                    i.ToolTipText = aField.FieldName;
                }
                i.Checked = aField.FieldSelected;

                lvListView.Items.Add(i);
            }
        }

        /// <summary>
        /// Process the selected configuration from the form. 
        /// Do not allow them to continue though if they haven't selected any fields to display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (UseDateRange && dtmStartDate.Value.Date > dtmEndDate.Value.Date)
            {
                errorProviderApp.SetError(dtmStartDate, "Start date must be on or before end date.");
                return;
            }
            errorProviderApp.SetError(dtmStartDate, "");
            // Make sure there is at least one field selected to show in Excel
            if (lvListView.CheckedItems.Count == 0)
            {
                MessageBox.Show("Nothing has been selected to be imported into Excel. Please select one or more fields to continue.", "Excel Oura Add-In");
                errorProviderApp.SetError(lvListView, "Nothing has been selected to be imported into Excel. Please select one or more fields to continue.");
            }
            else
            {
                // Clear out the error field, just in case it is set
                errorProviderApp.SetError(lvListView, "");

                // Store the configuration fields
                StartDate = dtmStartDate.Value;
                EndDate = dtmEndDate.Value;
                IncludeHeaders = chkIncludeHeaders.Checked;
                IncludeDescriptions = chkDescription.Checked;

                // Work through all fields in the table and update the OuraFields singleton accordingly
                int fieldOrder = 1;
                foreach (ListViewItem lvi in lvListView.Items)
                {
                    OuraFields f = lvi.Tag as OuraFields;
                    if (f != null)
                    {
                        f.CustomLabel = lvi.SubItems[2].Text;
                        f.FieldSelected = lvi.Checked;
                        f.FieldOrder = fieldOrder++;
                    }
                }
                // All is good, we can close the dialog box
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        /// <summary>
        /// User double clicked an item in the ListView, give them the option to change the 
        /// text that will be displayed in the Excel header
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvListView_DoubleClick(object sender, EventArgs e)
        {
            var list = sender as ListView;
            var item = list?.FocusedItem;
            if (item == null) return;
            using (var form = new ChangeLabelForm
            {
                FieldName = item.Text,
                Section = item.SubItems[1].Text,
                CustomLabel = item.SubItems[2].Text
            })
            {
                if (form.ShowDialog(this) != DialogResult.OK) return;
                item.SubItems[2].Text = form.CustomLabel == item.Text ? "" : form.CustomLabel;
            }
        }

        /// <summary>
        /// Mark all visible fields in the ListView to the value of the check field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvListView.Items)
            {
                item.Checked = chkAll.Checked;
            }
        }

        /// <summary>
        /// Start from scratch, clear out the fields and fall back to system defaults
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReinitialize_Click(object sender, EventArgs e)
        {
            if (DefaultsProvider != null) FormFields = DefaultsProvider().Select(OuraFieldSelections.Clone).ToList();
            lvListView.Items.Clear();
            InitializeListView();
        }

        /// <summary>
        /// You can only show descriptions in the header if the header is enabled. Otherwise, 
        /// clear it out and disable it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkIncludeHeaders_CheckedChanged(object sender, EventArgs e)
        {
            if (chkIncludeHeaders.Checked)
            {
                chkDescription.Enabled = true;
            }
            else
            {
                chkDescription.Enabled = false;
                chkDescription.Checked = false;
            }
        }
        private static DateTime ClampDate(DateTime value, DateTime fallback)
        {
            value = value == DateTime.MinValue ? fallback : value.Date;
            if (value < DateTimePicker.MinimumDateTime) return DateTimePicker.MinimumDateTime;
            return value > DateTime.Today ? DateTime.Today : value;
        }
    }
}
