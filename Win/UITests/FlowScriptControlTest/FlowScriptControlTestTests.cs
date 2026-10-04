using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.IO;
using Xunit;

namespace UITests.FlowScriptControlTest
{
    /// <summary>
    /// FlowScriptControlTest（フロースクリプトを編集して実行するテスト用のアプリ）の画面のテストです。
    /// </summary>
    /// <remarks>
    /// ノードの配置やつなぎ方など、編集の領域（描いて表示するもの）の操作はテストしない。
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
            File.Copy(Path.Combine(AppContext.BaseDirectory, "FlowScriptControlTest", "TestData", fileName), path);
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
