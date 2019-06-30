namespace PPDFramework.Mod
{
    /// <summary>
    /// 真偽値の設定を扱うクラスです。
    /// </summary>
    public class BooleanModSetting : TemplateModSetting<bool>
    {
        /// <summary>
        /// 利用可能な値を取得します。
        /// </summary>
        public override object[] AvailableValues
        {
            get
            {
                return new object[] { false, true };
            }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        public BooleanModSetting(string key, string name, string description, bool defaultValue)
            : base(key, name, description, defaultValue)
        {

        }

        /// <summary>
        /// 文字列を取得します。
        /// </summary>
        /// <param name="value">入力データ。</param>
        /// <returns>文字列。</returns>
        public override string GetStringValue(object value)
        {
            return ((bool)value).ToString();
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
            if (bool.TryParse(value, out bool ret))
            {
                val = ret;
                return true;
            }
            return false;
        }
    }
}
