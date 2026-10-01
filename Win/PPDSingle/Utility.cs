using ICSharpCode.SharpZipLib.Zip;
using PPDFramework;
using System.IO;
using System.Text.RegularExpressions;

namespace PPDSingle
{
    class Utility
    {
        private static readonly ExLanguageReader langReader = new("PPDSingle");
        private static readonly PathManager pathManager = new(@"img\PPD\single");

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

        public static FastZip FastZip
        {
            get;
            private set;
        }

        static Utility()
        {
            // UTF-8 フラグのないファイル名は、従来（SharpZipLib 0.86）と同じく OEM コードページで読む
            ZipStrings.CodePage = System.Globalization.CultureInfo.CurrentCulture.TextInfo.OEMCodePage;
            FastZip = new FastZip
            {
                RestoreAttributesOnExtract = true,
                RestoreDateTimeOnExtract = true,
                CreateEmptyDirectories = true
            };
        }

        public static void CopyDirectory(string sourceDirName, string destDirName, Regex[] exceptFiles)
        {
            //コピー先のディレクトリがないときは作る
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            //コピー先のディレクトリ名の末尾に"\"をつける
            if (destDirName[^1] != System.IO.Path.DirectorySeparatorChar)
                destDirName += System.IO.Path.DirectorySeparatorChar;

            //コピー元のディレクトリにあるファイルをコピー
            var files = Directory.GetFiles(sourceDirName);
            foreach (string file in files)
            {
                var filename = System.IO.Path.GetFileName(file);
                bool ok = true;
                foreach (Regex regex in exceptFiles)
                {
                    if (regex.Match(filename).Success)
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                {
                    File.Copy(file, destDirName + filename, true);
                }
            }
            //コピー元のディレクトリにあるディレクトリについて、
            //再帰的に呼び出す
            var dirs = Directory.GetDirectories(sourceDirName);
            foreach (string dir in dirs)
                CopyDirectory(dir, destDirName + System.IO.Path.GetFileName(dir), exceptFiles);
        }
    }
}
