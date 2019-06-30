using PPDFramework;

namespace PPDExpansion
{
    class Utility
    {
        private static ExLanguageReader langReader = new ExLanguageReader("PPDExpansion");

        public static ExLanguageReader Language
        {
            get
            {
                return langReader;
            }
        }
    }
}
