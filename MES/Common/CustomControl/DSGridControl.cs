using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MesAdmin.Common.CustomControl
{
    public class DSGridControl : GridControl
    {
        public bool RowIndicatorNumberOnly { get; set; }

        public DSGridControl()
        {
            CreateDefaultView();
            PastingFromClipboard += DSGridControl_PastingFromClipboard;
            PropertyChanged += DSGridControl_PropertyChanged;

            #region indicator에 선택적으로 row number 및 state image 넣기
            Initialized += (s, e) =>
            {
                var rd = new ResourceDictionary();
                string uri = string.Empty;

                if (RowIndicatorNumberOnly) uri = "pack://application:,,,/MesNL;component/Resources/Dictionary/GridRowIndicator.xaml";
                else uri = "pack://application:,,,/MesNL;component/Resources/Dictionary/GridRowIndicatorState.xaml";

                rd.Source = new Uri(uri);
                Resources.MergedDictionaries.Add(rd);
            };
            #endregion
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
                int j = 0;
                int i = 0;

                Task.Factory.StartNew(() =>
                {
                    Application.Current.Dispatcher.Invoke(async () =>
                    {
                        foreach (string row in rows)
                        {
                            i++;
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

                                // last cell paste(posting to datasource) / grid의 마지막 행/열일경우
                                if ((i == rows.Length || y + 1 == grid.VisibleRowCount) && ++j == cols.Length) view.Focus();

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

        private void DSGridControl_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == "VisibleRowCount")
                {
                    GridControl grid = (GridControl)sender;
                    TableView view = grid.View as TableView;

                    if (grid.VisibleRowCount == 0)
                    {
                        view.IndicatorWidth = 15;
                        return;
                    }

                    double maxWidth = MeasureString(grid.VisibleRowCount.ToString()).Width;
                    view.IndicatorWidth = maxWidth + 22;
                }
            }
            catch { }
        }

        private Size MeasureString(string candidate)
        {
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                12,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }
    }

    public class DSTreeListControl : TreeListControl
    {
        public bool RowIndicatorNumberOnly { get; set; }

        public DSTreeListControl()
        {
            CreateDefaultView();
            PastingFromClipboard += DSGridControl_PastingFromClipboard;

            #region indicator에 선택적으로 row number 및 state image 넣기
            Initialized += (s, e) =>
            {
                var rd = new ResourceDictionary();
                string uri = string.Empty;

                if (RowIndicatorNumberOnly) uri = "pack://application:,,,/Resources/Dictionary/GridRowIndicator.xaml";
                else uri = "pack://application:,,,/Resources/Dictionary/GridRowIndicatorState.xaml";

                rd.Source = new Uri(uri);
                Resources.MergedDictionaries.Add(rd);
            };
            #endregion
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
