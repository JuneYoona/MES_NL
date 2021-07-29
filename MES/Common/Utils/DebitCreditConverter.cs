using System;
using System.Windows.Data;

namespace MesAdmin.Common.Utils
{
    public class DebitCreditConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string dcCode = value as string;
            string dcName = "";

            switch (dcCode)
            {
                case "C":
                    dcName = "감소";
                    break;
                case "D":
                    dcName = "증가";
                    break;
            }

            return dcName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
