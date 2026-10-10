using PPDShared;
using System.IO;
using System.Linq;
using Xunit;

namespace UnitTests.PPDReleaser
{
    /// <summary>
    /// リリースで配るファイルの一覧と、PPDReleaser の、足りないファイルの確認のテストです。
    /// </summary>
    public class ReleaseFilesTests
    {
        [Fact]
        public void Lists_HaveEachApplicationOnceAndNoDuplicates()
        {
            var versioned = ReleaseFiles.VersionedFiles;
            var others = ReleaseFiles.OtherFiles;

            Assert.Equal(versioned.Length, versioned.Distinct().Count());
            Assert.Equal(others.Length, others.Distinct().Count());
            Assert.Empty(versioned.Intersect(others));
            foreach (var application in ReleaseFiles.Applications)
            {
                Assert.Contains(application + ".exe", versioned);
                Assert.Contains(application + ".dll", versioned);
                Assert.Contains(application + ".runtimeconfig.json", others);
                Assert.Contains(application + ".deps.json", others);
            }
        }

        private const string Deps = """
            {
              "targets": {
                ".NETCoreApp,Version=v10.0": {
                  "App/1.0.0": {
                    "dependencies": { "Lib": "1.0.0" },
                    "runtime": { "App.dll": {} }
                  },
                  "Lib/1.0.0": {
                    "runtime": { "lib/net8.0/Lib.dll": {} },
                    "runtimeTargets": {
                      "runtimes/win-x64/native/Native.dll": { "rid": "win-x64", "assetType": "native" },
                      "runtimes/linux-x64/native/Native.dll": { "rid": "linux-x64", "assetType": "native" }
                    }
                  },
                  "Project/1.0.0": {
                    "runtime": { "Project.dll": {} }
                  }
                }
              },
              "libraries": {
                "App/1.0.0": { "type": "project" },
                "Lib/1.0.0": { "type": "package" },
                "Project/1.0.0": { "type": "project" }
              }
            }
            """;

        [Fact]
        public void CheckDependencies_ReportsMissingFilesForWindowsX64()
        {
            using var folder = new TempFolder();
            folder.Write("App.deps.json", Deps);
            folder.Write("App.dll", "");

            // パッケージの dll はアプリのフォルダーの直下、Windows x64 のネイティブの dll は runtimes の下にあればよい（ほかの OS のものは要らない）
            var missing = global::PPDReleaser.Program.CheckDependencies(folder.Path);
            string[] expected = ["App.deps.json: lib/net8.0/Lib.dll", "App.deps.json: Project.dll", "App.deps.json: runtimes/win-x64/native/Native.dll"];
            Assert.Equal(expected.Order(), missing.Order());

            folder.Write("Lib.dll", "");
            folder.Write("Project.dll", "");
            folder.Write(Path.Combine("runtimes", "win-x64", "native", "Native.dll"), "");
            Assert.Empty(global::PPDReleaser.Program.CheckDependencies(folder.Path));
        }
    }
}
