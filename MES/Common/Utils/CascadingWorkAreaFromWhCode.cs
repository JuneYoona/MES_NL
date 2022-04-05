using System;
using System.Linq;
using System.Windows.Data;
using MesAdmin.Models;

namespace MesAdmin.Common.Utils
{
    public class CascadingWorkAreaFromWhCode : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string whCode = value as string;
            if (string.IsNullOrEmpty(whCode)) return string.Empty;

            return GlobalCommonWorkAreaInfo.Instance.Where(p => p.WhCode == whCode && p.IsEnabled == true);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}