using PPDFramework;

namespace PPDCore
{
    class Utility
    {
        private static readonly PathManager pathManager = new(@"img\PPD\main_game");

        public static PathManager Path
        {
            get
            {
                return pathManager;
            }
        }
    }
}
