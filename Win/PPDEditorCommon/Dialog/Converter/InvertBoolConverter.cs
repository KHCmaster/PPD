using System;
using System.Windows.Data;

namespace PPDEditorCommon.Dialog.Converter
{
    class InvertBoolConverter : IValueConverter
    {
        #region IValueConverter メンバー

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter is bool)
            {
                return !((bool)parameter);
            }
            return parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
