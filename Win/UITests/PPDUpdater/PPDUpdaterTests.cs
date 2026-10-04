using FlaUI.Core.AutomationElements;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace UITests.PPDUpdater
{
    /// <summary>
    /// PPDUpdater（PPD の更新を確かめてダウンロード、インストールするツール）の画面のテストです。
    /// </summary>
    /// <remarks>
    /// ネットワークにつながないように、設定のファイル（PPDUpdater.ini、更新のリストの場所がある）と
    /// インストールの情報（install.info）はコピーしない。更新のリストがないので、起動するとリストを取れないと知らせる。
    /// </remarks>
    public sealed class PPDUpdaterTests : IDisposable
    {
        readonly AppSession session = AppSession.Start("PPDUpdater", "PPDUpdater.exe", "PPDUpdater.ini", "install.info");

        public PPDUpdaterTests()
        {
            Assert.False(File.Exists(Path.Combine(session.WorkDirectory, "PPDUpdater.ini")), "更新のリストの場所がある設定をコピーしてはいけない");
        }

        public void Dispose()
        {
            session.Dispose();
        }

        /// <summary>
        /// 起動したときの、更新のリストを取れないというメッセージを閉じます。
        /// </summary>
        private string CloseStartupMessage()
        {
            return MessageBoxes.ReadAndClose(session.WaitForModal());
        }

        [Fact]
        public void Startup_WithoutUpdateList_ShowsErrorAndLogsIt()
        {
            Assert.Contains("アップデートリストの取得に失敗しました", CloseStartupMessage());

            // エラーのときはログを表示する。ボタンで隠せる
            var log = session.Find("LogTextBox").AsTextBox();
            Assert.Contains("Error: アップデートリストの取得に失敗しました", log.Text);
            session.Find("LogButton").AsButton().Invoke();
            Assert.True(AppSession.WaitUntil(() => session.MainWindow.FindFirstDescendant(cf => cf.ByAutomationId("LogTextBox")) == null));
        }

        [Fact]
        public void Language_SwitchToEnglish_ChangesTexts()
        {
            CloseStartupMessage();
            var menu = session.FindMenuItem("LanguageMenu");
            menu.Expand();
            menu.Items.First(i => i.Name == "English").Invoke();

            Assert.True(AppSession.WaitUntil(() => session.Find("LanguageMenu").Name == "Language"));
            Assert.Equal("File", session.Find("FileMenu").Name);
        }

        [Fact]
        public void Close_SavesLanguageAndLog()
        {
            CloseStartupMessage();
            var menu = session.FindMenuItem("LanguageMenu");
            menu.Expand();
            menu.Items.First(i => i.Name == "English").Invoke();
            Assert.True(AppSession.WaitUntil(() => session.Find("LanguageMenu").Name == "Language"));

            session.Find("CloseButton").AsButton().Invoke();

            Assert.True(session.WaitForExit());
            Assert.Contains("[Language]en", File.ReadAllText(Path.Combine(session.WorkDirectory, "PPDUpdater.ini")));
            Assert.True(File.Exists(Path.Combine(session.WorkDirectory, "update.log")));
        }

        [Fact]
        public void FinishMenu_Closes()
        {
            CloseStartupMessage();
            session.FindMenuItem("FileMenu", "FinishMenuItem").Invoke();

            Assert.True(session.WaitForExit());
        }
    }
}
