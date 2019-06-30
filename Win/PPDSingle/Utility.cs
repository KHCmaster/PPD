using ICSharpCode.SharpZipLib.Zip;
using PPDFramework;
using System.IO;
using System.Text.RegularExpressions;

namespace PPDSingle
{
    class Utility
    {
        private static ExLanguageReader langReader = new ExLanguageReader("PPDSingle");
        private static PathManager pathManager = new PathManager(@"img\PPD\single");

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
            if (destDirName[destDirName.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                destDirName = destDirName + System.IO.Path.DirectorySeparatorChar;

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
