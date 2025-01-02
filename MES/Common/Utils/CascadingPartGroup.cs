using System;
using System.Linq;
using System.Windows.Data;
using MesAdmin.Models;

namespace MesAdmin.Common.Utils
{
    public class CascadingPartGroup : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string bizAreaCode = value as string;
            if (bizAreaCode == "") return string.Empty;

            return GlobalCommonMinor.Instance.Where(o => o.MajorCode == "PT002" && o.Ref01 == bizAreaCode);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}