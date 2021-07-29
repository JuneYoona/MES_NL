using System;
using System.Linq;
using System.Windows.Data;
using DevExpress.Utils;

namespace MesAdmin.Models.Common.Utils
{
    public class BoolToDefaultBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? DefaultBoolean.True : DefaultBoolean.False;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
