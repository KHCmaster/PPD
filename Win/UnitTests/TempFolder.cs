using System;
using System.IO;

namespace UnitTests
{
    /// <summary>
    /// テスト用の一時フォルダです。終わったら消します。今のフォルダを変えることもできます（今のフォルダのファイルを読み書きするコードのため）。
    /// </summary>
    /// <remarks>
    /// 今のフォルダはプロセス全体で 1 つなので、テストは並べて実行しない（xunit.runner.json）。
    /// </remarks>
    public sealed class TempFolder : IDisposable
    {
        readonly string previousDirectory;

        public string Path { get; }

        /// <param name="setCurrentDirectory">今のフォルダを、この一時フォルダにするかどうか。</param>
        public TempFolder(bool setCurrentDirectory = false)
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "PPDUnitTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
            if (setCurrentDirectory)
            {
                previousDirectory = Environment.CurrentDirectory;
                Environment.CurrentDirectory = Path;
            }
        }

        /// <summary>
        /// 一時フォルダの中のパスを返します。
        /// </summary>
        public string Combine(params string[] names)
        {
            return System.IO.Path.Combine([Path, .. names]);
        }

        /// <summary>
        /// 一時フォルダの中にファイルを書きます（フォルダがなければ作る）。
        /// </summary>
        public string Write(string name, string content)
        {
            var path = Combine(name);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            File.WriteAllText(path, content);
            return path;
        }

        public void Dispose()
        {
            if (previousDirectory != null)
            {
                Environment.CurrentDirectory = previousDirectory;
            }
            try
            {
                Directory.Delete(Path, true);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
