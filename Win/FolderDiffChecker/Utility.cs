using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Text.RegularExpressions;

namespace FolderDiffChecker
{
    class Utility
    {
        public static void CopyDirectory(string sourceDirName, string destDirName)
        {
            CopyDirectory(sourceDirName, destDirName, []);
        }

        public static void CopyDirectory(string sourceDirName, string destDirName, Regex[] exceptFiles)
        {
            //コピー先のディレクトリがないときは作る
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            //コピー先のディレクトリ名の末尾に"\"をつける
            if (destDirName[^1] != Path.DirectorySeparatorChar)
                destDirName += Path.DirectorySeparatorChar;

            //コピー元のディレクトリにあるファイルをコピー
            var files = Directory.GetFiles(sourceDirName);
            foreach (string file in files)
            {
                var filename = Path.GetFileName(file);
                bool ok = true;
                foreach (Regex regex in exceptFiles)
                {
                    if (regex.IsMatch(filename))
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
                    if (regex.IsMatch(dirName))
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
            Zip(fileName, sourceDir, []);
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

            // ファイル名は、従来（SharpZipLib 0.86）と同じく OEM コードページで書く（古い PPDUpdater でも読めるように）
            ZipStrings.CodePage = System.Globalization.CultureInfo.CurrentCulture.TextInfo.OEMCodePage;
            var fastZip = new FastZip
            {
                CreateEmptyDirectories = true
            };
            fastZip.CreateZip(fileName, tempDir, true, null);
        }
    }
}
