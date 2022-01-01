using OuraAPIInterface;
using System;
using System.Configuration;

using System.Windows.Forms;

namespace ExcelOuraVSTOAddIn
{
    public partial class ConfigureOuraToken : Form
    {
        public ConfigureOuraToken()
        {
            InitializeComponent();
        }

        private string _token;

        private void btnTestAPIKey_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtOuraAPIKey.Text))
            {
                MessageBox.Show("Please enter your Oura Personal Access Token to test this function. You will receive an authorization error if the token is not valid", "Excel Oura Add-In");
            }
            else
            {
                // Reset the token in the Wrapper and perform the authentication function.
                OuraAPIWrapper.APIToken(txtOuraAPIKey.Text);


                UserInfoResponse user = OuraAPIWrapper.PerformAuthentication();
                if (user != null)
                {
                    // The token was valid, display the results
                    txtGender.Text = user.Gender;
                    txtWeight.Text = user.Weight;
                    txtHeight.Text = user.Height;
                    txtEmail.Text = user.Email;
                    txtAge.Text = user.Age;
                }
                else
                {
                    MessageBox.Show("The token provided is invalid, request unauthorized", "Excel Oura Add-In");
                }
            }
        }

        private void ConfigureOuraAPIKey_Load(object sender, EventArgs e)
        {
            _token = ConfigurationManager.AppSettings["OuraApiKey"];
            txtOuraAPIKey.Text = _token;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            // To use this add-in, you need to expose a Personal Access Token from the Oura Cloud Dashboard
            // from the following location - https://cloud.ouraring.com/personal-access-tokens
            // Once you've created the token, update the key in "app.Config" / "ExcelOuraVSTOAddIn.dll.config" to reflect this value

            if (!String.IsNullOrEmpty(txtOuraAPIKey.Text))
            {
                // Just in case they tested some other values while the form was open.
                OuraAPIWrapper.APIToken(txtOuraAPIKey.Text);

                if ( txtOuraAPIKey.Text == _token)
                { 
                    // it's the same value we initially opened the form with, just close the dialog without saving or testing
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }

                // Just in case they didn't test the key, let's make sure it's valid before continuing
                UserInfoResponse user = OuraAPIWrapper.PerformAuthentication();
                if (user != null)
                {
                    // They provided a valid personal access token, let's save it to the config file and quit
                    var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings["OuraApiKey"].Value = txtOuraAPIKey.Text;
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("The Oura Personal Access Token provided is invalid, please check the token through the link on the dialog above", "Excel Oura Add-In");
                }
            }
            else
            {
                MessageBox.Show("No Personal Access Token provided to save", "Excel Oura Add-In");
            }
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            OuraAPIWrapper.APIToken(String.Empty);
            txtOuraAPIKey.Text = OuraAPIWrapper.APIToken();
            MessageBox.Show("Token reloaded from last save to the configuration file", "Excel Oura Add-In");
        }

        private void lblLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            lblLink.LinkVisited = true;
            System.Diagnostics.Process.Start(lblLink.Text);
        }
    }
}
