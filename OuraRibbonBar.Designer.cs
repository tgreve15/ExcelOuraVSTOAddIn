// VSTO host shell only. All ribbon UI configuration lives in OuraRibbonDefinition
// and is constructed uniformly by OuraRibbonBar.Layout before Office loads it.
namespace ExcelOuraVSTOAddIn
{
    partial class OuraRibbonBar : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        private void InitializeComponent()
        {
            Name = "OuraRibbonBar";
            RibbonType = "Microsoft.Excel.Workbook";
            Load += OuraRibbonBar_Load;
        }
    }

    partial class ThisRibbonCollection
    {
        internal OuraRibbonBar OuraRibbonBar => GetRibbon<OuraRibbonBar>();
    }
}
