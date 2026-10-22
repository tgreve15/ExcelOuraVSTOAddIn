using ExcelOuraVSTOAddIn;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace OuraAPIInterface
{
    /// <summary>An expected failure whose message is safe to present to the user.</summary>
    public class OuraException : InvalidOperationException
    {
        public OuraException(string message) : base(message) { }
    }

    internal static class OuraJson
    {
        internal static JObject ParseObject(string json)
        {
            if (String.IsNullOrWhiteSpace(json))
                throw new OuraException("Oura returned an empty response. The export has stopped.");

            using (var text = new StringReader(json))
            using (var reader = new JsonTextReader(text) { DateParseHandling = DateParseHandling.None, MaxDepth = 32 })
            {
                var result = JObject.Load(reader, new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
                if (reader.Read())
                    throw new JsonReaderException("Unexpected trailing JSON content.");
                return result;
            }
        }
    }

    public static class OuraExportLimits
    {
        public const int MaximumRows = 1048576;
        public const int MaximumColumns = 16384;
        public const int MaximumTextLength = 32767;
        public const long MaximumCells = 5000000;

        public static void Validate(int rows, int columns, int startRow = 1, int startColumn = 1,
            int sheetRows = MaximumRows, int sheetColumns = MaximumColumns)
        {
            if (rows < 0 || columns < 1 || startRow < 1 || startColumn < 1 || sheetRows < 1 || sheetColumns < 1 ||
                (long)startRow + rows - 1 > sheetRows || (long)startColumn + columns - 1 > sheetColumns)
                throw new OuraException("The export will not fit from the selected cell. Select a cell with more space or use a smaller date range.");
            if ((long)rows * columns > MaximumCells)
                throw new OuraException("The export is too large to build safely in memory. Select fewer fields or split the date range.");
        }
    }

    public static class OuraFieldSelections
    {
        private sealed class SavedField
        {
            public string FieldName { get; set; }
            public string CustomLabel { get; set; }
            public bool FieldSelected { get; set; }
            public int FieldOrder { get; set; }
        }

        public static List<OuraFields> Merge(IEnumerable<OuraFields> defaults, string savedJson)
        {
            if (defaults == null) throw new ArgumentNullException(nameof(defaults));
            var fields = defaults.Select(Clone).ToList();
            var saved = new List<SavedField>();
            try
            {
                if (!String.IsNullOrWhiteSpace(savedJson) && savedJson.Length <= 1024 * 1024)
                    saved = JsonConvert.DeserializeObject<List<SavedField>>(savedJson, new JsonSerializerSettings { MaxDepth = 16 }) ?? saved;
            }
            catch (JsonException) { }

            // Trust only presentation preferences from saved settings, never accessor names.
            foreach (var field in fields)
            {
                var preference = saved.LastOrDefault(s => s != null && s.FieldName == field.FieldName);
                if (preference == null) continue;
                field.CustomLabel = preference.CustomLabel != null && preference.CustomLabel.Length <= OuraExportLimits.MaximumTextLength
                    ? preference.CustomLabel : null;
                field.FieldSelected = preference.FieldSelected;
                field.FieldOrder = preference.FieldOrder > 0 ? preference.FieldOrder : field.FieldOrder;
            }
            fields = fields.OrderBy(f => f.FieldOrder).ToList();
            for (int i = 0; i < fields.Count; i++) fields[i].FieldOrder = i + 1;
            return fields;
        }

        public static OuraFields Clone(OuraFields field)
        {
            if (field == null) throw new ArgumentException("An export field is missing.", nameof(field));
            return new OuraFields(field.FieldName, field.OuraSection, field.MethodName, field.FieldOrder,
                field.Accessible, field.Accessor, field.FieldDescription, field.FieldSelected)
            { CustomLabel = field.CustomLabel };
        }
    }

    internal static class OuraArguments
    {
        internal static void ValidateDateRange(DateTime start, DateTime end)
        {
            if (start.Date > end.Date) throw new OuraException("Start date must be on or before end date.");
            if (end.Date == DateTime.MaxValue.Date) throw new OuraException("The selected end date is too large.");
        }

        internal static void ValidateSecret(string value, string description, int maximumLength = 16384)
        {
            if (String.IsNullOrWhiteSpace(value) || value.Length > maximumLength || value.Any(Char.IsControl))
                throw new OuraException("The " + description + " is missing or invalid. Check the Oura application registration or sign in again.");
        }

        internal static void ValidateAccessToken(string value)
        {
            ValidateSecret(value, "access token");
            if (value.Any(Char.IsWhiteSpace)) throw new OuraException("The saved access token is invalid. Reconnect Oura.");
        }
    }
}
