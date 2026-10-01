using System;

namespace PPDSingle
{
    interface IFloatValueFormatter
    {
        string Format(float value);
    }

    class FloatToFloatFormatter : IFloatValueFormatter
    {
        static readonly FloatToFloatFormatter formatter = new();
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
        static readonly FloatToIntFormatter formatter = new();
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
        static readonly FloatToTimeFormatter formatter = new();
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
