using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PPDUpdater
{
    static partial class Utility
    {
        static Utility()
        {
            // UTF-8 フラグのないファイル名は、従来（SharpZipLib 0.86）と同じく OEM コードページで読む
            FastZip = new FastZip
            {
                RestoreAttributesOnExtract = true,
                RestoreDateTimeOnExtract = true,
                CreateEmptyDirectories = true,
                StringCodec = StringCodec.FromCodePage(System.Globalization.CultureInfo.CurrentCulture.TextInfo.OEMCodePage)
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
        public static Regex UpdateRegex = UpdateFileRegex();
        public static string UpdateRegexGroup = "version";


        /// <summary>
        /// PPDUpdater のあるフォルダー（PPD のフォルダー）から、インストール先を決めます。
        /// 一時フォルダーから動き直したときに、元のフォルダーを渡す。
        /// </summary>
        public static void SetApplicationDirectory(string dir)
        {
            KHCDir = Directory.GetParent(Path.GetFullPath(dir).TrimEnd(Path.DirectorySeparatorChar)).FullName;
        }

        public static string GetRelativePath(string path, int level)
        {
            while (level > 0)
            {
                var index = path.IndexOf(Path.DirectorySeparatorChar);
                if (index < 0) return "";
                path = path[(index + 1)..];
                level--;
            }
            return path;
        }

        [GeneratedRegex("^PPDDiff(?<version>\\d+)", RegexOptions.IgnoreCase)]
        private static partial Regex UpdateFileRegex();
    }
}
