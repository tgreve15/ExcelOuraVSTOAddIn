using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Authentication;

namespace OuraAPIInterface
{
    public static class OuraHttpClient
    {
        // VSTO runs inside Excel's process/AppDomain. Its TLS defaults can differ from
        // a standalone application even though the add-in targets Framework 4.7.2.
        // Set this per handler; never alter Excel's process-wide ServicePointManager
        // or bypass certificate validation.
        internal static HttpClientHandler CreateHandler() => new HttpClientHandler
        {
            AllowAutoRedirect = false,
            SslProtocols = SslProtocols.Tls12
        };
        public static HttpClient Create() => new HttpClient(CreateHandler()) { Timeout = TimeSpan.FromSeconds(45) };

        public static string DescribeSendFailure(HttpRequestException error, string operation)
        {
            string detail = "transport error";
            bool secure = false;
            for (Exception inner = error; inner != null; inner = inner.InnerException)
            {
                if (inner is AuthenticationException) { secure = true; detail = "TLS handshake/certificate validation"; }
                var web = inner as WebException;
                if (web != null)
                {
                    detail = web.Status.ToString();
                    secure |= web.Status == WebExceptionStatus.SecureChannelFailure || web.Status == WebExceptionStatus.TrustFailure;
                }
                var socket = inner as SocketException;
                if (socket != null) detail = socket.SocketErrorCode.ToString();
            }
            return "Could not send the " + operation + " to Oura (" + detail + "). " +
                (secure ? "The secure connection failed. " : "No usable response was received. ") +
                "This build uses TLS 1.2. Check Windows updates, your system date, and firewall/proxy or certificate settings, then start sign-in again.";
        }
    }
}
