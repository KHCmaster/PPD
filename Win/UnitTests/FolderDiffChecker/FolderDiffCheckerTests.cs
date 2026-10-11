using FolderDiffChecker;
using FolderDiffChecker.Models;
using FolderDiffChecker.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using Xunit;

namespace UnitTests.FolderDiffChecker
{
    /// <summary>
    /// 比べるフォルダを作ります。
    /// </summary>
    static class DiffFolders
    {
        public static (string Old, string New) Create(TempFolder folder)
        {
            folder.Write(Path.Combine("old", "same.txt"), "same");
            folder.Write(Path.Combine("new", "same.txt"), "same");
            folder.Write(Path.Combine("old", "edited.txt"), "before");
            folder.Write(Path.Combine("new", "edited.txt"), "after!");
            folder.Write(Path.Combine("old", "longer.txt"), "a");
            folder.Write(Path.Combine("new", "longer.txt"), "ab");
            folder.Write(Path.Combine("new", "added.txt"), "added");
            folder.Write(Path.Combine("new", "Thumbs.db"), "ignored");
            folder.Write(Path.Combine("old", "sub", "keep.txt"), "keep");
            folder.Write(Path.Combine("new", "sub", "keep.txt"), "keep");
            folder.Write(Path.Combine("new", "newdir", "inner", "deep.txt"), "deep");
            return (folder.Combine("old"), folder.Combine("new"));
        }
    }

    public class DiffFinderTests
    {
        private static List<DiffModel> Find(string oldFolder, string newFolder, Regex[] except, Regex[] force, List<string> compared = null)
        {
            var found = new List<DiffModel>();
            var finished = false;
            var finder = new DiffFinder(oldFolder, newFolder, except, force, new SynchronizationContext());
            finder.DiffFound += found.Add;
            finder.Finished += (sender, e) => finished = true;
            finder.BeforeCompare += text => compared?.Add(text);

            finder.Execute();

            Assert.True(finished);
            return found;
        }

        [Fact]
        public void FindsAddedEditedAndNewFolders()
        {
            using var folder = new TempFolder();
            var (oldFolder, newFolder) = DiffFolders.Create(folder);

            var found = Find(oldFolder, newFolder, [new Regex("thumbs\\.db", RegexOptions.IgnoreCase)], []);

            var byName = found.ToDictionary(d => Path.GetRelativePath(newFolder, d.FilePath));
            Assert.Equal(DiffModel.DiffMode.Edit, byName["edited.txt"].Mode);
            Assert.Equal(DiffModel.DiffMode.Edit, byName["longer.txt"].Mode);
            Assert.Equal(DiffModel.DiffMode.Add, byName["added.txt"].Mode);
            Assert.False(byName["newdir"].IsFile);
            Assert.False(byName[Path.Combine("newdir", "inner")].IsFile);
            Assert.True(byName[Path.Combine("newdir", "inner", "deep.txt")].IsFile);
            Assert.DoesNotContain("same.txt", byName.Keys);
            Assert.DoesNotContain("Thumbs.db", byName.Keys);
            Assert.DoesNotContain(Path.Combine("sub", "keep.txt"), byName.Keys);
        }

        [Fact]
        public void ForceIncludedFileIsEditedEvenIfSame()
        {
            using var folder = new TempFolder();
            var (oldFolder, newFolder) = DiffFolders.Create(folder);

            var found = Find(oldFolder, newFolder, [], [new Regex("same\\.txt")]);

            Assert.Contains(found, d => Path.GetFileName(d.FilePath) == "same.txt" && d.Mode == DiffModel.DiffMode.Edit);
        }

        [Fact]
        public void SameFolder_FindsNothingAndReportsComparing()
        {
            using var folder = new TempFolder();
            var (_, newFolder) = DiffFolders.Create(folder);
            var compared = new List<string>();

            var found = Find(newFolder, newFolder, [], [], compared);

            Assert.Empty(found);
            Assert.Contains(compared, text => text.EndsWith("same.txt"));
            Assert.Contains("", compared);
        }

        [Fact]
        public void ExeAndDll_AreComparedByVersion()
        {
            using var folder = new TempFolder();
            // 版の情報のない exe や dll は同じ版とみなす（中身が違っても差分にしない）
            folder.Write(Path.Combine("old", "a.dll"), "one");
            folder.Write(Path.Combine("new", "a.dll"), "two");
            folder.Write(Path.Combine("old", "b.exe"), "one");
            folder.Write(Path.Combine("new", "b.exe"), "two-longer");

            var found = Find(folder.Combine("old"), folder.Combine("new"), [], []);

            Assert.Empty(found);
        }
    }

    public class ReleaseFolderTests
    {
        [Fact]
        public void FindLatestPair_SortsVersionsNumerically()
        {
            using var folder = new TempFolder();
            foreach (var version in new[] { "1.0.0.9", "1.0.0.10", "1.0.0.2", "notes" })
            {
                Directory.CreateDirectory(folder.Combine(version));
            }

            var pair = ReleaseFolder.FindLatestPair(folder.Path);

            Assert.Equal(folder.Combine("1.0.0.9", "PPDInstaller(x64)"), pair.Value.OldFolder);
            Assert.Equal(folder.Combine("1.0.0.10", "PPDInstaller(x64)"), pair.Value.NewFolder);
        }

        [Fact]
        public void FindLatestPair_NeedsTwoVersions()
        {
            using var folder = new TempFolder();
            Directory.CreateDirectory(folder.Combine("1.0.0.1"));

            Assert.Null(ReleaseFolder.FindLatestPair(folder.Path));
            Assert.Null(ReleaseFolder.FindLatestPair(folder.Combine("missing")));
        }

        [Fact]
        public void GetVersion_FromInstallerFolder()
        {
            Assert.Equal("1.2.3.4", ReleaseFolder.GetVersion(@"C:\releases\1.2.3.4\PPDInstaller(x64)", "."));
            Assert.Equal("1234", ReleaseFolder.GetVersion(@"C:\releases\1.2.3.4\PPDInstaller(x64)", ""));
            Assert.Null(ReleaseFolder.GetVersion(@"C:\releases\latest\PPDInstaller(x64)", "."));
        }

        [Fact]
        public void CopyDiff_KeepsRelativePaths()
        {
            using var folder = new TempFolder();
            var (oldFolder, newFolder) = DiffFolders.Create(folder);
            var diffs = new List<DiffModel>
            {
                new() { FilePath = Path.Combine(newFolder, "added.txt"), IsFile = true },
                new() { FilePath = Path.Combine(newFolder, "newdir"), IsFile = false },
                new() { FilePath = Path.Combine(newFolder, "newdir", "inner", "deep.txt"), IsFile = true },
            };

            ReleaseFolder.CopyDiff(newFolder, diffs, folder.Combine("out"));

            Assert.Equal("added", File.ReadAllText(folder.Combine("out", "added.txt")));
            Assert.Equal("deep", File.ReadAllText(folder.Combine("out", "newdir", "inner", "deep.txt")));
        }

        [Fact]
        public void CopyDiffToVersionFolder_UsesVersionAndBits()
        {
            using var folder = new TempFolder();
            var newFolder = folder.Combine("1.2.3.4", "PPDInstaller(x64)");
            folder.Write(Path.Combine("1.2.3.4", "PPDInstaller(x64)", "a.txt"), "a");

            var copied = ReleaseFolder.CopyDiffToVersionFolder(newFolder, [new DiffModel { FilePath = Path.Combine(newFolder, "a.txt"), IsFile = true }], folder.Path);

            Assert.True(copied);
            Assert.True(File.Exists(folder.Combine("PPDDiff1234_x64", "a.txt")));
            Assert.False(ReleaseFolder.CopyDiffToVersionFolder(folder.Combine("other"), [], folder.Path));
        }

        private static List<string> ZipEntries(string path)
        {
            using var zip = ZipFile.OpenRead(path);
            return [.. zip.Entries.Select(e => e.FullName.Replace('\\', '/'))];
        }

        [Fact]
        public void ZipAll_MakesDataPakInstallerAndDiffZips()
        {
            using var folder = new TempFolder();
            var newFolder = folder.Combine("releases", "1.0.0.1", "PPDInstaller(x64)");
            folder.Write(Path.Combine("releases", "1.0.0.1", "PPDInstaller(x64)", "Data", "a.txt"), "a");
            folder.Write(Path.Combine("releases", "1.0.0.1", "PPDInstaller(x64)", "setup.txt"), "setup");
            folder.Write(Path.Combine("releases", "PPDDiff1001_x64", "Data", "b.txt"), "b");

            Assert.True(ReleaseFolder.ZipAll(newFolder));

            Assert.Contains("Data/a.txt", ZipEntries(Path.Combine(newFolder, "Data.pak")));
            var installer = ZipEntries(folder.Combine("releases", "1.0.0.1", "PPDInstaller1001(x64).zip"));
            Assert.Contains("PPDInstaller(x64)/setup.txt", installer);
            Assert.Contains("PPDInstaller(x64)/Data.pak", installer);
            // Data フォルダーは Data.pak に入れたので、インストーラーの zip には入れない
            Assert.DoesNotContain(installer, e => e.StartsWith("PPDInstaller(x64)/Data/"));
            Assert.Contains("Data/b.txt", ZipEntries(folder.Combine("releases", "PPDDiff1001_x64", "PPDDiff1001_x64.zip")));
            Assert.False(ReleaseFolder.ZipAll(folder.Combine("other")));
        }

        [Fact]
        public void CopyDirectory_SkipsExceptedFilesAndFolders()
        {
            using var folder = new TempFolder();
            folder.Write(Path.Combine("src", "keep.txt"), "keep");
            folder.Write(Path.Combine("src", "skip.ini"), "skip");
            folder.Write(Path.Combine("src", "sub", "keep2.txt"), "keep");
            folder.Write(Path.Combine("src", "skipdir", "x.txt"), "x");

            Utility.CopyDirectory(folder.Combine("src"), folder.Combine("dst"), [new Regex("\\.ini$"), new Regex("^skipdir$")]);

            Assert.True(File.Exists(folder.Combine("dst", "keep.txt")));
            Assert.True(File.Exists(folder.Combine("dst", "sub", "keep2.txt")));
            Assert.False(File.Exists(folder.Combine("dst", "skip.ini")));
            Assert.False(Directory.Exists(folder.Combine("dst", "skipdir")));
        }

        [Fact]
        public void UpdateList_AddsNewVersionToBothLists()
        {
            using var folder = new TempFolder();
            var oldFolder = folder.Combine("1.0.0.0", "PPDInstaller(x64)");
            var newFolder = folder.Combine("1.0.0.1", "PPDInstaller(x64)");
            Directory.CreateDirectory(newFolder);
            foreach (var name in new[] { "updatelist.xml", "beta_updatelist.xml" })
            {
                folder.Write(Path.Combine("1.0.0.0", name), "<Updates><Update Version=\"1.0.0.0\" Path=\"old.zip\" AssemblyType=\"x64\" /></Updates>");
            }

            Assert.True(ReleaseFolder.UpdateList(oldFolder, newFolder));

            foreach (var name in new[] { "updatelist.xml", "beta_updatelist.xml" })
            {
                var updates = XDocument.Load(folder.Combine("1.0.0.1", name)).Root.Elements("Update").ToList();
                Assert.Equal(2, updates.Count);
                Assert.Equal("1.0.0.1", updates[1].Attribute("Version").Value);
                Assert.Equal("PPDDiff1001_x64.zip", updates[1].Attribute("Path").Value);
            }
            Assert.False(ReleaseFolder.UpdateList(oldFolder, folder.Combine("other")));
        }
    }

    public class FolderDiffCheckerViewModelTests : IDisposable
    {
        sealed class FakeDialogService : IDialogService
        {
            public Queue<string> Folders { get; } = new();
            public List<string> Messages { get; } = [];
            public int UserNameAsked { get; private set; }

            public string SelectFolder(string initialDirectory) => Folders.Count > 0 ? Folders.Dequeue() : null;

            public string AskUserName(string lastUserName)
            {
                UserNameAsked++;
                return null;
            }

            public void ShowMessage(string text) => Messages.Add(text);
        }

        readonly TempFolder folder = new(setCurrentDirectory: true);
        readonly FakeDialogService dialog = new();
        readonly SynchronizationContext previousContext = SynchronizationContext.Current;

        public FolderDiffCheckerViewModelTests()
        {
            // 差分を調べる処理は別のスレッドから結果を送るので、送られた処理をその場で実行するようにする
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        public void Dispose()
        {
            SynchronizationContext.SetSynchronizationContext(previousContext);
            folder.Dispose();
        }

        private static void WaitUntilChecked(MainWindowViewModel viewModel)
        {
            Assert.True(SpinWait.SpinUntil(() => !viewModel.IsChecking, TimeSpan.FromSeconds(10)));
        }

        [Fact]
        public void Startup_SelectsLatestTwoReleases()
        {
            Directory.CreateDirectory(folder.Combine("releases", "1.0.0.1"));
            Directory.CreateDirectory(folder.Combine("releases", "1.0.0.2"));

            var viewModel = new MainWindowViewModel(dialog, folder.Combine("releases"));

            Assert.Equal(folder.Combine("releases", "1.0.0.1", "PPDInstaller(x64)"), viewModel.OldFolder);
            Assert.Equal(folder.Combine("releases", "1.0.0.2", "PPDInstaller(x64)"), viewModel.NewFolder);
            Assert.Equal("PPD.ini:Data.pak", viewModel.IgnoreFiles);
        }

        [Fact]
        public void SelectFolders_SetsOrKeeps()
        {
            var viewModel = new MainWindowViewModel(dialog, folder.Path);
            dialog.Folders.Enqueue(@"C:\a");

            viewModel.SelectOldFolderCommand.Execute(null);
            viewModel.SelectNewFolderCommand.Execute(null);

            Assert.Equal(@"C:\a", viewModel.OldFolder);
            Assert.Equal("", viewModel.NewFolder);
        }

        [Fact]
        public void DiffCheck_MissingFolder_ShowsMessage()
        {
            var viewModel = new MainWindowViewModel(dialog, folder.Path) { OldFolder = folder.Combine("x"), NewFolder = folder.Combine("y") };

            viewModel.DiffCheckCommand.Execute(null);

            Assert.Equal(["存在しないディレクトリを比較しています。"], dialog.Messages);
        }

        [Fact]
        public void DiffCheck_ListsDiffsExceptIgnoredFiles()
        {
            var (oldFolder, newFolder) = DiffFolders.Create(folder);
            folder.Write(Path.Combine("new", "PPD.ini"), "secret");
            var viewModel = new MainWindowViewModel(dialog, folder.Path) { OldFolder = oldFolder, NewFolder = newFolder, ForceIncludeFiles = "same.txt" };

            viewModel.DiffCheckCommand.Execute(null);
            WaitUntilChecked(viewModel);

            var names = viewModel.Rows.Select(r => r.Name).ToList();
            Assert.Contains("added.txt", names);
            Assert.Contains("same.txt", names);
            Assert.DoesNotContain("PPD.ini", names);
            Assert.DoesNotContain("Thumbs.db", names);
            Assert.Equal("", viewModel.StatusText);
        }

        [Fact]
        public void CopyDiff_CopiesToSelectedFolder()
        {
            var (oldFolder, newFolder) = DiffFolders.Create(folder);
            var viewModel = new MainWindowViewModel(dialog, folder.Path) { OldFolder = oldFolder, NewFolder = newFolder };
            viewModel.DiffCheckCommand.Execute(null);
            WaitUntilChecked(viewModel);
            dialog.Folders.Enqueue(folder.Combine("out"));

            viewModel.CopyDiffCommand.Execute(null);

            Assert.True(File.Exists(folder.Combine("out", "added.txt")));
            Assert.False(File.Exists(folder.Combine("out", "same.txt")));
        }

        [Fact]
        public void CopyDiffClever_CopiesToVersionFolderInReleases()
        {
            folder.Write(Path.Combine("1.0.0.0", "PPDInstaller(x64)", "a.txt"), "a");
            folder.Write(Path.Combine("1.0.0.1", "PPDInstaller(x64)", "a.txt"), "b");
            var viewModel = new MainWindowViewModel(dialog, folder.Path);
            viewModel.DiffCheckCommand.Execute(null);
            WaitUntilChecked(viewModel);

            viewModel.CopyDiffCleverCommand.Execute(null);

            Assert.Equal("b", File.ReadAllText(folder.Combine("PPDDiff1001_x64", "a.txt")));
        }

        [Fact]
        public void UpdateList_WritesListsNextToNewRelease()
        {
            Directory.CreateDirectory(folder.Combine("1.0.0.0", "PPDInstaller(x64)"));
            Directory.CreateDirectory(folder.Combine("1.0.0.1", "PPDInstaller(x64)"));
            folder.Write(Path.Combine("1.0.0.0", "updatelist.xml"), "<Updates />");
            folder.Write(Path.Combine("1.0.0.0", "beta_updatelist.xml"), "<Updates />");
            var viewModel = new MainWindowViewModel(dialog, folder.Path);

            viewModel.UpdateListCommand.Execute(null);

            Assert.True(File.Exists(folder.Combine("1.0.0.1", "updatelist.xml")));
        }

        [Fact]
        public void ZipAll_ZipsNewRelease()
        {
            folder.Write(Path.Combine("1.0.0.0", "PPDInstaller(x64)", "Data", "a.txt"), "a");
            folder.Write(Path.Combine("1.0.0.1", "PPDInstaller(x64)", "Data", "a.txt"), "b");
            folder.Write(Path.Combine("PPDDiff1001_x64", "Data", "a.txt"), "b");
            var viewModel = new MainWindowViewModel(dialog, folder.Path);

            viewModel.ZipAllCommand.Execute(null);

            Assert.True(File.Exists(folder.Combine("1.0.0.1", "PPDInstaller1001(x64).zip")));
        }

        [Fact]
        public void Upload_WithoutVersion_DoesNotConnect()
        {
            var viewModel = new MainWindowViewModel(dialog, folder.Path) { NewFolder = folder.Combine("latest") };

            viewModel.UploadCommand.Execute(null);
            viewModel.UploadBetaCommand.Execute(null);

            Assert.Equal(0, dialog.UserNameAsked);
        }

        [Fact]
        public void Upload_UserNameCancelled_DoesNotConnect()
        {
            Directory.CreateDirectory(folder.Combine("1.0.0.0", "PPDInstaller(x64)"));
            Directory.CreateDirectory(folder.Combine("1.0.0.1", "PPDInstaller(x64)"));
            var viewModel = new MainWindowViewModel(dialog, folder.Path);

            // ユーザー名を聞いたところでやめるので、サーバーにはつながない
            viewModel.UploadCommand.Execute(null);
            viewModel.SessionTestCommand.Execute(null);

            Assert.Equal(2, dialog.UserNameAsked);
            Assert.False(File.Exists("lastUser.txt"));
            Assert.Empty(dialog.Messages);
        }
    }
}
