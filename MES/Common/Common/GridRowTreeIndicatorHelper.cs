using DevExpress.Xpf.Grid;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace MesAdmin.Common.Common
{
    public class GridRowTreeIndicatorHelper
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(GridRowTreeIndicatorHelper), new FrameworkPropertyMetadata(IsEnabledPropertyChanged));
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
            TreeListControl grid = source as TreeListControl;
            grid.PropertyChanged += grid_PropertyChanged;
        }

        static void grid_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == "VisibleRowCount")
                {
                    TreeListControl grid = (TreeListControl)sender;
                    TreeListView view = grid.View as TreeListView;

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