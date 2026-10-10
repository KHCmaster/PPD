using PPDFramework;

namespace PPDMulti
{
    class Utility
    {
        private static readonly ExLanguageReader langReader = new("PPDMulti");
        private static readonly PathManager pathManager = new(@"img\PPD\multi");
        private static readonly PathManager mainGamePathManager = new(@"img\PPD\main_game");

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
