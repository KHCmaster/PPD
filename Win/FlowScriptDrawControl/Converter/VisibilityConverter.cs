using System;
using System.Windows;
using System.Windows.Data;

namespace FlowScriptDrawControl.Converter
{
    class VisibilityConverter : IValueConverter
    {
        #region IValueConverter メンバー

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var param = parameter as string;
            bool val;
            if (value is bool)
            {
                val = (bool)value;
            }
            else if (value is string)
            {
                val = !String.IsNullOrEmpty((string)value);
            }
            else
            {
                val = value != null;
            }
            if (param != null && param == "Invert")
            {
                val = !val;
            }
            return val ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
