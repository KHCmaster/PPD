using PPDConfiguration;

namespace PPDFramework
{
    /// <summary>
    /// 言語読み取りクラスです。
    /// </summary>
    public class ExLanguageReader : LanguageReader
    {
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="componentName"></param>
        public ExLanguageReader(string componentName)
            : base(
                componentName, PPDSetting.Setting.LangISO)
        {
        }
    }
}
