using System;
using System.Linq;
using System.Windows.Data;
using MesAdmin.Models;

namespace MesAdmin.Common.Utils
{
    public class CascadingWorkArea : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string bizAreaCode = value as string;
            if (bizAreaCode == "") return string.Empty;

            return GlobalCommonWorkAreaInfo.Instance.Where(p => p.BizAreaCode == bizAreaCode && p.IsEnabled == true);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
