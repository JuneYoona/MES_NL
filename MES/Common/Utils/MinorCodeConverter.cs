using System;
using System.Linq;
using System.Windows.Data;
using MesAdmin.Models;

namespace MesAdmin.Common.Utils
{
    public class MinorCodeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string majorCode = values[0] as string;
            string minorCode = values[1] as string;
            if (string.IsNullOrEmpty(majorCode) || string.IsNullOrEmpty(minorCode))
                return string.Empty;

            CommonMinor minor = GlobalCommonMinor.Instance.Where(u => u.MajorCode == majorCode && u.MinorCode == minorCode).FirstOrDefault();
            if (minor == null) return string.Empty;
            return minor.MinorName;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static string Convert(string majorCode, string minorCode)
        {
            CommonMinor minor = GlobalCommonMinor.Instance.Where(u => u.MajorCode == majorCode && u.MinorCode == minorCode).FirstOrDefault();
            if (minor == null) return string.Empty;
            return minor.MinorName;
        }
    }
}
