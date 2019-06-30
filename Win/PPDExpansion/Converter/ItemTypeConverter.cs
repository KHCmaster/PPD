using PPDMultiCommon.Data;
using System;
using System.Windows.Data;

namespace PPDExpansion.Converter
{
    class ItemTypeConverter : IValueConverter
    {
        #region IValueConverter メンバー

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var itemType = (ItemType)value;
            return Utility.Language[itemType.ToString()];
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
