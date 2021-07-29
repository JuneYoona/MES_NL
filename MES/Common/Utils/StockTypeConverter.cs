using System;
using System.Windows.Data;

namespace MesAdmin.Common.Utils
{
    public class StockTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string typeCode = value as string;
            string typeName = "";

            switch (typeCode)
            {
                case "G":
                    typeName = "양품수량";
                    break;
                case "Q":
                    typeName = "품질검사수량";
                    break;
                case "B":
                    typeName = "불량수량";
                    break;
            }

            return typeName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
