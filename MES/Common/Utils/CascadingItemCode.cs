using System;
using System.Linq;
using System.Windows.Data;
using MesAdmin.Models;
using System.Collections.Generic;

namespace MesAdmin.Common.Utils
{
    public class CascadingItemCode : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string waCode = value as string;
            if (waCode == "") return string.Empty;

            return new CommonItemList().Where(o => o.InWaCode == waCode && o.IsEnabled == true);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
