using OuraAPIInterface;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelOuraVSTOAddIn
{
    /// <summary>Holds the original worksheet/cell throughout the download. Access only from the Excel UI thread.</summary>
    internal sealed class ExcelExportTarget : IDisposable
    {
        private readonly int ownerThread = Thread.CurrentThread.ManagedThreadId;
        private Excel.Worksheet worksheet;
        private Excel.Range anchor;
        private readonly int startRow;
        private readonly int startColumn;
        private readonly int sheetRows;
        private readonly int sheetColumns;

        internal ExcelExportTarget(Excel.Application application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));
            Excel.Range rows = null;
            Excel.Range columns = null;
            try
            {
                object activeSheet = application.ActiveSheet;
                worksheet = activeSheet as Excel.Worksheet;
                if (worksheet == null)
                {
                    Release(activeSheet);
                    throw new OuraException("Select a worksheet before exporting.");
                }
                if (worksheet.ProtectContents) throw new OuraException("Unprotect the destination worksheet before exporting.");
                anchor = application.ActiveCell;
                if (anchor == null) throw new OuraException("Select a destination cell before exporting.");
                if (!(anchor.MergeCells is bool merged) || merged) throw new OuraException("Select an unmerged destination cell before exporting.");
                startRow = anchor.Row;
                startColumn = anchor.Column;
                rows = worksheet.Rows;
                columns = worksheet.Columns;
                sheetRows = rows.Count;
                sheetColumns = columns.Count;
            }
            catch
            {
                Dispose();
                throw;
            }
            finally
            {
                Release(rows);
                Release(columns);
            }
        }

        internal int MaximumDataRows(int columns, bool headers)
        {
            AssertAccess();
            OuraExportLimits.Validate(headers ? 2 : 1, columns, startRow, startColumn, sheetRows, sheetColumns);
            int available = Math.Min(sheetRows - startRow + 1, (int)(OuraExportLimits.MaximumCells / columns)) - (headers ? 1 : 0);
            if (available < 1) throw new OuraException("The selected cell does not leave space for an export.");
            return available;
        }

        internal string Write(object[,] values, IList<OuraFields> fields, bool headers, bool descriptions)
        {
            AssertAccess();
            if (values == null) throw new ArgumentNullException(nameof(values));
            OuraExportLimits.Validate(values.GetLength(0), values.GetLength(1), startRow, startColumn, sheetRows, sheetColumns);
            Excel.Range block = null;
            Excel.Range heading = null;
            Excel.Font font = null;
            try
            {
                if (worksheet.ProtectContents) throw new OuraException("The destination worksheet was protected during the download. Unprotect it and try again.");
                block = anchor.Resize[values.GetLength(0), values.GetLength(1)];
                if (!(block.MergeCells is bool merged) || merged)
                    throw new OuraException("The destination contains merged cells. Choose an unmerged area.");
                block.Value2 = values;
                try
                {
                    if (headers)
                    {
                        heading = anchor.Resize[1, fields.Count];
                        font = heading.Font;
                        font.Bold = true;
                    }
                    if (headers && descriptions)
                    {
                        for (int column = 0; column < fields.Count; column++)
                        {
                            Excel.Range cell = null;
                            Excel.Comment comment = null;
                            try
                            {
                                cell = anchor.Offset[0, column];
                                cell.ClearComments();
                                if (!String.IsNullOrEmpty(fields[column].FieldDescription)) comment = cell.AddComment(fields[column].FieldDescription);
                            }
                            finally
                            {
                                Release(comment);
                                Release(cell);
                            }
                        }
                    }
                }
                catch (COMException)
                {
                    return "The data was imported, but Excel could not finish the headings or notes. Inspect the destination before retrying.";
                }
                return null;
            }
            catch (COMException)
            {
                throw new OuraException("Excel could not finish writing the export. Check that the original workbook is open and the destination is editable; inspect it for changes before retrying.");
            }
            finally
            {
                Release(font);
                Release(heading);
                Release(block);
            }
        }

        private void AssertAccess()
        {
            if (Thread.CurrentThread.ManagedThreadId != ownerThread)
                throw new OuraException("Excel must be updated from its original UI thread.");
            if (worksheet == null || anchor == null) throw new ObjectDisposedException(nameof(ExcelExportTarget));
        }

        public void Dispose()
        {
            Release(anchor);
            Release(worksheet);
            anchor = null;
            worksheet = null;
        }

        private static void Release(object value)
        {
            if (value == null || !Marshal.IsComObject(value)) return;
            try { Marshal.ReleaseComObject(value); }
            catch (InvalidComObjectException) { }
            catch (COMException) { }
        }
    }
}
