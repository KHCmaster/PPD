using FlaUI.Core.AutomationElements;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace UITests.FolderDiffChecker
{
    /// <summary>
    /// FolderDiffChecker（リリースのフォルダの差分を調べて、更新のファイルを作るツール）の画面のテストです。
    /// </summary>
    /// <remarks>
    /// アップロードやサーバーにつなぐボタン（UpdateList、Upload、Upload(beta)、SessonTest）は押さない。
    /// 鍵（*.ppk）と最後のユーザーの名前（lastUser.txt）は、AppSession がコピーしない。
    /// </remarks>
    public sealed class FolderDiffCheckerTests : IDisposable
    {
        readonly AppSession session = AppSession.Start("FolderDiffChecker", "FolderDiffChecker.exe", new AppStartOptions
        {
            // 起動する前に、鍵をコピーしていないことを確かめる
            Prepare = workDirectory => Assert.Empty(Directory.GetFiles(workDirectory, "*.ppk")),
        });

        public void Dispose()
        {
            session.Dispose();
        }

        /// <summary>
        /// 比べる 2 つのフォルダを作ります。
        /// </summary>
        /// <returns>古いフォルダと新しいフォルダ。</returns>
        private (string Old, string New) CreateFolders()
        {
            var oldFolder = Path.Combine(session.WorkDirectory, "test_old");
            var newFolder = Path.Combine(session.WorkDirectory, "test_new");
            Directory.CreateDirectory(oldFolder);
            Directory.CreateDirectory(newFolder);
            File.WriteAllText(Path.Combine(oldFolder, "same.txt"), "same");
            File.WriteAllText(Path.Combine(newFolder, "same.txt"), "same");
            File.WriteAllText(Path.Combine(oldFolder, "edited.txt"), "before");
            File.WriteAllText(Path.Combine(newFolder, "edited.txt"), "after editing");
            File.WriteAllText(Path.Combine(newFolder, "added.txt"), "added");
            File.WriteAllText(Path.Combine(newFolder, "PPD.ini"), "ignored");
            return (oldFolder, newFolder);
        }

        private void SetFolders(string oldFolder, string newFolder)
        {
            session.Find("OldFolderBox").AsTextBox().Text = oldFolder;
            session.Find("NewFolderBox").AsTextBox().Text = newFolder;
        }

        /// <summary>
        /// 差分の一覧の、名前の列を返します。
        /// </summary>
        private string[] DiffNames()
        {
            var grid = session.Find("DiffGrid").AsGrid();
            return [.. grid.Rows.Select(row => row.Cells[0].Value)];
        }

        [Fact]
        public void DiffCheck_ListsAddedAndEditedFiles()
        {
            var (oldFolder, newFolder) = CreateFolders();
            SetFolders(oldFolder, newFolder);
            var button = session.Find("DiffCheckButton").AsButton();
            button.Invoke();

            // 調べ終わると、ボタンを押せるようになる
            Assert.True(AppSession.WaitUntil(() => button.IsEnabled && DiffNames().Length >= 2));
            var names = DiffNames();
            Assert.Contains("edited.txt", names);
            Assert.Contains("added.txt", names);
            Assert.DoesNotContain("same.txt", names);
            // 無視するファイル（既定は PPD.ini と Data.pak）は出さない
            Assert.DoesNotContain("PPD.ini", names);
        }

        [Fact]
        public void DiffCheck_IgnoreFilesCanBeChanged()
        {
            var (oldFolder, newFolder) = CreateFolders();
            SetFolders(oldFolder, newFolder);
            session.Find("IgnoreFilesBox").AsTextBox().Text = "added.txt";
            var button = session.Find("DiffCheckButton").AsButton();
            button.Invoke();

            Assert.True(AppSession.WaitUntil(() => button.IsEnabled && DiffNames().Contains("PPD.ini")));
            Assert.DoesNotContain("added.txt", DiffNames());
        }

        [Fact]
        public void DiffCheck_MissingFolder_ShowsMessage()
        {
            SetFolders(Path.Combine(session.WorkDirectory, "not_exist_old"), Path.Combine(session.WorkDirectory, "not_exist_new"));
            AppSession.InvokeAsync(session.Find("DiffCheckButton"));

            Assert.Contains("存在しないディレクトリを比較しています。", MessageBoxes.ReadAndClose(session.WaitForModal()));
        }
    }
}
