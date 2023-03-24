using System;
using System.Linq;
using System.Windows.Data;
using MesAdmin.Models;

namespace MesAdmin.Common.Utils
{
    public class BizAreaCodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string bizAreaCode = value as string;
            if (bizAreaCode == "") return string.Empty;

            string bizName = string.Empty;

            try
            {
                CommonMinor minor = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004" && u.MinorCode == bizAreaCode).FirstOrDefault();
                bizName = minor == null ? "" : minor.MinorName;
            }
            catch { }

            return bizName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}