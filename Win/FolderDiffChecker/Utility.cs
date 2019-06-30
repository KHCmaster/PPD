using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Text.RegularExpressions;

namespace FolderDiffChecker
{
    class Utility
    {
        public static void CopyDirectory(string sourceDirName, string destDirName)
        {
            CopyDirectory(sourceDirName, destDirName, new Regex[0]);
        }

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
            {
                var dirName = Path.GetFileName(dir);
                bool ok = true;
                foreach (Regex regex in exceptFiles)
                {
                    if (regex.Match(dirName).Success)
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                {
                    CopyDirectory(dir, destDirName + Path.GetFileName(dir), exceptFiles);
                }
            }
        }

        public static void Zip(string fileName, string sourceDir)
        {
            Zip(fileName, sourceDir, new Regex[0]);
        }

        public static void Zip(string fileName, string sourceDir, Regex[] exceptFiles)
        {
            var tempFileName = Path.GetTempFileName();
            var tempDir = Path.Combine(Path.GetDirectoryName(tempFileName), Path.GetFileNameWithoutExtension(tempFileName));
            var dirName = Path.GetFileName(sourceDir);
            Directory.CreateDirectory(tempDir);
            var targetDir = Path.Combine(tempDir, dirName);
            Directory.CreateDirectory(targetDir);
            Utility.CopyDirectory(sourceDir, targetDir, exceptFiles);

            var fastZip = new FastZip
            {
                CreateEmptyDirectories = true
            };
            fastZip.CreateZip(fileName, tempDir, true, null);
        }
    }
}
