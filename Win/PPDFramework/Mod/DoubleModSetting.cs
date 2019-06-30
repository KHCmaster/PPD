using System;

namespace PPDFramework.Mod
{
    /// <summary>
    /// 倍精度浮動小数の設定を扱うクラスです。
    /// </summary>
    public class DoubleModSetting : NumberModSetting<double>
    {
        /// <summary>
        /// 最小値がクラスの最小値かどうかを取得します。
        /// </summary>
        protected override double ClassMinValue
        {
            get { return double.MinValue; throw new NotImplementedException(); }
        }

        /// <summary>
        /// 最大値がクラスの最大値かどうかを取得します。
        /// </summary>
        protected override double ClassMaxValue
        {
            get { return double.MaxValue; }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        public DoubleModSetting(string key, string name, string description, double defaultValue, double minValue, double maxValue)
            : base(key, name, description, defaultValue, minValue, maxValue)
        {
        }

        /// <summary>
        /// 文字列を取得します。
        /// </summary>
        /// <param name="value">入力データ。</param>
        /// <returns>文字列。</returns>
        public override string GetStringValue(object value)
        {
            return ((double)value).ToString();
        }

        /// <summary>
        /// バリデートを行います。
        /// </summary>
        /// <param name="value">入力文字列。</param>
        /// <param name="val">出力データ。</param>
        /// <returns>バリデートの結果。</returns>
        public override bool Validate(string value, out object val)
        {
            val = null;
            if (!double.TryParse(value, out double temp))
            {
                return false;
            }
            val = temp;

            if (!IsMaxClassMaxValue && temp > MaxValue)
            {
                return false;
            }

            if (!IsMinClassMinValue && temp < MinValue)
            {
                return false;
            }

            return true;
        }
    }
}
