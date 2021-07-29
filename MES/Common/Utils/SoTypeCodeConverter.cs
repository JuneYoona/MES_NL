using System;
using System.Linq;
using System.Windows.Data;
using MesAdmin.Models;


namespace MesAdmin.Common.Utils
{
    public class SoTypeCodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string code = value as string;
            if (code == "") return string.Empty;

            SalesOrderTypeConfig soType = GlobalCommonSoTypeList.Instance.Where(u => u.SoType == code.Trim()).FirstOrDefault();
            if (soType == null) return string.Empty;
            return soType.SoTypeName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
