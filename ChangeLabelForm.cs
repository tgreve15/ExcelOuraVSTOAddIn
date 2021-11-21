using System;
using System.Windows.Forms;

namespace ExcelOuraVSTOAddIn
{
    public partial class ChangeLabelForm : Form
    {
        public ChangeLabelForm()
        {
            InitializeComponent();
        }

        public string FieldName { get; set; }
        public string CustomLabel { get; set; }
        public string Section { get; set; }

        // If the user chooses to accept whatever is now in the Custom Label field, return it
        private void btnOk_Click(object sender, EventArgs e)
        {
            CustomLabel = txtCustomLabel.Text.Trim();
        }

        // Load the current values for field name and custom label
        private void ChangeLabelForm_Load(object sender, EventArgs e)
        {
            txtFieldName.Text = FieldName;
            txtSection.Text = Section;
            txtCustomLabel.Text = CustomLabel;
        }

        // Shortcut to use the text of the field name as a starter for the custom label
        private void txtFieldName_MouseClick(object sender, MouseEventArgs e)
        {
            // Double click the field name to copy that value into the custom label
            txtCustomLabel.Text = txtFieldName.Text;
        }
    }
}
