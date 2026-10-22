using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OuraAPIInterface
{
    internal static class OuraHttpContent
    {
        internal const int MaximumDataBytes = 16 * 1024 * 1024;
        internal const int MaximumTokenBytes = 64 * 1024;

        internal static async Task<string> ReadAsync(HttpContent content, int maximumBytes, CancellationToken cancellation)
        {
            if (content == null) throw new OuraException("Oura returned an empty response.");
            if (content.Headers.ContentLength > maximumBytes)
                throw new OuraException("Oura returned a response that is too large. Use a smaller date range.");

            using (var source = await content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var destination = new MemoryStream())
            {
                var buffer = new byte[8192];
                int count;
                while ((count = await source.ReadAsync(buffer, 0, buffer.Length, cancellation).ConfigureAwait(false)) != 0)
                {
                    if (destination.Length + count > maximumBytes)
                        throw new OuraException("Oura returned a response that is too large. Use a smaller date range.");
                    destination.Write(buffer, 0, count);
                }
                cancellation.ThrowIfCancellationRequested();
                try { return new UTF8Encoding(false, true).GetString(destination.ToArray()); }
                catch (DecoderFallbackException) { throw new OuraException("Oura returned invalid text data. The export has stopped."); }
            }
        }
    }
}
