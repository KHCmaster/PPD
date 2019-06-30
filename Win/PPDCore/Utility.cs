using PPDFramework;

namespace PPDCore
{
    class Utility
    {
        private static PathManager pathManager = new PathManager(@"img\PPD\main_game");

        public static PathManager Path
        {
            get
            {
                return pathManager;
            }
        }
    }
}
