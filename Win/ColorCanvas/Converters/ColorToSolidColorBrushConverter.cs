using System;
using System.Windows.Data;
using System.Windows.Media;

namespace ColorCanvas.Converters
{
    public class ColorToSolidColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                return new SolidColorBrush((Color)value);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                return ((SolidColorBrush)value).Color;
            }

            return value;
        }
    }
}
