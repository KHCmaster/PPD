using System;
using System.Windows;
using System.Windows.Data;

namespace PPDEditorCommon.Dialog.Converter
{
    class VisibilityConverter : IValueConverter
    {
        #region IValueConverter メンバー

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool val = value is bool v ? v : value != null;
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
