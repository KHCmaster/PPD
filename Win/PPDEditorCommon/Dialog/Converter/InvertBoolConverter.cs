using System;
using System.Windows.Data;

namespace PPDEditorCommon.Dialog.Converter
{
    class InvertBoolConverter : IValueConverter
    {
        #region IValueConverter メンバー

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // 反転するのは結び付けた値（以前は parameter を反転していた）
            if (value is bool v)
            {
                return !v;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
