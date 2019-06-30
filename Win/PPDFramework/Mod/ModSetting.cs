namespace PPDFramework.Mod
{
    /// <summary>
    /// MODの設定を扱うクラスです。
    /// </summary>
    public abstract class ModSetting
    {
        /// <summary>
        /// キーを取得します。
        /// </summary>
        public string Key
        {
            get;
            private set;
        }

        /// <summary>
        /// 名前を取得します。
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 説明を取得します。
        /// </summary>
        public string Description
        {
            get;
            private set;
        }

        /// <summary>
        /// デフォルト値を取得します。
        /// </summary>
        public abstract object Default
        {
            get;
        }

        /// <summary>
        /// 利用可能な値を取得します。
        /// </summary>
        public virtual object[] AvailableValues
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        protected ModSetting(string key, string name, string description)
        {
            Key = key;
            Name = name;
            Description = description;
        }

        /// <summary>
        /// バリデートを行います。
        /// </summary>
        /// <param name="value">入力文字列。</param>
        /// <param name="val">出力データ。</param>
        /// <returns>バリデートの結果。</returns>
        public abstract bool Validate(string value, out object val);

        /// <summary>
        /// 文字列を取得します。
        /// </summary>
        /// <param name="value">入力データ。</param>
        /// <returns>文字列。</returns>
        public abstract string GetStringValue(object value);
    }
}