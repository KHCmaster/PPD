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
            bool val;
            if (value is bool v)
            {
                val = v;
            }
            else if (value is string v1)
            {
                val = !String.IsNullOrEmpty(v1);
            }
            else
            {
                val = value != null;
            }
            if (parameter is string param && param == "Invert")
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
