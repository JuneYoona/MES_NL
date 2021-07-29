using System;
using System.Linq;
using System.Windows.Data;
using MesAdmin.Models;

namespace MesAdmin.Common.Utils
{
    public class BizCodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string bizCode = value as string;
            if (bizCode == "") return string.Empty;

            CommonBizPartner bizPartner = GlobalCommonBizPartner.Instance.Where(u => u.BizCode == bizCode).FirstOrDefault();
            if (bizPartner == null) return string.Empty;
            return bizPartner.BizName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
