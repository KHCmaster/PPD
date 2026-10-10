using PPDConfiguration;

namespace PPDFramework
{
    /// <summary>
    /// 言語読み取りクラスです。
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="componentName"></param>
    public class ExLanguageReader(string componentName) : LanguageReader(
            componentName, PPDSetting.Setting.LangISO)
    {
    }
}
