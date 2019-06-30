namespace PPDFramework.Mod
{
    /// <summary>
    /// 文字列の設定を扱うクラスです。
    /// </summary>
    public class StringModSetting : TemplateModSetting<string>
    {
        /// <summary>
        /// 文字列の最大長を取得します。
        /// </summary>
        public int MaxLength
        {
            get;
            private set;
        }

        /// <summary>
        /// 最大長が設定されているかどうかを取得します。
        /// </summary>
        public bool IsMaxLengthClassMaxLength
        {
            get
            {
                return int.MaxValue == MaxLength;
            }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        /// <param name="maxLength"></param>
        public StringModSetting(string key, string name, string description, string defaultValue, int maxLength)
            : base(key, name, description, defaultValue)
        {
            MaxLength = maxLength;
        }

        /// <summary>
        /// 文字列を取得します。
        /// </summary>
        /// <param name="value">入力データ。</param>
        /// <returns>文字列。</returns>
        public override string GetStringValue(object value)
        {
            return (string)value;
        }

        /// <summary>
        /// バリデートを行います。
        /// </summary>
        /// <param name="value">入力文字列。</param>
        /// <param name="val">出力データ。</param>
        /// <returns>バリデートの結果。</returns>
        public override bool Validate(string value, out object val)
        {
            val = value;
            if (!IsMaxLengthClassMaxLength && value.Length > MaxLength)
            {
                return false;
            }
            return true;
        }
    }
}
