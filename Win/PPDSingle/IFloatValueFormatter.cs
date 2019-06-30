using System;

namespace PPDSingle
{
    interface IFloatValueFormatter
    {
        string Format(float value);
    }

    class FloatToFloatFormatter : IFloatValueFormatter
    {
        static FloatToFloatFormatter formatter = new FloatToFloatFormatter();
        public static FloatToFloatFormatter Formatter
        {
            get
            {
                return formatter;
            }
        }
        #region IFloatValueFormatter メンバ

        public string Format(float value)
        {
            return value.ToString();
        }

        #endregion
    }

    class FloatToIntFormatter : IFloatValueFormatter
    {
        static FloatToIntFormatter formatter = new FloatToIntFormatter();
        public static FloatToIntFormatter Formatter
        {
            get
            {
                return formatter;
            }
        }
        #region IFloatValueFormatter メンバ

        public string Format(float value)
        {
            return ((int)value).ToString();
        }

        #endregion
    }

    class FloatToTimeFormatter : IFloatValueFormatter
    {
        static FloatToTimeFormatter formatter = new FloatToTimeFormatter();
        public static FloatToTimeFormatter Formatter
        {
            get
            {
                return formatter;
            }
        }
        #region IFloatValueFormatter メンバ

        public string Format(float value)
        {
            var val = (int)value;
            return String.Format("{0}:{1:D2}", val / 60, val - (val / 60) * 60);
        }

        #endregion
    }
}
