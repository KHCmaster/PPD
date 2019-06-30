using System.IO;
using System.Reflection;

namespace PPDMovie
{
    class Utility
    {
        public static string AppDir
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }
    }
}
