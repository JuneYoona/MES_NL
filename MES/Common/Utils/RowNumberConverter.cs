using DevExpress.Xpf.Grid;
using MesAdmin.Common.CustomControl;
using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace MesAdmin.Common.Utils
{
    public class RowNumberConverter : MarkupExtension, IMultiValueConverter
    {
        public RowNumberConverter() { }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0].GetType() == typeof(DSTreeListControl)) return string.Format("{0}", (int)values[1] + 1);

            var grid = (GridControl)values[0];
            var rowHandle = (int)values[1];

            string ret = string.Empty;
            if (rowHandle >= 0)
            {
                int parentHandle = grid.GetParentRowHandle(rowHandle);
                if (grid.IsValidRowHandle(parentHandle))
                {
                    int count = grid.GetChildRowCount(parentHandle);
                    for (int i = 0; i < count; i++)
                    {
                        int childHandle = grid.GetChildRowHandle(parentHandle, i);
                        if (childHandle == rowHandle)
                        {
                            ret = string.Format("{0}", i + 1);
                            break;
                        }
                    }
                }
                else ret = string.Format("{0}", rowHandle + 1);
            }

            return ret;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
