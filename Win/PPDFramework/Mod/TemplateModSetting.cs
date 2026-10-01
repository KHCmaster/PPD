namespace PPDFramework.Mod
{
    /// <summary>
    /// 既定値を持つ設定を扱うクラスです。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="key"></param>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="defaultValue"></param>
    public abstract class TemplateModSetting<T>(string key, string name, string description, T defaultValue) : ModSetting(key, name, description)
    {
        /// <summary>
        /// 既定値を取得します。
        /// </summary>
        public T DefaultValue
        {
            get;
            private set;
        } = defaultValue;

        /// <summary>
        /// 既定値を取得します。
        /// </summary>
        public override object Default
        {
            get { return DefaultValue; }
        }
    }
}
