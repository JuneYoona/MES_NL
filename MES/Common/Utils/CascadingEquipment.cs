using System;
using System.Linq;
using System.Windows.Data;
using MesAdmin.Models;

namespace MesAdmin.Common.Utils
{
    public class CascadingEquipment : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string waCode = value as string;
            if (waCode == "") return string.Empty;
            return GlobalCommonEquipment.Instance.Where(p => p.WaCode == waCode && p.IsEnabled == true);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
