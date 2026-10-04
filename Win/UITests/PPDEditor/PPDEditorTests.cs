using FlaUI.Core.AutomationElements;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace UITests.PPDEditor
{
    /// <summary>
    /// PPDEditor を 1 回だけ起動して、テストのクラスの中で使い回します（起動に時間がかかるため）。
    /// </summary>
    /// <remarks>
    /// 本物の PPD.ini（AppSession がコピーしない）、プロジェクト（Projects）、最近使ったプロジェクト、
    /// アカウントの情報を持つことがある PPDFramework.setting.db はコピーしない。
    /// </remarks>
    public sealed class PPDEditorFixture : IDisposable
    {
        public AppSession Session { get; } = AppSession.Start(Path.Combine("PPDEditor", "PPDEditor"), "PPDEditor.exe", new AppStartOptions
        {
            ExcludedFiles = ["Projects", "projectcache.dat", "PPDFramework.setting.db", "PPDUpdater.ini"],
            MainWindowTitle = "PPDEditor",
            StartupTimeout = TimeSpan.FromMinutes(1),
        });

        public PPDEditorFixture()
        {
            try
            {
                // 作者名がないときは、起動したときに入力のダイアログが出るので閉じる
                Session.ModalWindowsOf().FirstOrDefault()?.Close();
            }
            catch
            {
                // コンストラクターで失敗すると Dispose は呼ばれないので、ここでアプリを終わらせる
                Session.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            Session.Dispose();
        }
    }

    /// <summary>
    /// PPDEditor（譜面のエディター）の、プロジェクトを開かないで使えるダイアログのテストです。
    /// </summary>
    /// <remarks>
    /// ダイアログは開いて、キャンセル（閉じる）だけをする。設定のダイアログでは、OK（設定の保存）を押さない。
    /// </remarks>
    public sealed class PPDEditorDialogTests(PPDEditorFixture fixture) : IClassFixture<PPDEditorFixture>, IDisposable
    {
        readonly AppSession session = fixture.Session;

        /// <summary>
        /// テストの後に、開いたままのダイアログやメニューを閉じておく（次のテストのため）。
        /// </summary>
        public void Dispose()
        {
            foreach (var modal in session.ModalWindowsOf())
            {
                session.CloseWindow(modal);
            }
            session.WaitForNoModal();
        }

        /// <summary>
        /// メニューの項目を押して開いたダイアログを返します。
        /// </summary>
        private Window OpenDialogFromMenu(string menuId, string itemId, out string itemName)
        {
            var item = session.FindMenuItem(menuId, itemId);
            itemName = item.Name;
            AppSession.InvokeAsync(item);
            return session.WaitForModal();
        }

        /// <summary>
        /// ダイアログを閉じて（キャンセル）、閉じたことを確かめます。
        /// </summary>
        private void CloseDialog(Window dialog)
        {
            session.CloseWindow(dialog);
            Assert.True(session.WaitForNoModal(), "ダイアログが閉じません: " + session.DescribeModalWindows());
        }

        [Theory]
        [InlineData("fileMenu", "MergeProjectMenuItem")]
        [InlineData("editMenu", "SwapLineMenuItem")]
        [InlineData("movieMenu", "CutMovieMenuItem")]
        [InlineData("OthersMenu", "SettingMenuItem")]
        public void MenuDialog_OpensWithMenuTitleAndCancels(string menuId, string itemId)
        {
            var dialog = OpenDialogFromMenu(menuId, itemId, out var itemName);

            // ダイアログのタイトルは、メニューの項目と同じ文字
            Assert.Equal(itemName.Replace("_", ""), dialog.Title.Replace("_", ""));
            CloseDialog(dialog);
        }

        [Fact]
        public void NewProject_OpensAndCancels()
        {
            var dialog = OpenDialogFromMenu("fileMenu", "NewProjectMenuItem", out _);

            Assert.NotEqual("", dialog.Title);
            CloseDialog(dialog);
        }

        [Theory]
        [InlineData("FarnessDropDown", "FarnessDetailMenuItem")]
        [InlineData("GridDropDown", "GridDetailMenuItem")]
        [InlineData("TimeLineRowDropDown", "TimeLineRowDetailMenuItem")]
        public void ToolbarDropDown_OpensDetailDialogAndCancels(string dropDownId, string itemId)
        {
            session.Find(dropDownId).AsButton().Invoke();
            AppSession.InvokeAsync(session.FindInPopups(itemId));
            var dialog = session.WaitForModal();

            CloseDialog(dialog);
        }

        [Fact]
        public void Toolbar_ShowsDefaultValues()
        {
            Assert.Equal("100", session.Find("BpmBox").AsTextBox().Text);
            Assert.Equal("240", session.Find("DisplayWidthBox").AsTextBox().Text);
        }
    }
}
