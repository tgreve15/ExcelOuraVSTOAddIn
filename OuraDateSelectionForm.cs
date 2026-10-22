using System;
using System.Windows.Forms;

namespace ExcelOuraVSTOAddIn
{
    public partial class OuraDateSelectionForm : Form
    {
        public OuraDateSelectionForm()
        {
            InitializeComponent();
            AcceptButton = btnOk;
            // The click handler sets OK only after validation succeeds.
            btnOk.DialogResult = DialogResult.None;
        }

        public bool IncludeHeaders { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public string InformationTypeLabel { get; set; }

        private void OuraDateSelectionForm_Load(object sender, EventArgs e)
        {
            dtmStartDate.MaxDate = DateTime.Today;
            dtmEndDate.MaxDate = DateTime.Today;
            dtmEndDate.Value = ClampDate(EndDate, DateTime.Today);
            dtmStartDate.Value = ClampDate(StartDate, DateTime.Today.AddDays(-14));
            chkIncludeHeaders.Checked = IncludeHeaders;
            lblInformationType.Text = InformationTypeLabel;
        }

        /// <summary>
        /// Process the selected configuration from the form. 
        /// Do not allow them to continue though if they haven't selected any fields to display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOk_Click(object sender, EventArgs e)
        {
            if (dtmStartDate.Value.Date > dtmEndDate.Value.Date)
            {
                MessageBox.Show(this, "Start date must be on or before end date.", "Oura dates");
                return;
            }
            // Store the configuration fields
            StartDate = dtmStartDate.Value;
            EndDate = dtmEndDate.Value;
            IncludeHeaders = chkIncludeHeaders.Checked;

            // All is good, we can close the dialog box
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private static DateTime ClampDate(DateTime value, DateTime fallback)
        {
            value = value == DateTime.MinValue ? fallback : value.Date;
            if (value < DateTimePicker.MinimumDateTime) return DateTimePicker.MinimumDateTime;
            return value > DateTime.Today ? DateTime.Today : value;
        }
    }
}
