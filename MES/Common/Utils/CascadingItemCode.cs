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

            IEnumerable<CommonMinorDetail> detail = new CommonMinorDetailList("P2001", waCode).Where(o => o.IsEnabled == true);
            CommonItemList items = new CommonItemList();
            var ret =
                from a in detail
                join b in items on a.DetailCode equals b.ItemCode
                select new { b.ItemCode, b.ItemName, b.ItemSpec };
                    
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
