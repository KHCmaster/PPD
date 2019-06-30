using PPDFramework;

namespace PPDMulti
{
    class Utility
    {
        private static ExLanguageReader langReader = new ExLanguageReader("PPDMulti");
        private static PathManager pathManager = new PathManager(@"img\PPD\multi");
        private static PathManager mainGamePathManager = new PathManager(@"img\PPD\main_game");

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

        public static PathManager MainGamePath
        {
            get
            {
                return mainGamePathManager;
            }
        }

        public static bool IsSameArray(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
