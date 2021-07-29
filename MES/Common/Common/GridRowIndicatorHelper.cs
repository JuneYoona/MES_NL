using DevExpress.Mvvm;
using DevExpress.Xpf.Grid;
using System.Windows;
using System.Windows.Media;
using System.Globalization;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;

namespace MesAdmin.Common.Common
{
    public class GridRowIndicatorHelper
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(GridRowIndicatorHelper), new FrameworkPropertyMetadata(IsEnabledPropertyChanged));
        public static void SetIsEnabled(UIElement element, bool value)
        {
            element.SetValue(IsEnabledProperty, value);
        }
        public static bool GetIsEnabled(UIElement element)
        {
            return (bool)element.GetValue(IsEnabledProperty);
        }

        private static void IsEnabledPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            GridControl grid = source as GridControl;
            grid.PropertyChanged += grid_PropertyChanged;

            // PLinqInstantFeedbackDataSource(Server Mode)에서 그룹화 해제 후
            // VisibleRowCount PropertyChanged Event 가 발생하지 않아 강제로 발생
             grid.AsyncOperationCompleted += grid_AsyncOperationCompleted;
        }

        static int visibleRowCount;
        private static void grid_AsyncOperationCompleted(object sender, RoutedEventArgs e)
        {
            try
            {
                GridControl grid = (GridControl)sender;
                if (visibleRowCount != grid.VisibleRowCount)
                {
                    grid_PropertyChanged(sender, new PropertyChangedEventArgs("VisibleRowCount"));
                    visibleRowCount = grid.VisibleRowCount;
                }
            }
            catch { }
        }

        static void grid_PropertyChanged(object sender, PropertyChangedEventArgs e)
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

        public static Size MeasureString(string candidate)
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
}
