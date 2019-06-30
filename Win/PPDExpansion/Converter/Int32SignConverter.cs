using System;
using System.Windows.Data;

namespace PPDExpansion.Converter
{
    class Int32SignConverter : IValueConverter
    {
        #region IValueConverter メンバー

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var val = (int)value;
            return String.Format("{0}{1}", val > 0 ? "+" : "", val);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
