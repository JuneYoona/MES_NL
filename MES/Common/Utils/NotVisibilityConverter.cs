using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MesAdmin.Common.Utils
{
    public class NotVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Visible;
            if (value is int) return (int)value != 0 ? Visibility.Collapsed : Visibility.Visible;
            if (value is string) return !string.IsNullOrEmpty((string)value) ? Visibility.Collapsed : Visibility.Visible;
            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}