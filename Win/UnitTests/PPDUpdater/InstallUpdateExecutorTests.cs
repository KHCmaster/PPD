using PPDUpdater;
using PPDUpdater.Executor;
using PPDUpdater.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Xunit;

namespace UnitTests.PPDUpdater
{
    /// <summary>
    /// ダウンロードした差分（zip）を展開して、インストール先に写す処理のテストです。
    /// </summary>
    /// <remarks>
    /// インストール先（Utility.KHCDir）は一時フォルダーにする。zip は今のフォルダー（一時フォルダー）からの相対パスに置く（実際と同じ）。
    /// </remarks>
    public sealed class InstallUpdateExecutorTests : IDisposable
    {
        readonly TempFolder folder = new(setCurrentDirectory: true);
        readonly string savedKhcDir = Utility.KHCDir;
        readonly string khcDir;

        public InstallUpdateExecutorTests()
        {
            khcDir = folder.Combine("KHC");
            Utility.KHCDir = khcDir;
        }

        public void Dispose()
        {
            Utility.KHCDir = savedKhcDir;
            folder.Dispose();
        }

        // Update\{版}\PPDDiff{版}.zip を、Data\{アプリ}\ の下にファイルを入れて作る
        string WriteDiff(int version)
        {
            var content = folder.Combine("content");
            folder.Write(Path.Combine("content", "Data", "PPD", "readme.txt"), "ppd");
            folder.Write(Path.Combine("content", "Data", "PPD", "sub", "a.txt"), "sub");
            folder.Write(Path.Combine("content", "Data", "BMSTOPPD", "b.txt"), "bms");
            folder.Write(Path.Combine("content", "Data", "Effect2DEditor", "e.txt"), "effect");
            File.Copy(typeof(PPDMath.MathCompat).Assembly.Location, Path.Combine(content, "Data", "PPD", "PPD.exe"));
            var zip = Path.Combine("Update", version.ToString(), $"PPDDiff{version}.zip");
            Directory.CreateDirectory(Path.GetDirectoryName(zip));
            ZipFile.CreateFromDirectory(content, zip);
            Directory.Delete(content, true);
            return zip;
        }

        [Fact]
        public void Execute_CopiesSelectedApplications()
        {
            var zip = WriteDiff(5);
            var info = new InstallInfo { PPD = true, BMSTOPPD = false, Effect2DEditor = true };
            var executor = new InstallUpdateExecutor(zip, new VersionInfo("5"), info, 0, new SynchronizationContext());
            var progress = 0;
            var finished = 0;
            executor.Progressed += (s, e) => progress = executor.Progress;
            executor.Finished += (s, e) => finished++;

            executor.Execute();

            Assert.True(executor.Success, executor.ErrorLog);
            Assert.Equal((1, 100), (finished, progress));
            Assert.Equal("ppd", File.ReadAllText(Path.Combine(khcDir, "PPD", "readme.txt")));
            Assert.Equal("sub", File.ReadAllText(Path.Combine(khcDir, "PPD", "sub", "a.txt")));
            Assert.Equal("effect", File.ReadAllText(Path.Combine(khcDir, "Effect2DEditor", "e.txt")));
            // 選ばなかったアプリは写さない
            Assert.False(Directory.Exists(Path.Combine(khcDir, "BMSTOPPD")));
            // 写した PPD.exe の版を覚える。ダウンロードしたものは消す
            Assert.Equal(new VersionInfo(FileVersionInfo.GetVersionInfo(typeof(PPDMath.MathCompat).Assembly.Location).FileVersion).ToString(), info.PPDVersion.ToString());
            Assert.False(Directory.Exists(Path.Combine("Update", "5")));
        }

        [Fact]
        public void Execute_ReportsErrors()
        {
            Directory.CreateDirectory(Path.Combine("Update", "6"));
            var executor = new InstallUpdateExecutor(Path.Combine("Update", "6", "PPDDiff6.zip"), new VersionInfo("6"), new InstallInfo(), 0, new SynchronizationContext());

            executor.Execute();

            Assert.False(executor.Success);
            Assert.False(string.IsNullOrEmpty(executor.ErrorLog));
            Assert.False(Directory.Exists(khcDir));
        }
    }
}
