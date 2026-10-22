// Compile-check scaffolding only. Production uses VSTO's actual host and ribbon types.
// Both the real ribbon shell and runtime builder are compiled against these signatures.
namespace Microsoft.Office.Core
{
    public enum RibbonControlSize { RibbonControlSizeRegular, RibbonControlSizeLarge }
}
namespace Microsoft.Office.Tools.Ribbon
{
    public class RibbonUIEventArgs : System.EventArgs { }
    public class RibbonControlEventArgs : System.EventArgs { }
    public delegate void RibbonUIEventHandler(object sender, RibbonUIEventArgs args);
    public delegate void RibbonControlEventHandler(object sender, RibbonControlEventArgs args);
    public enum RibbonControlIdType { Custom, Office }
    public sealed class RibbonControlId
    {
        public RibbonControlIdType ControlIdType { get; set; }
    }
    public class RibbonButton
    {
        public bool Visible { get; set; }
        public bool Enabled { get; set; }
        public bool ShowImage { get; set; }
        public bool ShowLabel { get; set; }
        public System.Drawing.Image Image { get; set; }
        public Microsoft.Office.Core.RibbonControlSize ControlSize { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string ScreenTip { get; set; }
        public string SuperTip { get; set; }
        public event RibbonControlEventHandler Click { add { } remove { } }
    }
    public class RibbonGroup
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public System.Collections.Generic.List<RibbonButton> Items { get; } = new System.Collections.Generic.List<RibbonButton>();
    }
    public class RibbonTab
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public RibbonControlId ControlId { get; } = new RibbonControlId();
        public System.Collections.Generic.List<RibbonGroup> Groups { get; } = new System.Collections.Generic.List<RibbonGroup>();
    }
    public class CompileCheckFactory
    {
        public RibbonButton CreateRibbonButton() => new RibbonButton();
        public RibbonGroup CreateRibbonGroup() => new RibbonGroup();
        public RibbonTab CreateRibbonTab() => new RibbonTab();
    }
    public class RibbonBase : System.IDisposable
    {
        public RibbonBase(CompileCheckFactory factory) { Factory = factory; }
        protected CompileCheckFactory Factory { get; }
        public string Name { get; set; }
        public string RibbonType { get; set; }
        public System.Collections.Generic.List<RibbonTab> Tabs { get; } = new System.Collections.Generic.List<RibbonTab>();
        public event RibbonUIEventHandler Load { add { } remove { } }
        protected virtual void Dispose(bool disposing) { }
        public void Dispose() => Dispose(true);
    }
}
namespace ExcelOuraVSTOAddIn
{
    public partial class ThisRibbonCollection
    {
        protected T GetRibbon<T>() where T : new() => new T();
    }
    internal static class Globals
    {
        internal static readonly CompileCheckAddIn ThisAddIn = new CompileCheckAddIn();
        internal static readonly CompileCheckHostFactory Factory = new CompileCheckHostFactory();
    }
    internal sealed class CompileCheckHostFactory
    {
        internal Microsoft.Office.Tools.Ribbon.CompileCheckFactory GetRibbonFactory() => new Microsoft.Office.Tools.Ribbon.CompileCheckFactory();
    }
    internal sealed class CompileCheckAddIn
    {
        internal Microsoft.Office.Interop.Excel.Application Application { get; set; }
    }
}
