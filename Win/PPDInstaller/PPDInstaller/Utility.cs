using System.IO;
using System.Text.RegularExpressions;

namespace PPDInstaller
{
    class Utility
    {
        public static void CopyDirectory(string sourceDirName, string destDirName, Regex[] exceptFiles)
        {
            //コピー先のディレクトリがないときは作る
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            //コピー先のディレクトリ名の末尾に"\"をつける
            if (destDirName[destDirName.Length - 1] != Path.DirectorySeparatorChar)
                destDirName = destDirName + Path.DirectorySeparatorChar;

            //コピー元のディレクトリにあるファイルをコピー
            var files = Directory.GetFiles(sourceDirName);
            foreach (string file in files)
            {
                var filename = Path.GetFileName(file);
                bool ok = true;
                foreach (Regex regex in exceptFiles)
                {
                    if (regex.Match(filename).Success && File.Exists(destDirName + filename))
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
                CopyDirectory(dir, destDirName + Path.GetFileName(dir), exceptFiles);
        }

        public static void Unzip(string path, string unzippath)
        {
            string fileFilter = "";
            var fastZip =
                new ICSharpCode.SharpZipLib.Zip.FastZip
                {
                    RestoreAttributesOnExtract = true,
                    RestoreDateTimeOnExtract = true,
                    CreateEmptyDirectories = true
                };
            fastZip.ExtractZip(path, unzippath, fileFilter);
        }
    }
}
