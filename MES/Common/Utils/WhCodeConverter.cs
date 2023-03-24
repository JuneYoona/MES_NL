using System;
using System.Linq;
using System.Windows.Data;
using MesAdmin.Models;

namespace MesAdmin.Common.Utils
{
    public class WhCodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string whCode = value as string;
            if (whCode == "") return string.Empty;

            CommonMinor minor = null;
            try
            {
                minor = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0011" && u.MinorCode == whCode).FirstOrDefault();
            }
            catch { }

            if (minor == null) return string.Empty;
            return minor.MinorName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}