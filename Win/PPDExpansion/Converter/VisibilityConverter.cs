using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PPDExpansion.Converter
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val;
            if (value == null)
            {
                val = false;
            }
            else if (value is bool)
            {
                val = (bool)value;
            }
            else
            {
                val = true;
            }
            if (parameter is string && (string)parameter == "Invert")
            {
                val = !val;
            }
            return val ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
