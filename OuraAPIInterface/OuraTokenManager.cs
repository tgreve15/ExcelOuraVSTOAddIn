using System;
using System.Threading;

namespace OuraAPIInterface
{
    public sealed class OuraAccount
    {
        public OuraOAuthSettings Settings { get; set; } = new OuraOAuthSettings();
        public OuraOAuthTokens Tokens { get; set; }
        public bool RefreshPending { get; set; }
    }

    /// <summary>Coordinates one token rotation while its caller owns the account's cross-process lock.</summary>
    public sealed class OuraTokenManager
    {
        private readonly Func<OuraAccount> load;
        private readonly Action<OuraAccount> save;
        private readonly Func<OuraOAuthSettings, string, CancellationToken, OuraOAuthTokens> refresh;
        private readonly Func<DateTimeOffset> utcNow;

        public OuraTokenManager(Func<OuraAccount> load, Action<OuraAccount> save,
            Func<OuraOAuthSettings, string, CancellationToken, OuraOAuthTokens> refresh,
            Func<DateTimeOffset> utcNow = null)
        {
            this.load = load ?? throw new ArgumentNullException(nameof(load));
            this.save = save ?? throw new ArgumentNullException(nameof(save));
            this.refresh = refresh ?? throw new ArgumentNullException(nameof(refresh));
            this.utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        }

        public string GetAccessToken(bool forceRefresh, string rejectedToken, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            var account = load();
            if (account?.Settings == null || account.Tokens == null)
                throw new OuraException("Use Connect Oura to sign in before exporting.");
            if (account.RefreshPending)
                throw new OuraException("A previous token refresh was interrupted. Reconnect Oura before exporting; the old refresh token will not be used again.");

            account.Settings.Validate();
            OuraArguments.ValidateAccessToken(account.Tokens.AccessToken);
            bool expired = account.Tokens.ExpiresAtUtc <= utcNow().AddMinutes(2);
            bool rejectedCurrentToken = forceRefresh && (rejectedToken == null || rejectedToken == account.Tokens.AccessToken);
            if (!expired && !rejectedCurrentToken) return account.Tokens.AccessToken;

            OuraArguments.ValidateSecret(account.Tokens.RefreshToken, "refresh token");
            cancellation.ThrowIfCancellationRequested();
            // Persist before sending: a crash/lost response can otherwise reuse a consumed token.
            account.RefreshPending = true;
            Save(account);
            OuraOAuthTokens updated;
            try
            {
                updated = refresh(account.Settings, account.Tokens.RefreshToken, cancellation);
                if (updated == null) throw new OuraException("Oura returned an incomplete token response. Reconnect Oura.");
                updated.Validate();
            }
            catch (OperationCanceledException) { throw; }
            catch (OuraException) { throw; }
            catch (Exception ex) when (ex is System.IO.IOException || ex is System.Net.Http.HttpRequestException)
            {
                throw new OuraException("The Oura token refresh did not complete. Reconnect Oura before exporting again.");
            }

            account.Tokens = updated;
            account.Settings.TokenEndpoint = OuraOAuthEndpoints.PublicToken;
            account.RefreshPending = false;
            Save(account);
            return updated.AccessToken;
        }

        private void Save(OuraAccount account)
        {
            try { save(account); }
            catch (Exception ex) when (ex is System.IO.IOException || ex is UnauthorizedAccessException || ex is OuraException)
            {
                throw new OuraException("Could not save the Oura connection. Check your Windows profile storage and reconnect Oura.");
            }
        }
    }
}
