using System;
using System.Linq;
using System.Windows.Data;
using MesAdmin.Models;
using System.Collections;

namespace MesAdmin.Common.Utils
{
    public class CascadingItemAccount : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string bizAreaCode = value as string;
            if (bizAreaCode == "") return string.Empty;

            IEnumerable accounts = null;
            try
            {
                accounts = GlobalCommonMinor.Instance.Where(p => p.MajorCode == "P1001" && p.Ref01 == bizAreaCode && p.IsEnabled == true);
            }
            catch { }

            return accounts;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
