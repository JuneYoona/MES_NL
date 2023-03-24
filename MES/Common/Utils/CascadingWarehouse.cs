using System;
using System.Collections;
using System.Linq;
using System.Windows.Data;
using MesAdmin.Models;

namespace MesAdmin.Common.Utils
{
    public class CascadingWarehouse : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string bizAreaCode = value as string;
            if (bizAreaCode == "") return string.Empty;

            IEnumerable whNames = null;
            try
            {
                whNames = GlobalCommonMinor.Instance.Where(p => p.MajorCode == "I0011" && p.Ref01 == bizAreaCode && p.IsEnabled == true);
            }
            catch { }

            return whNames;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
