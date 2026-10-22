using OuraAPIInterface;
using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExcelOuraVSTOAddIn
{
    internal static class OuraProgressDialog
    {
        internal static T Run<T>(string title, Func<CancellationToken, IProgress<int>, Task<T>> operation)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            T result = default(T);
            Exception failure = null;
            bool finished = false;
            using (var cancellation = new CancellationTokenSource())
            using (var dialog = new Form
            {
                Text = title,
                Width = 460,
                Height = 170,
                StartPosition = FormStartPosition.CenterScreen,
                ControlBox = false,
                FormBorderStyle = FormBorderStyle.FixedDialog
            })
            {
                var message = new Label { Dock = DockStyle.Top, Height = 45, Text = "Retrieving data before updating Excel…" };
                var stop = new Button { Text = "Cancel", Dock = DockStyle.Bottom };
                Action cancel = () =>
                {
                    stop.Enabled = false;
                    message.Text = "Cancelling…";
                    cancellation.Cancel();
                };
                stop.Click += (sender, args) => cancel();
                dialog.FormClosing += (sender, args) =>
                {
                    if (finished) return;
                    args.Cancel = true;
                    cancel();
                };
                dialog.Controls.Add(message);
                dialog.Controls.Add(new ProgressBar { Dock = DockStyle.Top, Style = ProgressBarStyle.Marquee });
                dialog.Controls.Add(stop);
                dialog.Shown += async (sender, args) =>
                {
                    var updates = new Progress<int>(pages =>
                    {
                        if (!dialog.IsDisposed && !cancellation.IsCancellationRequested)
                            message.Text = "Downloaded " + pages + " pages. Preparing the export…";
                    });
                    try
                    {
                        // Only managed data reaches this worker. Excel COM stays on the UI thread.
                        result = await Task.Run(() => operation(cancellation.Token, updates));
                        cancellation.Token.ThrowIfCancellationRequested();
                    }
                    catch (Exception ex) { failure = ex; }
                    finally
                    {
                        finished = true;
                        dialog.Close();
                    }
                };
                dialog.ShowDialog();
            }
            if (failure != null) ExceptionDispatchInfo.Capture(failure).Throw();
            return result;
        }
    }
}
