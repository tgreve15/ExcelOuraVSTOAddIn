using ExcelOuraVSTOAddIn;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace OuraAPIInterface
{
    public static class OuraExportTable
    {
        public static object[,] Build<T>(IList<T> rows, IList<OuraFields> fields, bool headers,
            CancellationToken cancellation = default(CancellationToken))
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            OuraExportLimits.Validate(checked(rows.Count + (headers ? 1 : 0)), fields.Count);
            cancellation.ThrowIfCancellationRequested();
            var readers = fields.Select(CreateReader<T>).ToArray();
            var table = new object[rows.Count + (headers ? 1 : 0), fields.Count];
            for (int column = 0; column < fields.Count; column++)
            {
                var field = fields[column];
                if (headers) table[0, column] = CellValue(String.IsNullOrEmpty(field.CustomLabel) ? field.FieldName : field.CustomLabel);
                for (int row = 0; row < rows.Count; row++)
                {
                    cancellation.ThrowIfCancellationRequested();
                    if (Object.Equals(rows[row], null)) throw new OuraException("The export contains an invalid empty row.");
                    try { table[row + (headers ? 1 : 0), column] = CellValue(readers[column](rows[row])); }
                    catch (TargetInvocationException) { throw new OuraException("An export field could not be read. Update the add-in or reset the field selection."); }
                }
            }
            return table;
        }

        private static Func<T, object> CreateReader<T>(OuraFields field)
        {
            if (field == null || String.IsNullOrWhiteSpace(field.MethodName)) throw new OuraException("An export field is missing its definition.");
            if (typeof(T) == typeof(OuraRecord))
            {
                if (!field.MethodName.Split('.').All(part => part.Length > 0 && part.All(c => Char.IsLetterOrDigit(c) || c == '_')))
                    throw new OuraException("The export contains an invalid field path.");
                return row => ((OuraRecord)(object)row).Read(field.MethodName);
            }
            if (field.Accessor == OuraFields.AccessorType.Method)
            {
                var method = typeof(T).GetMethod(field.MethodName, BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
                if (method != null && method.ReturnType != typeof(void)) return row => method.Invoke(row, null);
            }
            else if (field.Accessor == OuraFields.AccessorType.Property)
            {
                var property = typeof(T).GetProperty(field.MethodName, BindingFlags.Instance | BindingFlags.Public);
                if (property != null && property.CanRead && property.GetIndexParameters().Length == 0) return row => property.GetValue(row);
            }
            throw new OuraException("An export field is no longer recognized. Reset the saved field selection.");
        }

        internal static object CellValue(object value)
        {
            if (value == null) return null;
            if (value is DateTimeOffset) return ((DateTimeOffset)value).ToString("o", CultureInfo.InvariantCulture);
            if (value is DateTime) return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            if (value is string)
            {
                string text = (string)value;
                string trimmed = text.TrimStart();
                if (trimmed.Length > 0 && "=+-@".IndexOf(trimmed[0]) >= 0) text = "'" + text;
                if (text.Length > OuraExportLimits.MaximumTextLength)
                    throw new OuraException("An export value exceeds Excel's cell text limit. Deselect raw samples or choose a shorter session.");
                return text;
            }
            if (value is double && (Double.IsNaN((double)value) || Double.IsInfinity((double)value)) ||
                value is float && (Single.IsNaN((float)value) || Single.IsInfinity((float)value)))
                throw new OuraException("Oura returned a non-finite numeric value. The export has stopped.");
            if (value is long && ((long)value > 999999999999999L || (long)value < -999999999999999L))
                return "'" + ((long)value).ToString(CultureInfo.InvariantCulture);
            if (value is IEnumerable)
            {
                var items = new List<string>();
                foreach (var item in (IEnumerable)value) items.Add(Convert.ToString(item, CultureInfo.InvariantCulture));
                return CellValue(String.Join(";", items));
            }
            if (value is bool || value is IConvertible) return value;
            throw new OuraException("An export field contains an unsupported value type.");
        }
    }
}
