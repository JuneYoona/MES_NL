using System;
using System.Linq;
using System.Windows.Data;
using MesAdmin.Models;

namespace MesAdmin.Common.Utils
{
    public class EqpCodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string eqpCode = value as string;
            if (eqpCode == "") return string.Empty;

            CommonEquipment equipment = GlobalCommonEquipment.Instance.Where(u => u.EqpCode == eqpCode).FirstOrDefault();
            if (equipment == null) return string.Empty;
            return equipment.EqpName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
