using System;
using System.Linq;
using System.Windows.Data;
using MesAdmin.Models;
using System.Collections.Generic;

namespace MesAdmin.Common.Utils
{
    public class CascadingWorkArea : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string bizAreaCode = value as string;
            if (bizAreaCode == "") return string.Empty;

            IEnumerable<CommonWorkAreaInfo> wa = null;
            try
            {
                wa = GlobalCommonWorkAreaInfo.Instance.Where(p => p.BizAreaCode == bizAreaCode && p.IsEnabled == true);
            }
            catch { }

            return wa;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
