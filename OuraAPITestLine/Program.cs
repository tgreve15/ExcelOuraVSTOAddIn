using OuraAPIInterface;
using System;

namespace OuraAPITestLine
{
    internal static class Program
    {
        // Diagnostic-only: supply a current OAuth access token in the environment.
        // The Excel add-in owns browser sign-in, encrypted storage and refresh.
        private static int Main(string[] args)
        {
            var token = Environment.GetEnvironmentVariable("OURA_ACCESS_TOKEN");
            if (String.IsNullOrWhiteSpace(token))
            {
                Console.Error.WriteLine("Set OURA_ACCESS_TOKEN to a current OAuth access token. For normal use, connect your account in Excel. No token is read from app.config.");
                return 2;
            }
            OuraAPI2Wrapper.APIToken(token);
            try
            {
                var rows = OuraAPI2Wrapper.GetAllOuraDataV2ForDateRange(DateTime.Today.AddDays(-7), DateTime.Today);
                Console.WriteLine("Retrieved {0} daily rows successfully. Health data and credentials are not printed.", rows.Count);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex is OuraException ? ex.Message : "The diagnostic failed. Check the access token, dates and connection; no response contents are printed.");
                return 1;
            }
        }
    }
}
