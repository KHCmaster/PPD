using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace UITests.FlowScriptControlTest
{
    /// <summary>
    /// FlowScriptControlTest（フロースクリプトを編集して実行するテスト用のアプリ）の画面のテストです。
    /// </summary>
    /// <remarks>
    /// 編集の領域（描いて表示するもの）は、ノードをノードの一覧からドラッグで足す、消す、元に戻す、拡大縮小を試す。
    /// </remarks>
    public sealed class FlowScriptControlTestTests : IDisposable
    {
        readonly AppSession session = AppSession.Start("FlowScriptControlTest", "FlowScriptControlTest.exe");

        public void Dispose()
        {
            session.Dispose();
        }

        private int TabCount
        {
            get { return session.Find("DocumentTabs").FindAllChildren(cf => cf.ByControlType(ControlType.TabItem)).Length; }
        }

        private string ConsoleText
        {
            get { return session.Find("ConsoleTextBox").AsTextBox().Text; }
        }

        /// <summary>
        /// テストのデータのスクリプトを作業のフォルダにコピーして、開くのメニューから開きます。
        /// </summary>
        private void OpenScript(string fileName)
        {
            var path = Path.Combine(session.WorkDirectory, fileName);
            File.Copy(Path.Combine(AppContext.BaseDirectory, "FlowScriptControlTest", "TestData", fileName), path, true);
            AppSession.InvokeAsync(session.FindMenuItem("FileMenu", "OpenMenuItem"));
            FileDialogs.Accept(session.WaitForModal(), path);
            Assert.True(session.WaitForNoModal());
        }

        /// <summary>
        /// 実行して、終わるまで待ちます。
        /// </summary>
        private void ExecuteAndWait()
        {
            var execute = session.Find("ExecuteButton").AsButton();
            execute.Invoke();
            Assert.True(AppSession.WaitUntil(() => execute.IsEnabled));
        }

        [Fact]
        public void NewDocument_AddsTab()
        {
            var before = TabCount;
            session.FindMenuItem("FileMenu", "NewMenuItem").Invoke();

            Assert.True(AppSession.WaitUntil(() => TabCount == before + 1));
        }

        [Fact]
        public void OpenAndExecute_WritesToConsole()
        {
            // 開くと、今のタブに読み込んで、タブの名前をファイルの名前にする
            OpenScript("hello.fsml");
            Assert.NotNull(session.Find("DocumentTabs").FindFirstChild(cf => cf.ByControlType(ControlType.TabItem).And(cf.ByName("hello.fsml"))));

            ExecuteAndWait();

            Assert.True(AppSession.WaitUntil(() => ConsoleText.Contains("Hello MVVM")));
        }

        [Fact]
        public void ClearConsole_RemovesOutput()
        {
            OpenScript("hello.fsml");
            ExecuteAndWait();
            Assert.True(AppSession.WaitUntil(() => ConsoleText.Contains("Hello MVVM")));

            session.FindMenuItem("ConsoleMenu", "ClearConsoleMenuItem").Invoke();

            Assert.True(AppSession.WaitUntil(() => ConsoleText == ""));
        }

        private string SaveTo(string fileName)
        {
            var path = Path.Combine(session.WorkDirectory, fileName);
            AppSession.InvokeAsync(session.FindMenuItem("FileMenu", "SaveMenuItem"));
            FileDialogs.Accept(session.WaitForModal(), path);
            Assert.True(AppSession.WaitUntil(() => File.Exists(path) && new FileInfo(path).Length > 0));
            Assert.True(session.WaitForNoModal());
            return File.ReadAllText(path);
        }

        [Fact]
        public void NodeIsDraggedFromListDeletedAndRestored()
        {
            // ノードの一覧を絞り込む（名前空間を「.」で区切る。合うものまで開く）
            var filter = session.Find("filterTextBox").AsTextBox();
            filter.Text = "console.writeline";
            filter.Focus();
            FlaUI.Core.Input.Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.ENTER);
            var tree = session.Find("treeView");
            AutomationElement item = null;
            Assert.True(AppSession.WaitUntil(() => (item = tree.FindFirstDescendant(cf => cf.ByName("WriteLine"))) != null), "ノードが一覧にありません");

            // 編集の領域へドラッグして足す
            var tabs = session.Find("DocumentTabs");
            session.DragBetween(item, 20, 8, tabs, 200, 150);
            Assert.Contains("WriteLine", SaveTo("added.fsml"));

            // 落とした位置のノードを選んで、Delete で消し、Ctrl+Z で戻す
            session.ClickAt(tabs, 215, 165);
            FlaUI.Core.Input.Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.DELETE);
            FlaUI.Core.Input.Wait.UntilInputIsProcessed();
            Assert.DoesNotContain("WriteLine", SaveTo("deleted.fsml"));
            session.FindMenuItem("EditMenu", "UndoMenuItem").Invoke();
            Assert.Contains("WriteLine", SaveTo("restored.fsml"));
            session.FindMenuItem("EditMenu", "RedoMenuItem").Invoke();
            Assert.DoesNotContain("WriteLine", SaveTo("redone.fsml"));
            session.FindMenuItem("EditMenu", "UndoMenuItem").Invoke();

            // 位置の自動調整、すべて展開・縮小
            session.FindMenuItem("EditMenu", "AdjustPositionMenuItem").Invoke();
            session.FindMenuItem("EditMenu", "ExpandAllMenuItem").Invoke();
            session.FindMenuItem("EditMenu", "CollapseAllMenuItem").Invoke();
            Assert.Contains("WriteLine", SaveTo("adjusted.fsml"));

            // ホイールで拡大縮小する
            FlaUI.Core.Input.Mouse.Scroll(2);
            FlaUI.Core.Input.Mouse.Scroll(-2);
            Assert.Empty(session.ModalWindowsOf());
        }

        [Fact]
        public void Save_WritesScriptFile()
        {
            OpenScript("hello.fsml");
            var path = Path.Combine(session.WorkDirectory, "saved.fsml");
            AppSession.InvokeAsync(session.FindMenuItem("FileMenu", "SaveMenuItem"));
            FileDialogs.Accept(session.WaitForModal(), path);

            Assert.True(AppSession.WaitUntil(() => File.Exists(path) && new FileInfo(path).Length > 0));
            Assert.True(session.WaitForNoModal());
            Assert.Contains("Hello MVVM", File.ReadAllText(path));
        }

        [Fact]
        public void CloseTab_FromTabMenu()
        {
            session.FindMenuItem("FileMenu", "NewMenuItem").Invoke();
            Assert.True(AppSession.WaitUntil(() => TabCount >= 2));
            var before = TabCount;
            var tab = session.Find("DocumentTabs").FindAllChildren(cf => cf.ByControlType(ControlType.TabItem)).Last();

            // タブの見出しを右クリックして、閉じる
            session.RightClickForPopup(tab, 10, 8, "CloseTabMenuItem").AsMenuItem().Invoke();

            Assert.True(AppSession.WaitUntil(() => TabCount == before - 1));
        }

        [Fact]
        public void Execute_TwoTabs_RunsBoth()
        {
            OpenScript("hello.fsml");
            session.FindMenuItem("FileMenu", "NewMenuItem").Invoke();
            Assert.True(AppSession.WaitUntil(() => TabCount >= 2));
            OpenScript("hello.fsml");

            ExecuteAndWait();

            // すべてのタブのスクリプトを実行するので、2 回書かれる
            Assert.True(AppSession.WaitUntil(() => ConsoleText.Split("Hello MVVM").Length - 1 == 2));
        }

        [Fact]
        public void Stdin_KeepsTextAfterReopen()
        {
            AppSession.InvokeAsync(session.FindMenuItem("ConsoleMenu", "StdinMenuItem"));
            var window = session.WaitForModal();
            window.FindFirstDescendant(cf => cf.ByAutomationId("StdinTextBox")).AsTextBox().Text = "abc";
            window.FindFirstDescendant(cf => cf.ByAutomationId("OkButton")).AsButton().Invoke();
            Assert.True(session.WaitForNoModal());

            AppSession.InvokeAsync(session.FindMenuItem("ConsoleMenu", "StdinMenuItem"));
            window = session.WaitForModal();

            Assert.Equal("abc", window.FindFirstDescendant(cf => cf.ByAutomationId("StdinTextBox")).AsTextBox().Text);
        }
    }
}
