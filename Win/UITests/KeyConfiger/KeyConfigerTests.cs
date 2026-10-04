using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace UITests.KeyConfiger
{
    /// <summary>
    /// KeyConfiger（ボタンに割り当てるキーとゲームパッドのボタンを設定するツール）の画面のテストです。
    /// </summary>
    /// <remarks>
    /// キーやボタンを押して割り当てるところは、DirectInput で実際の入力を読むのでテストしない。
    /// </remarks>
    public sealed class KeyConfigerTests : IDisposable
    {
        readonly AppSession session = AppSession.Start(Path.Combine("KeyConfiger", "KeyConfiger"), "KeyConfiger.exe");

        public void Dispose()
        {
            session.Dispose();
        }

        private AutomationElement Find(string automationId)
        {
            return session.MainWindow.FindFirstDescendant(cf => cf.ByAutomationId(automationId))
                ?? throw new InvalidOperationException(automationId + " が見つかりません。");
        }

        /// <summary>
        /// メニューを開いて、名前の項目を返します。
        /// </summary>
        private MenuItem OpenMenuAndFind(string menuId, string itemName)
        {
            var menu = Find(menuId).AsMenuItem();
            menu.Expand();
            var item = menu.Items.FirstOrDefault(i => i.Name == itemName);
            return item ?? throw new InvalidOperationException(menuId + " に「" + itemName + "」がありません。");
        }

        private string[] MenuItemNames(string menuId)
        {
            var menu = Find(menuId).AsMenuItem();
            menu.Expand();
            var names = menu.Items.Select(i => i.Name).ToArray();
            menu.Collapse();
            return names;
        }

        private void CreateSetting(string name)
        {
            var menu = Find("SettingMenu").AsMenuItem();
            menu.Expand();
            AppSession.InvokeAsync(menu.Items.First(i => i.AutomationId == "CreateSettingMenuItem"));
            var dialog = session.WaitForModal();
            dialog.FindFirstDescendant(cf => cf.ByAutomationId("NameTextBox")).AsTextBox().Text = name;
            dialog.FindFirstDescendant(cf => cf.ByAutomationId("OkButton")).AsButton().Invoke();
            Assert.True(AppSession.WaitUntil(() => session.MainWindow.ModalWindows.Length == 0));
        }

        private void InvokeDeleteSetting()
        {
            var menu = Find("SettingMenu").AsMenuItem();
            menu.Expand();
            AppSession.InvokeAsync(menu.Items.First(i => i.AutomationId == "DeleteSettingMenuItem"));
        }

        [Fact]
        public void MainWindow_ListsAllButtons()
        {
            var grid = Find("KeyGrid").AsGrid();

            Assert.Equal(12, grid.RowCount);
            var headers = grid.Rows.Select(row => row.FindFirstDescendant(cf => cf.ByControlType(ControlType.Text))?.Name).ToArray();
            Assert.Equal(["Square", "Cross", "Circle", "Triangle", "Left", "Down", "Right", "Up", "R", "L", "Start", "Home"], headers);
        }

        [Fact]
        public void Language_SwitchToJapanese_ChangesTexts()
        {
            OpenMenuAndFind("LanguageMenu", "日本語").Invoke();

            Assert.True(AppSession.WaitUntil(() => Find("LanguageMenu").Name == "言語"));
            Assert.Equal("設定", Find("SettingMenu").Name);
            Assert.Equal("新しい設定を作成", MenuItemNames("SettingMenu")[0]);
        }

        [Fact]
        public void CreateSetting_AddsItToSettingMenu()
        {
            CreateSetting("uitest");

            Assert.Contains("uitest", MenuItemNames("SettingMenu"));
        }

        [Fact]
        public void DeleteSetting_DefaultCannotBeDeleted()
        {
            InvokeDeleteSetting();

            Assert.Contains("You cannot delete \"default\" setting.", MessageBoxes.ReadAndClose(session.WaitForModal()));
            Assert.Contains("default", MenuItemNames("SettingMenu"));
        }

        [Fact]
        public void DeleteSetting_AfterConfirmation_RemovesCurrentSetting()
        {
            CreateSetting("uitest");
            InvokeDeleteSetting();
            var confirm = session.WaitForModal();
            Assert.Contains("Are you sure you want to delete current setting?", MessageBoxes.GetText(confirm));
            MessageBoxes.Press(confirm, MessageBoxButton.Ok);

            Assert.True(AppSession.WaitUntil(() => session.MainWindow.ModalWindows.Length == 0));
            Assert.DoesNotContain("uitest", MenuItemNames("SettingMenu"));
        }

        [Fact]
        public void DeleteSetting_CancelKeepsSetting()
        {
            CreateSetting("uitest");
            InvokeDeleteSetting();
            MessageBoxes.Press(session.WaitForModal(), MessageBoxButton.Cancel);

            Assert.True(AppSession.WaitUntil(() => session.MainWindow.ModalWindows.Length == 0));
            Assert.Contains("uitest", MenuItemNames("SettingMenu"));
        }

        [Fact]
        public void Ok_SavesSettingsAndCloses()
        {
            CreateSetting("uitest");
            Find("OkButton").AsButton().Invoke();

            Assert.True(session.WaitForExit());
            var keyConfig = File.ReadAllText(Path.Combine(session.WorkDirectory, "keyconfig.ini"));
            Assert.Contains("uitest", keyConfig);
            Assert.True(File.Exists(Path.Combine(session.WorkDirectory, "KeyConfiger.ini")));
        }

        [Fact]
        public void Cancel_ClosesWithoutSaving()
        {
            CreateSetting("uitest");
            Find("CancelButton").AsButton().Invoke();

            Assert.True(session.WaitForExit());
            Assert.False(File.Exists(Path.Combine(session.WorkDirectory, "keyconfig.ini")));
        }
    }
}
