using System;
using System.Windows;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Core;
using System.Threading.Tasks;

namespace MesAdmin.Common.CustomControl
{
    public class DSGridControl : GridControl
    {
        public DSGridControl()
        {
            this.CreateDefaultView();
            this.PastingFromClipboard += DSGridControl_PastingFromClipboard;

           // this.Loaded += (o, s) => { ((TableView)this.View).BestFitColumns(); };
        }

        private void DSGridControl_PastingFromClipboard(object sender, PastingFromClipboardEventArgs e)
        {
            e.Handled = true;
            GridControl grid = sender as GridControl;
            TableView view = grid.View as TableView;

            string[] rows = Clipboard.GetText().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                var cell = view.GetSelectedCells();
                int y = cell[0].RowHandle;
                int x = cell[0].Column.VisibleIndex;

                Task.Factory.StartNew(() =>
                {
                    Application.Current.Dispatcher.Invoke(async () =>
                    {
                        foreach (string row in rows)
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(50));
                            string[] cols = row.Split('\t');
                            foreach (string col in cols)
                            {
                                view.FocusedRowHandle = y;
                                view.DataControl.CurrentColumn = grid.Columns[x];

                                if (grid.Columns[x].AllowEditing != DevExpress.Utils.DefaultBoolean.False && grid.Columns[x].ReadOnly != true)
                                {
                                    await Task.Delay(TimeSpan.FromMilliseconds(50));
                                    view.ShowEditor();
                                    grid.SetCellValue(y, grid.Columns[x].FieldName, col.Trim());
                                    view.HideEditor();
                                }

                                x += 1;
                                if (x >= view.VisibleColumns.Count) break;
                            }
                            y += 1;
                            x = cell[0].Column.VisibleIndex;
                            if (y >= grid.VisibleRowCount) break;
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                DXMessageBox.Show(this, ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None, MessageBoxOptions.None, FloatingMode.Popup);
            }
        }
    }

    public class DSTreeListControl : TreeListControl
    {
        public DSTreeListControl()
        {
            this.CreateDefaultView();
            this.PastingFromClipboard += DSGridControl_PastingFromClipboard;
        }

        private void DSGridControl_PastingFromClipboard(object sender, PastingFromClipboardEventArgs e)
        {
            TreeListControl grid = sender as TreeListControl;
            TreeListView view = grid.View as TreeListView;

            string[] rows = Clipboard.GetText().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                var cell = view.GetSelectedCells();
                int y = cell[0].RowHandle;
                int x = cell[0].Column.VisibleIndex;

                foreach (string row in rows)
                {
                    string[] cols = row.Split('\t');
                    foreach (string col in cols)
                    {
                        view.FocusedRowHandle = y;
                        view.DataControl.CurrentColumn = grid.Columns[x];

                        if (grid.Columns[x].AllowEditing != DevExpress.Utils.DefaultBoolean.False)
                        {
                            view.ShowEditor();
                            view.SetNodeValue(view.GetNodeByRowHandle(y), grid.Columns[x].FieldName, col.Trim());
                            view.HideEditor();
                        }

                        x += 1;
                        if (x >= view.VisibleColumns.Count) break;
                    }
                    y += 1;
                    x = cell[0].Column.VisibleIndex;
                    if (y >= grid.VisibleRowCount) break;
                }
            }
            catch (Exception ex)
            {
                DXMessageBox.Show(this, ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None, MessageBoxOptions.None, FloatingMode.Popup);
            }
        }
    }
}
