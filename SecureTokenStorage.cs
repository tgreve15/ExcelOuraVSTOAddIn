using Newtonsoft.Json;
using OuraAPIInterface;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ExcelOuraVSTOAddIn
{
    /// <summary>DPAPI-protected, atomically replaced per-user credentials. Call while holding the account mutex.</summary>
    public static class SecureTokenStorage
    {
        private const int MaximumFileBytes = 128 * 1024;
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("ExcelOuraVSTOAddIn.OAuth.v2");
        private static readonly string Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ExcelOuraVSTOAddIn");
        private static readonly string FileName = Path.Combine(Folder, "oauth.dat");

        public static OuraAccount Load()
        {
            byte[] plain = null;
            try
            {
                if (!File.Exists(FileName)) return new OuraAccount();
                byte[] encrypted;
                using (var source = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (source.Length > MaximumFileBytes) throw new OuraException("The saved Oura connection is invalid. Disconnect and reconnect Oura.");
                    encrypted = new byte[(int)source.Length];
                    int offset = 0;
                    while (offset < encrypted.Length)
                    {
                        int read = source.Read(encrypted, offset, encrypted.Length - offset);
                        if (read == 0) throw new IOException("Incomplete credential file.");
                        offset += read;
                    }
                }
                plain = ProtectedData.Unprotect(encrypted, Entropy, DataProtectionScope.CurrentUser);
                var account = JsonConvert.DeserializeObject<OuraAccount>(new UTF8Encoding(false, true).GetString(plain), new JsonSerializerSettings { MaxDepth = 16 });
                if (account?.Settings == null) throw new OuraException("The saved Oura connection is invalid. Disconnect and reconnect Oura.");
                return account;
            }
            catch (Exception ex) when (ex is CryptographicException || ex is JsonException || ex is DecoderFallbackException)
            {
                throw new OuraException("The saved Oura connection cannot be read in this Windows profile. Disconnect and reconnect Oura.");
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                throw new OuraException("The saved Oura connection could not be read. Check access to your Windows profile storage.");
            }
            finally
            {
                if (plain != null) Array.Clear(plain, 0, plain.Length);
            }
        }

        public static void Save(OuraAccount account)
        {
            if (account?.Settings == null) throw new ArgumentException("An Oura account is required.", nameof(account));
            account.Settings.Validate();
            account.Tokens?.Validate();
            byte[] plain = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(account));
            string temporary = Path.Combine(Folder, Guid.NewGuid().ToString("N") + ".tmp");
            try
            {
                var encrypted = ProtectedData.Protect(plain, Entropy, DataProtectionScope.CurrentUser);
                if (encrypted.Length > MaximumFileBytes) throw new OuraException("The Oura credentials are too large to save safely. Reconnect Oura.");
                Directory.CreateDirectory(Folder);
                using (var destination = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
                {
                    destination.Write(encrypted, 0, encrypted.Length);
                    destination.Flush(true);
                }
                if (File.Exists(FileName)) File.Replace(temporary, FileName, null);
                else File.Move(temporary, FileName);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is CryptographicException)
            {
                throw new OuraException("The Oura connection could not be saved. Check your Windows profile storage and reconnect Oura.");
            }
            finally
            {
                Array.Clear(plain, 0, plain.Length);
                try { if (File.Exists(temporary)) File.Delete(temporary); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }

        public static void Clear()
        {
            try
            {
                if (File.Exists(FileName)) File.Delete(FileName);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                throw new OuraException("The Oura connection could not be deleted. Check access to your Windows profile storage.");
            }
            try
            {
                OuraExcelSettings.Default.OuraApiKeyProtected = String.Empty;
                OuraExcelSettings.Default.Save();
            }
            catch (Exception ex) when (ex is System.Configuration.ConfigurationErrorsException || ex is IOException || ex is UnauthorizedAccessException)
            {
                throw new OuraException("The OAuth connection was removed, but the old unused token preference could not be cleared. Check your Windows profile settings.");
            }
        }
    }
}
