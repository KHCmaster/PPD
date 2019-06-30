namespace PPDFramework.Mod
{
    /// <summary>
    /// 整数の設定を扱うクラスです。
    /// </summary>
    public class Int32ModSetting : NumberModSetting<int>
    {
        /// <summary>
        /// 最小値がクラスの最小値かどうかを取得します。
        /// </summary>
        protected override int ClassMinValue
        {
            get { return int.MinValue; }
        }

        /// <summary>
        /// 最大値がクラスの最大値かどうかを取得します。
        /// </summary>
        protected override int ClassMaxValue
        {
            get { return int.MaxValue; }
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
        public Int32ModSetting(string key, string name, string description, int defaultValue, int minValue, int maxValue)
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
            return ((int)value).ToString();
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
            if (!int.TryParse(value, out int temp))
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
