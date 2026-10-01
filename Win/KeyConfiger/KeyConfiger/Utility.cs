using PPDConfiguration;
using PPDFramework;

namespace KeyConfiger
{
    class Utility
    {
        private static LanguageReader langReader = new("KeyConfiger", PPDSetting.Setting.LangISO);

        public static LanguageReader Language
        {
            get
            {
                return langReader;
            }
        }

        public static void ChangeLanguage(string langIso)
        {
            langReader = new LanguageReader("KeyConfiger", langIso);
        }
    }
}
