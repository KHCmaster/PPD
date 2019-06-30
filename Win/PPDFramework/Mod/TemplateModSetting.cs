namespace PPDFramework.Mod
{
    /// <summary>
    /// 既定値を持つ設定を扱うクラスです。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TemplateModSetting<T> : ModSetting
    {
        /// <summary>
        /// 既定値を取得します。
        /// </summary>
        public T DefaultValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 既定値を取得します。
        /// </summary>
        public override object Default
        {
            get { return DefaultValue; }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        protected TemplateModSetting(string key, string name, string description, T defaultValue)
            : base(key, name, description)
        {
            DefaultValue = defaultValue;
        }
    }
}
