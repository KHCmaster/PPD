using PPDFramework;

namespace PPD
{
    public static class Utility
    {
        private static ExLanguageReader langReader = new ExLanguageReader("PPD");
        private static PathManager pathManager = new PathManager(@"img\PPD\home");

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
