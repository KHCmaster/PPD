using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PPDUpdater
{
    static class Utility
    {
        static Utility()
        {
            FastZip = new FastZip
            {
                RestoreAttributesOnExtract = true,
                RestoreDateTimeOnExtract = true,
                CreateEmptyDirectories = true
            };

            string location = Assembly.GetExecutingAssembly().Location;
            KHCDir = Directory.GetParent(Directory.GetParent(location).FullName).FullName;
        }
        public static FastZip FastZip;
        public static string ZipFilter = "";
        public static string DownloadDirectory = "Update";
        public static string DataDir = "Data";
        public static string LangDir = "Lang";
        public static string KHCDir;
        public static string Complete = "Complete";
        public static Regex UpdateRegex = new Regex("^PPDDiff(?<version>\\d+)", RegexOptions.IgnoreCase);
        public static string UpdateRegexGroup = "version";


        public static string GetRelativePath(string path, int level)
        {
            while (level > 0)
            {
                var index = path.IndexOf(Path.DirectorySeparatorChar);
                if (index < 0) return "";
                path = path.Substring(index + 1);
                level--;
            }
            return path;
        }
    }
}
