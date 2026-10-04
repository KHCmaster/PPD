using System;
using System.IO;

namespace UnitTests
{
    /// <summary>
    /// アプリが今のフォルダから読むファイル（言語ファイルなど）を、一時フォルダにコピーします。
    /// </summary>
    /// <remarks>
    /// ファイルは、テストの出力のフォルダの Apps\アプリの名前 にある（UnitTests.csproj でコピーしている）。
    /// </remarks>
    public static class AppFiles
    {
        /// <summary>
        /// アプリのファイルを一時フォルダにコピーします。
        /// </summary>
        public static void CopyTo(string appName, TempFolder folder)
        {
            CopyDirectory(Path.Combine(AppContext.BaseDirectory, "Apps", appName), folder.Path);
        }

        private static void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (var file in Directory.GetFiles(source))
            {
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
            }
            foreach (var directory in Directory.GetDirectories(source))
            {
                CopyDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)));
            }
        }
    }
}
