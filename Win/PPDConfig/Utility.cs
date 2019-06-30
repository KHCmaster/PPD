using PPDConfiguration;
using PPDFramework;

namespace PPDConfig
{
    class Utility
    {
        private static LanguageReader langReader = new ExLanguageReader("PPDConfig");

        public static LanguageReader Language
        {
            get
            {
                return langReader;
            }
        }

        public static void ChangeLanguage(string langIso)
        {
            langReader = new LanguageReader("PPDConfig", langIso);
        }
    }
}
