using PPDFrameworkCore;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PPDExpansion.Converter
{
    class ScoreConverter : IMultiValueConverter
    {
        #region IMultiValueConverter メンバー

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var scoreName = values[0] as string;
            if (String.IsNullOrEmpty(scoreName))
            {
                return "";
            }
            var difficulty = (Difficulty)values[1];
            return string.Format("{0} {1}", scoreName, difficulty);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
