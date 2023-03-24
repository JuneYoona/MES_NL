using System;
using System.Linq;
using System.Windows.Data;
using MesAdmin.Models;

namespace MesAdmin.Common.Utils
{
    public class WaCodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string waCode = value as string;
            if (waCode == "") return string.Empty;

            CommonWorkAreaInfo workAreaInfo = null;
            try
            {
                workAreaInfo = GlobalCommonWorkAreaInfo.Instance.Where(u => u.WaCode == waCode).FirstOrDefault();
            }
            catch { }

            if (workAreaInfo == null) return string.Empty;
            return workAreaInfo.WaName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}