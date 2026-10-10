using PPDUpdater;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace UnitTests.PPDUpdater
{
    /// <summary>
    /// PPDUpdater を一時フォルダーにコピーして動き直すときの、コピーするファイルや引数のテストです（実際には起動しない）。
    /// </summary>
    public class SelfRelauncherTests
    {
        [Fact]
        public void GetFromDirectory_ReadsTheFolderAfterTheArgument()
        {
            Assert.Equal(@"F:\KHC_x64\PPD\", SelfRelauncher.GetFromDirectory(["-i", "--from", @"F:\KHC_x64\PPD\"]));
            Assert.Null(SelfRelauncher.GetFromDirectory(["-i"]));
            Assert.Null(SelfRelauncher.GetFromDirectory(["--from"]));
        }

        [Fact]
        public void SetApplicationDirectory_InstallsIntoTheParentOfThePPDFolder()
        {
            var saved = Utility.KHCDir;
            try
            {
                Utility.SetApplicationDirectory(@"F:\KHC_x64\PPD\");
                Assert.Equal(@"F:\KHC_x64", Utility.KHCDir);
                Utility.SetApplicationDirectory(@"F:\KHC_x64\PPD");
                Assert.Equal(@"F:\KHC_x64", Utility.KHCDir);
            }
            finally
            {
                Utility.KHCDir = saved;
            }
        }

        private const string Deps = """
            {
              "targets": {
                ".NETCoreApp,Version=v10.0": {
                  "App/1.0.0": { "runtime": { "App.dll": {} } },
                  "Lib/1.0.0": {
                    "runtime": { "lib/net8.0/Lib.dll": {} },
                    "runtimeTargets": {
                      "runtimes/win-x64/native/Native.dll": { "rid": "win-x64", "assetType": "native" },
                      "runtimes/linux-x64/native/Native.dll": { "rid": "linux-x64", "assetType": "native" }
                    }
                  }
                }
              },
              "libraries": {}
            }
            """;

        [Fact]
        public void GetFilesToCopy_TakesTheAppAndItsDependenciesOnly()
        {
            using var folder = new TempFolder();
            foreach (var name in new[] { "App.exe", "App.dll", "App.runtimeconfig.json", "Lib.dll", "Other.dll", "App.ini" })
            {
                folder.Write(name, "");
            }
            folder.Write("App.deps.json", Deps);
            folder.Write(Path.Combine("runtimes", "win-x64", "native", "Native.dll"), "");
            folder.Write(Path.Combine("runtimes", "linux-x64", "native", "Native.dll"), "");

            var files = SelfRelauncher.GetFilesToCopy(folder.Path, "App");

            // 設定（App.ini）や、使わない dll、ほかの OS の dll はコピーしない（設定は今のフォルダーから読む）
            string[] expected = ["App.exe", "App.dll", "App.runtimeconfig.json", "App.deps.json", "Lib.dll", Path.Combine("runtimes", "win-x64", "native", "Native.dll")];
            Assert.Equal(expected.Order(), files.Order());
        }

        [Fact]
        public void GetFilesToCopy_ForTheBuiltUpdater_HasEveryDependency()
        {
            // ビルドした PPDUpdater で、コピーしたものだけで動けるか（deps.json に書かれた dll がすべてあるか）を確かめる
            var dir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "PPDUpdater", "bin", "x64", "Debug"));
            Assert.True(File.Exists(Path.Combine(dir, "PPDUpdater.deps.json")), dir);
            using var folder = new TempFolder();
            foreach (var file in SelfRelauncher.GetFilesToCopy(dir, "PPDUpdater"))
            {
                var target = Path.Combine(folder.Path, file);
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                File.Copy(Path.Combine(dir, file), target);
            }

            Assert.True(File.Exists(Path.Combine(folder.Path, "PPDUpdater.exe")));
            Assert.Empty(global::PPDReleaser.Program.CheckDependencies(folder.Path));
        }

        [Fact]
        public void DeleteOldCopies_KeepsTheRunningCopy()
        {
            using var folder = new TempFolder();
            var running = folder.Combine("running");
            folder.Write(Path.Combine("running", "PPDUpdater.dll"), "");
            folder.Write(Path.Combine("old1", "PPDUpdater.dll"), "");
            folder.Write(Path.Combine("old2", "sub", "a.dll"), "");

            SelfRelauncher.DeleteOldCopies(folder.Path, running + Path.DirectorySeparatorChar);

            Assert.Equal([running], Directory.GetDirectories(folder.Path));
            SelfRelauncher.DeleteOldCopies(folder.Combine("none"), running);
        }
    }
}
