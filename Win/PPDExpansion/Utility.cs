using PPDFramework;

namespace PPDExpansion
{
    class Utility
    {
        private static readonly ExLanguageReader langReader = new("PPDExpansion");

        public static ExLanguageReader Language
        {
            get
            {
                return langReader;
            }
        }
    }
}
