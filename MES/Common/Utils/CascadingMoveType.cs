using System;
using System.Linq;
using System.Windows.Data;
using MesAdmin.Models;

namespace MesAdmin.Common.Utils
{
    public class CascadingMoveType : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string transType = value as string;
            if (transType == "") return null;

            return GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0001" && u.Ref02 == transType && u.IsEnabled);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}