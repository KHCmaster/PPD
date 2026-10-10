using PPDFramework;

namespace PPD
{
    public static class Utility
    {
        private static readonly ExLanguageReader langReader = new("PPD");
        private static readonly PathManager pathManager = new(@"img\PPD\home");

        public static ExLanguageReader Language
        {
            get
            {
                return langReader;
            }
        }

        public static PathManager Path
        {
            get
            {
                return pathManager;
            }
        }
    }
}
