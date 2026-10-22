using OuraAPIInterface;
using System;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace ExcelOuraVSTOAddIn
{
    internal static class OuraConnection
    {
        private static readonly HttpClient Http = OuraHttpClient.Create();
        internal static readonly OuraOAuthClient OAuth = new OuraOAuthClient(Http);
        private static readonly OuraTokenManager Tokens = new OuraTokenManager(SecureTokenStorage.Load, SecureTokenStorage.Save,
            (settings, refresh, cancellation) => OAuth.RefreshAsync(settings, refresh, cancellation).GetAwaiter().GetResult());

        internal static T WithLock<T>(Func<T> action, CancellationToken cancellation = default(CancellationToken))
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            cancellation.ThrowIfCancellationRequested();
            using (var identity = WindowsIdentity.GetCurrent())
            using (var mutex = new Mutex(false, "Local\\ExcelOuraVSTOAddIn.OAuth." + identity.User.Value))
            {
                bool owned = false;
                try
                {
                    int outcome;
                    try { outcome = WaitHandle.WaitAny(new WaitHandle[] { mutex, cancellation.WaitHandle }, TimeSpan.FromSeconds(60)); }
                    catch (AbandonedMutexException) { outcome = 0; }
                    owned = outcome == 0;
                    cancellation.ThrowIfCancellationRequested();
                    if (!owned) throw new OuraException("Another Excel window is updating Oura sign-in. Try again shortly.");
                    return action();
                }
                finally
                {
                    // A mutex must be released on the same thread that acquired it; no awaits inside this scope.
                    if (owned) mutex.ReleaseMutex();
                }
            }
        }

        internal static bool IsConnected() => WithLock(() =>
        {
            var account = SecureTokenStorage.Load();
            if (account.Tokens == null || account.RefreshPending) return false;
            account.Settings.Validate();
            account.Tokens.Validate();
            return true;
        });

        internal static Task<bool> IsConnectedAsync() => Task.Run((Func<bool>)IsConnected);

        internal static void ConfigureApi()
        {
            OuraAPI2Wrapper.AccessTokenProviderAsync = (force, rejected, cancellation) => Task.Run(
                () => WithLock(() => Tokens.GetAccessToken(force, rejected, cancellation), cancellation), cancellation);
        }

        internal static void Disconnect() => WithLock(() =>
        {
            SecureTokenStorage.Clear();
            return true;
        });
    }
}
