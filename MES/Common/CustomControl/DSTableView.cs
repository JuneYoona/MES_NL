using System;
using System.Collections.Generic;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Bars;
using System.Windows.Media.Imaging;
using System.Linq;

namespace MesAdmin.Common.CustomControl
{
    public class DSTableView : TableView
    {
        BarButtonItem barSummary;
        BarButtonItem barMin;
        BarButtonItem barMax;
        BarButtonItem barCount;
        BarButtonItem barAverage;

        BarButtonItem BarSummary
        {
            get
            {
                barSummary = barSummary ?? new BarButtonItem { Name = "summary", Glyph = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/ItemSum.png")) };
                return barSummary;
            }
        }
        BarButtonItem BarMin
        {
            get
            {
                barMin = barMin ?? new BarButtonItem { Name = "min", Glyph = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/ItemMin.png")) };
                return barMin;
            }
        }
        BarButtonItem BarMax
        {
            get
            {
                barMax = barMax ?? new BarButtonItem { Name = "max", Glyph = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/ItemMax.png")) };
                return barMax;
            }
        }
        BarButtonItem BarCount
        {
            get
            {
                barCount = barCount ?? new BarButtonItem { Name = "count", Glyph = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/ItemCount.png")) };
                return barCount;
            }
        }
        BarButtonItem BarAverage
        {
            get
            {
                barAverage = barAverage ?? new BarButtonItem { Name = "average", Glyph = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/ItemAverage.png")) };
                return barAverage;
            }
        }

        public DSTableView()
        {
            ShowGridMenu += DSTableView_ShowGridMenu;
            AllowMergedGrouping = true;
            MergedGroupingMode = MergedGroupingMode.Always;
            AllowCommitOnValidationAttributeError = true;
            EnableImmediatePosting = true; //필드값이 바뀔경우 focus in 상태에서도 CellValueChang 이벤트 발생
            ShowEmptyText = true;

            // gridcontrol optimizing(scroll)
            this.AllowCascadeUpdate = true;
            //this.RowAnimationKind = RowAnimationKind.None;
            //this.RowOpacityAnimationDuration = new System.Windows.Duration(TimeSpan.FromMilliseconds(100));
        }

        private void DSTableView_ShowGridMenu(object sender, GridMenuEventArgs e)
        {
            try
            {
                if (e.MenuType != GridMenuType.RowCell) return;

                TableView view = sender as TableView;
                GridControl grid = view.Grid;
                IList<GridCell> selectedCells = view.GetSelectedCells();
                List<decimal> list = new List<decimal>();
                
                foreach (var item in selectedCells)
                {
                    var cellValue = grid.GetCellValue(item.RowHandle, item.Column);
                    if (cellValue != null)
                    {
                        decimal temp = 0;
                        bool res = decimal.TryParse(cellValue.ToString(), out temp);
                        if (res)
                            list.Add(temp);
                    }
                }

                string average = list.Count() == 0 ? "0" : (list.Sum() / list.Count()).ToString("#,#.######");
                string summary = list.Count() == 0 ? "0" : list.Sum().ToString("#,#.######");
                string count = list.Count().ToString("n0");
                string max = list.Count() == 0 ? "0" : list.Max().ToString("#,#.######");
                string min = list.Count() == 0 ? "0" : list.Min().ToString("#,#.######");

                BarSummary.Content = "선택합계 : " + summary;
                BarCount.Content = "선택개수 : " + count;
                BarAverage.Content = "선택평균 : " + average;
                BarMax.Content = "선택최대값 : " + max;
                BarMin.Content = "선택최소값 : " + min;

                e.Customizations.Add(BarSummary);
                e.Customizations.Add(BarCount);
                e.Customizations.Add(BarAverage);
                e.Customizations.Add(BarMin);
                e.Customizations.Add(BarMax);
            }
            catch { }
        }
    }
}