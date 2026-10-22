using OuraAPIInterface;
using System;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ExcelOuraVSTOAddIn
{
    public partial class ConfigureOuraToken : Form
    {
        private readonly TextBox clientId = new TextBox();
        private readonly TextBox clientSecret = new TextBox { UseSystemPasswordChar = true };
        private readonly TextBox redirect = new TextBox();
        private readonly Label status = new Label();
        private readonly Button connect = new Button { Text = "Sign in with Oura", AutoSize = true };
        private readonly Button disconnect = new Button { Text = "Disconnect", AutoSize = true };
        private CancellationTokenSource signIn;
        public event EventHandler ConnectionChanged;

        public ConfigureOuraToken()
        {
            InitializeComponent();
            Text = "Connect Oura — 1.0.0.18";
            ClientSize = new System.Drawing.Size(620, 390);
            MinimumSize = Size;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(15), ColumnCount = 2, RowCount = 7 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            var instructions = new Label { AutoSize = true, MaximumSize = new System.Drawing.Size(570, 0), Text = "Register your own Oura API application, then enter its credentials below. Register the redirect address exactly as shown. Your browser opens for Oura sign-in; credentials are encrypted for your Windows account." };
            layout.Controls.Add(instructions, 0, 0); layout.SetColumnSpan(instructions, 2);
            AddRow(layout, "Client ID", clientId, 1);
            AddRow(layout, "Client secret", clientSecret, 2);
            AddRow(layout, "Redirect address", redirect, 3);
            var portal = new LinkLabel { Text = "Open Oura application registrations", AutoSize = true };
            portal.LinkClicked += (s, e) => Process.Start(new ProcessStartInfo("https://developer.ouraring.com/applications") { UseShellExecute = true });
            layout.Controls.Add(portal, 1, 4);
            status.AutoSize = true; status.MaximumSize = new System.Drawing.Size(460, 0);
            layout.Controls.Add(status, 1, 5);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            var close = new Button { Text = "Close", AutoSize = true };
            close.Click += (s, e) => Close();
            connect.Click += Connect_Click;
            disconnect.Click += (s, e) =>
            {
                try { OuraConnection.Disconnect(); ConnectionChanged?.Invoke(this, EventArgs.Empty); status.Text = "Disconnected locally. To revoke access at Oura, remove this application in your Oura account."; }
                catch (Exception ex) { MessageBox.Show(this, OuraUserMessages.For(ex), "Oura disconnect"); }
            };
            buttons.Controls.AddRange(new Control[] { connect, disconnect, close });
            layout.Controls.Add(buttons, 1, 6);
            Controls.Add(layout);
            try
            {
                var account = OuraConnection.WithLock(() => SecureTokenStorage.Load());
                clientId.Text = account.Settings.ClientId;
                clientSecret.Text = account.Settings.ClientSecret;
                redirect.Text = account.Settings.RedirectUri;
                status.Text = account.Tokens == null || account.RefreshPending ? "Not connected." : "Connected. Sign in again to approve newly added exports if a permission is missing.";
            }
            catch (Exception ex) { redirect.Text = new OuraOAuthSettings().RedirectUri; status.Text = OuraUserMessages.For(ex); }
            FormClosing += (sender, args) =>
            {
                if (signIn == null) return;
                args.Cancel = true;
                signIn.Cancel();
                status.Text = "Cancelling sign-in. Wait for it to finish before closing.";
            };
        }
        private static void AddRow(TableLayoutPanel layout, string label, TextBox input, int row)
        {
            layout.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
            input.Dock = DockStyle.Fill; layout.Controls.Add(input, 1, row);
        }
        private async void Connect_Click(object sender, EventArgs e)
        {
            connect.Enabled = disconnect.Enabled = false;
            clientId.Enabled = clientSecret.Enabled = redirect.Enabled = false;
            signIn = new CancellationTokenSource(TimeSpan.FromMinutes(3));
            try
            {
                var settings = new OuraOAuthSettings { ClientId = clientId.Text.Trim(), ClientSecret = clientSecret.Text.Trim(), RedirectUri = redirect.Text.Trim() };
                var authorization = new OuraAuthorization(settings);
                using (var listener = new HttpListener())
                {
                    listener.Prefixes.Add(settings.RedirectUri);
                    try { listener.Start(); }
                    catch (HttpListenerException) { throw new OuraException("Could not open the local sign-in callback. Check the port is free and Windows allows this localhost listener. See the setup guide for URL permissions."); }
                    using (signIn.Token.Register(() => listener.Stop()))
                    {
                        Process.Start(new ProcessStartInfo(authorization.AuthorizationUrl) { UseShellExecute = true });
                        status.Text = "Waiting for Oura approval in your browser (up to three minutes)…";
                        string code = await ReceiveCodeAsync(listener, authorization, settings.RedirectUri, signIn.Token);
                        settings.TokenEndpoint = authorization.TokenEndpoint;
                        status.Text = "Oura approval received. Completing the secure sign-in request…";
                        var tokens = await OuraConnection.OAuth.ExchangeAsync(settings, code, authorization.CodeVerifier, signIn.Token);
                        // Saving immediately is essential: the refresh token is single-use.
                        OuraConnection.WithLock(() => { SecureTokenStorage.Save(new OuraAccount { Settings = settings, Tokens = tokens }); return true; });
                        ConnectionChanged?.Invoke(this, EventArgs.Empty);
                        status.Text = "Connected. Close this window to export your approved data.";
                    }
                }
            }
            catch (Exception ex)
            {
                if (!IsDisposed && !Disposing)
                    status.Text = signIn.IsCancellationRequested ? "Sign-in cancelled or timed out. Try again." : OuraUserMessages.For(ex);
            }
            finally
            {
                signIn.Dispose(); signIn = null;
                if (!IsDisposed && !Disposing)
                {
                    connect.Enabled = disconnect.Enabled = true;
                    clientId.Enabled = clientSecret.Enabled = redirect.Enabled = true;
                }
            }
        }
        private static async Task<string> ReceiveCodeAsync(HttpListener listener, OuraAuthorization authorization,
            string redirectUri, CancellationToken cancellation)
        {
            while (true)
            {
                cancellation.ThrowIfCancellationRequested();
                var callback = await listener.GetContextAsync();
                string code = null;
                bool denied = false;
                try
                {
                    if (callback.Request.HttpMethod == "GET") code = authorization.ReadCode(callback.Request.Url, redirectUri);
                }
                catch (OuraAuthorizationDeniedException) { denied = true; }
                catch (OuraException) { } // Ignore unrelated requests; keep waiting for the matching state.
                try
                {
                    callback.Response.StatusCode = code != null || denied ? 200 : 400;
                    callback.Response.ContentType = "text/plain; charset=utf-8";
                    callback.Response.Headers["Cache-Control"] = "no-store";
                    byte[] content = Encoding.UTF8.GetBytes(code != null
                        ? "Oura authorization response received. Return to Excel and wait for Connected before exporting. You can close this tab."
                        : "This sign-in response was not accepted. Return to Excel to check the connection.");
                    await callback.Response.OutputStream.WriteAsync(content, 0, content.Length, cancellation);
                }
                catch (IOException) { } // A closed browser tab must not discard a valid code.
                catch (HttpListenerException) { }
                finally { callback.Response.Close(); }
                if (denied) throw new OuraAuthorizationDeniedException();
                if (code != null) return code;
            }
        }
    }
}
