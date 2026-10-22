using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ExcelOuraVSTOAddIn
{
    /// <summary>Reorders selected rows only within this control, preserving their relative order.</summary>
    public class ListViewWithReordering : ListView
    {
        protected override void OnItemDrag(ItemDragEventArgs e)
        {
            base.OnItemDrag(e);
            if (SelectedItems.Count > 0) DoDragDrop(this, DragDropEffects.Move);
        }

        private bool IsLocalMove(DragEventArgs args) => args.Data != null &&
            ReferenceEquals(args.Data.GetData(typeof(ListViewWithReordering)), this) &&
            (args.AllowedEffect & DragDropEffects.Move) != 0;

        protected override void OnDragEnter(DragEventArgs args)
        {
            base.OnDragEnter(args);
            args.Effect = IsLocalMove(args) ? DragDropEffects.Move : DragDropEffects.None;
        }

        protected override void OnDragDrop(DragEventArgs args)
        {
            base.OnDragDrop(args);
            if (!IsLocalMove(args) || SelectedItems.Count == 0) return;
            Point location = PointToClient(new Point(args.X, args.Y));
            var target = GetItemAt(location.X, location.Y);
            if (target == null || target.Selected) return;
            var selected = SelectedItems.Cast<ListViewItem>().OrderBy(item => item.Index).ToArray();
            int insertion = target.Index - selected.Count(item => item.Index < target.Index);
            BeginUpdate();
            try
            {
                foreach (var item in selected) Items.Remove(item);
                foreach (var item in selected) Items.Insert(insertion++, item);
            }
            finally { EndUpdate(); }
        }
    }
}
