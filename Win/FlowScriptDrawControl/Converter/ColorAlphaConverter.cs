using System;
using System.Windows.Data;
using System.Windows.Media;

namespace FlowScriptDrawControl.Converter
{
    class ColorAlphaConverter : IValueConverter
    {
        #region IValueConverter メンバー

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var color = (Color)value;
            double alpha = 0.5;
            if (parameter is string param && !double.TryParse(param, out alpha))
            {
                alpha = 0.5;
            }
            return Color.FromArgb((byte)(alpha * 255), color.R, color.G, color.B);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
