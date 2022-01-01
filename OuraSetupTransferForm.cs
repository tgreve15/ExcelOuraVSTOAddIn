using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExcelOuraVSTOAddIn
{
    public partial class OuraSetupTransferForm : Form
    {
        public OuraSetupTransferForm()
        {
            InitializeComponent();
        }

        public bool IncludeHeaders { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public bool IncludeDescriptions { get; set; }

        private void OuraConfigurationForm_Load(object sender, EventArgs e)
        {
            dtmStartDate.MaxDate = DateTime.Today;
            dtmEndDate.MaxDate = DateTime.Today;
            dtmEndDate.Value = ((EndDate == default(DateTime)) ? DateTime.Today : EndDate);
            dtmStartDate.Value = ((StartDate == default(DateTime)) ? DateTime.Today.AddDays(-14) : StartDate);
            chkIncludeHeaders.Checked = IncludeHeaders;
            chkDescription.Checked = IncludeDescriptions;
            chkDescription.Enabled = IncludeHeaders;    // If IncludeHeaders is checked, enable Description

            InitializeListView();
        }

        // Do Everything to load the list view with available fields
        private void InitializeListView()
        {
            // ListView Grouping
            // Although this looked good when enabled, the grouping caused the drag-drop 
            // reordering to not work correctly, and didn't make sense when you may want 
            // Activity or Readiness data in front of Sleep data. Shame, but there it is.

            //lvListView.ShowGroups = true;
            //ListViewGroup sg = new ListViewGroup("sleep", "Sleep");
            //ListViewGroup ag = new ListViewGroup("activity", "Activity");
            //ListViewGroup rg = new ListViewGroup("readiness", "Readiness");

            //lvListView.Groups.Add(sg);
            //lvListView.Groups.Add(ag);
            //lvListView.Groups.Add(rg);

            foreach (OuraFields aField in OuraFields.CurrentFields().OrderBy(i => i.FieldOrder))
            {
                // For more complex fields, don't show them - configured with Accessible = false
                if (!aField.Accessible)
                    continue;

                ListViewItem i = new ListViewItem(aField.FieldName);

                // Set the group based on the Oura Section
                //switch (aField.OuraSection)
                //{
                //    case "Sleep":
                //        i.Group = sg;
                //        break;
                //    case "Activity":
                //        i.Group = ag;
                //        break;
                //    case "Readiness":
                //        i.Group = rg;
                //        break;
                //    default:
                //        break;
                //}

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
                    OuraFields f = OuraFields.CurrentFields().FirstOrDefault(item => item.FieldName == lvi.Text);
                    if (f != null)
                    {
                        f.CustomLabel = lvi.SubItems[2].Text;
                        f.FieldSelected = lvi.Checked;
                        f.FieldOrder = fieldOrder++;
                    }
                }
                // All is good, we can close the dialog box
                // NOTE: this button is NOT set to be default for the form as if it is, it is much harder to
                // prevent this finalizing and closing the dialog if no fields were selected.
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
            ChangeLabelForm changeLabelForm = new ChangeLabelForm();
            ListView localSender = sender as ListView;
            if (localSender != null)
            {
                ListViewItem selectedItem = localSender.FocusedItem;
                if (selectedItem != null)
                {
                    changeLabelForm.FieldName = selectedItem.Text;
                    changeLabelForm.Section = selectedItem.SubItems[1].Text;
                    changeLabelForm.CustomLabel = selectedItem.SubItems[2].Text;
                    DialogResult result = changeLabelForm.ShowDialog();
                    if (result != DialogResult.OK)
                    {
                        return;
                    }
                    else
                    {
                        // If they set the CustomLabel to the FieldName, just clear the custom label and use the default
                        if (selectedItem.Text == changeLabelForm.CustomLabel)
                        {
                            selectedItem.SubItems[2].Text = "";
                        }
                        else
                        {
                            // Update the field in the list view with the new custom label
                            selectedItem.SubItems[2].Text = changeLabelForm.CustomLabel;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Mark all visible fields in the ListView to the value of the check field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach(ListViewItem item in lvListView.Items)
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
            OuraFields.ResetFields();
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
            } else
            {
                chkDescription.Enabled = false;
                chkDescription.Checked = false;
            }
        }
    }
}
