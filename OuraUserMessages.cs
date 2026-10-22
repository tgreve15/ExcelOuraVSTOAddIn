using OuraAPIInterface;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ExcelOuraVSTOAddIn
{
    internal static class OuraUserMessages
    {
        internal static string For(Exception error)
        {
            if (error is OperationCanceledException) return "Export cancelled. No worksheet values were written.";
            var expected = error as OuraException;
            if (expected != null) return expected.Message;
            if (error is ConfigurationErrorsException)
                return "The saved preferences could not be read or written. Check your Windows profile settings.";
            if (error is COMException)
                return "Excel could not access the destination. Make sure the workbook is open and editable, then try again.";

            // Do not log exception messages, URLs, response bodies or values: they can contain credentials or health data.
            Trace.TraceError("Oura operation failed. Type={0}; HResult={1:X8}", error.GetType().FullName, error.HResult);
            return "An unexpected error stopped the operation. Restart Excel and try again; if it persists, report the action and add-in version.";
        }
    }
}
