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

        /// <summary>
        /// ノードの一覧を絞り込んで（名前空間を「.」で区切る。合うものまで開く）ノードを探し、編集の領域（タブの左上から x, y）へドラッグして足します。
        /// </summary>
        /// <remarks>
        /// ノードは、落とした位置を左上にして置かれる。
        /// </remarks>
        /// <returns>編集の領域のあるタブ。</returns>
        private AutomationElement AddNode(string filterText, string name, double x, double y)
        {
            var filter = session.Find("filterTextBox").AsTextBox();
            filter.Text = filterText;
            filter.Focus();
            FlaUI.Core.Input.Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.ENTER);
            var tree = session.Find("treeView");
            AutomationElement item = null;
            Assert.True(AppSession.WaitUntil(() => (item = tree.FindFirstDescendant(cf => cf.ByName(name))) != null), "ノードが一覧にありません");

            var tabs = session.Find("DocumentTabs");
            session.DragBetween(item, 20, 8, tabs, x, y);
            return tabs;
        }

        /// <summary>
        /// WriteLine のノードを、編集の領域（タブの左上から 200, 150）に足します。
        /// </summary>
        /// <returns>編集の領域のあるタブ。ノードは、タブの左上から 215, 165 のところにある。</returns>
        private AutomationElement AddWriteLineNode()
        {
            return AddNode("console.writeline", "WriteLine", 200, 150);
        }

        private static int CountOf(string text, string value)
        {
            return text.Split(value).Length - 1;
        }

        [Fact]
        public void NodeIsDraggedFromListDeletedAndRestored()
        {
            // 編集の領域へドラッグして足す
            var tabs = AddWriteLineNode();
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
        public void ContextMenu_CommentsScopesAndBreakPoints()
        {
            var tabs = AddWriteLineNode();
            const int nodeX = 215, nodeY = 165;
            AutomationElement MenuItemAt(double x, double y, string itemId) => session.RightClickForPopup(tabs, x, y, itemId);

            // ノードを選んで、ノードに付くコメントを足して、消す
            session.ClickAt(tabs, nodeX, nodeY);
            MenuItemAt(nodeX, nodeY, "addBoundCommentMenuItem").AsMenuItem().Invoke();
            Assert.Equal(1, CountOf(SaveTo("bound.fsml"), "<Comment "));
            MenuItemAt(nodeX, nodeY, "removeBoundCommentMenuItem").AsMenuItem().Invoke();
            Assert.Equal(0, CountOf(SaveTo("unbound.fsml"), "<Comment "));

            // ブレークポイントを付けて、外す（付けているときだけ、外すのメニューが出る）
            MenuItemAt(nodeX, nodeY, "addBreakPointMenuItem").AsMenuItem().Invoke();
            MenuItemAt(nodeX, nodeY, "removeBreakPointMenuItem").AsMenuItem().Invoke();
            MenuItemAt(nodeX, nodeY, "addBreakPointMenuItem").AsMenuItem().Invoke();
            FlaUI.Core.Input.Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.ESCAPE);

            // 選んだノードを囲むスコープを足す。スコープの中では、中を選ぶ・スコープを消すのメニューが出る
            MenuItemAt(nodeX, nodeY, "addScopeMenuItem").AsMenuItem().Invoke();
            Assert.Equal(1, CountOf(SaveTo("scope.fsml"), "<Scope "));
            MenuItemAt(nodeX, nodeY, "selectAllInScopeMenuItem").AsMenuItem().Invoke();
            MenuItemAt(nodeX, nodeY, "selectInScopeMenuItem").AsMenuItem().Invoke();
            MenuItemAt(nodeX, nodeY, "removeScopeMenuItem").AsMenuItem().Invoke();
            Assert.Equal(0, CountOf(SaveTo("unscoped.fsml"), "<Scope "));

            // 何もないところにコメントを足す
            MenuItemAt(600, 400, "addCommentMenuItem").AsMenuItem().Invoke();
            Assert.Equal(1, CountOf(SaveTo("comment.fsml"), "<Comment "));

            // 選んだノードをメニューで消して、全体を表示する
            session.ClickAt(tabs, nodeX, nodeY);
            MenuItemAt(nodeX, nodeY, "deleteMenuItem").AsMenuItem().Invoke();
            Assert.DoesNotContain("WriteLine", SaveTo("deleted.fsml"));
            MenuItemAt(600, 400, "fitViewMenuItem").AsMenuItem().Invoke();
            Assert.Empty(session.ModalWindowsOf());
        }

        /// <summary>
        /// 検索のタブで探して、見つかったものの数を返します。
        /// </summary>
        private AutomationElement[] Search(string query)
        {
            session.Find("searchTabItem").AsTabItem().Select();
            var box = session.Find("SearchQueryBox").AsTextBox();
            box.Text = query;
            box.Focus();
            FlaUI.Core.Input.Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.ENTER);
            FlaUI.Core.Input.Wait.UntilInputIsProcessed();
            return session.Find("SearchResultList").FindAllChildren(cf => cf.ByControlType(ControlType.ListItem));
        }

        [Fact]
        public void Search_SelectsNodeAndItsPropertyIsEdited()
        {
            OpenScript("hello.fsml");

            // 値で探して選ぶと、ノードを選んで、プロパティの欄に値が出る
            var found = Search("hello mvvm");
            Assert.Single(found);
            found[0].AsListBoxItem().Select();
            var text = session.Find("TextPropertyBox").AsTextBox();
            Assert.True(AppSession.WaitUntil(() => text.Text == "Hello MVVM"), "プロパティが出ません");

            // プロパティを変えると、保存したファイルにも残る
            text.Text = "Changed";
            text.Focus();
            FlaUI.Core.Input.Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.ENTER);
            FlaUI.Core.Input.Wait.UntilInputIsProcessed();
            Assert.Contains("Changed", SaveTo("changed.fsml"));

            // つながっているノードを探す（@ID[In か Out][項目の名前]）
            Assert.Single(Search("@1[out][start]"));
            Assert.Empty(Search("not found"));
        }

        [Fact]
        public void BreakPoint_StopsExecutionUntilContinued()
        {
            OpenScript("hello.fsml");
            Search("hello mvvm")[0].AsListBoxItem().Select();
            Assert.True(AppSession.WaitUntil(() => session.Find("TextPropertyBox").AsTextBox().Text == "Hello MVVM"));

            // 選んだノードにブレークポイントを付ける（右クリックしても選んだまま）
            session.RightClickForPopup(session.Find("DocumentTabs"), 600, 400, "addBreakPointMenuItem").AsMenuItem().Invoke();

            // 実行すると、そのノードの前で止まって、デバッグのタブに値が出る
            session.Find("ExecuteButton").AsButton().Invoke();
            var resume = session.Find("ContinueButton").AsButton();
            Assert.True(AppSession.WaitUntil(() => resume.IsEnabled), "止まりません");
            Assert.DoesNotContain("Hello MVVM", ConsoleText);
            session.Find("DebugTabItem").AsTabItem().Select();
            Assert.True(AppSession.WaitUntil(() => session.Find("ExecutingTree").FindAllChildren().Length > 0), "デバッグのタブに値が出ません");

            // 1 つ進める（止めたノードが最後なので、それで終わる）
            session.Find("StepInButton").AsButton().Invoke();
            Assert.True(AppSession.WaitUntil(() => session.Find("ExecuteButton").IsEnabled), "終わりません");
            Assert.True(AppSession.WaitUntil(() => ConsoleText.Contains("Hello MVVM")));

            // もう一度実行すると、また止まり、続行で終わる
            session.Find("ExecuteButton").AsButton().Invoke();
            Assert.True(AppSession.WaitUntil(() => resume.IsEnabled), "止まりません");
            resume.Invoke();
            Assert.True(AppSession.WaitUntil(() => session.Find("ExecuteButton").IsEnabled), "終わりません");
            Assert.True(AppSession.WaitUntil(() => ConsoleText.Split("Hello MVVM").Length - 1 == 2));
        }

        [Fact]
        public void NodesAreConnectedExecutedAndRegionDeleted()
        {
            // EntryPoint（左）と WriteLine（右）を置いて、Start の端子から In の端子へドラッグしてつなぐ
            var tabs = AddWriteLineNode();
            AddNode("misc.entrypoint", "EntryPoint", 40, 150);
            const double startX = 178, inX = 212, portY = 193;
            session.DragAt(tabs, startX, portY, inX, portY);
            Assert.Equal(1, CountOf(SaveTo("connected.fsml"), "<Flow "));

            // つないだ線の先を、何もないところへドラッグすると、線が消える
            // （描画の範囲の端から 20 の内側まで動かすとスクロールするので、端には近づけない）
            session.DragAt(tabs, inX, portY, 450, 300);
            Assert.Equal(0, CountOf(SaveTo("disconnected.fsml"), "<Flow "));

            // 見出しの右のボタンで、使っていない項目を隠したり出したりする
            session.ClickAt(tabs, 359, 164);
            session.ClickAt(tabs, 359, 164);

            // 何もないところから範囲で囲んで、両方選んで消し、元に戻す
            session.DragAt(tabs, 32, 100, 450, 300);
            session.RightClickForPopup(tabs, 600, 400, "deleteMenuItem").AsMenuItem().Invoke();
            Assert.Equal(0, CountOf(SaveTo("cleared.fsml"), "<Source "));
            session.FindMenuItem("EditMenu", "UndoMenuItem").Invoke();
            Assert.Equal(2, CountOf(SaveTo("undone.fsml"), "<Source "));
            session.FindMenuItem("EditMenu", "UndoMenuItem").Invoke();
            Assert.Equal(1, CountOf(SaveTo("reconnected.fsml"), "<Flow "));

            // WriteLine を選んで、書く文字を入れる
            session.ClickAt(tabs, 280, 165);
            var text = session.Find("TextPropertyBox").AsTextBox();
            text.Text = "Connected";
            text.Focus();
            FlaUI.Core.Input.Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.ENTER);

            // 端子を右クリックして、つながっているノードを探す（見つけたノードへ表示が動く）
            session.RightClickForPopup(tabs, inX, portY, "findLinkedNodeMenuItem").AsMenuItem().Invoke();
            Assert.True(AppSession.WaitUntil(() => session.Find("SearchResultList").FindAllChildren(cf => cf.ByControlType(ControlType.ListItem)).Length == 1), "見つかりません");

            // 実行すると、つないだ WriteLine が書く
            ExecuteAndWait();
            Assert.True(AppSession.WaitUntil(() => ConsoleText.Contains("Connected")), "つながっていません");
        }

        /// <summary>
        /// 保存したファイルの、最初の要素の属性の値です。
        /// </summary>
        private static string AttributeOf(string xml, string element, string attribute)
        {
            var node = System.Xml.Linq.XDocument.Parse(xml).Descendants(element).First();
            return node.Attribute(attribute)?.Value;
        }

        /// <summary>
        /// 動かしている間に左の Ctrl を押します（押すと、動かしているものをスコープに入れる・出す）。
        /// </summary>
        private static void PressLeftControl()
        {
            FlaUI.Core.Input.Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.LCONTROL);
            FlaUI.Core.Input.Wait.UntilInputIsProcessed();
        }

        [Fact]
        public void CommentsAreEditedAndJoinedToScope()
        {
            // ノードを選んで囲むスコープ（タブの左上から 180, 130 から 395, 260）と、その右にコメントを足す
            var tabs = AddWriteLineNode();
            session.ClickAt(tabs, 280, 165);
            session.RightClickForPopup(tabs, 280, 165, "addScopeMenuItem").AsMenuItem().Invoke();
            session.RightClickForPopup(tabs, 500, 150, "addCommentMenuItem").AsMenuItem().Invoke();
            Assert.Equal("Comment", AttributeOf(SaveTo("comment.fsml"), "Comment", "Text"));

            // コメントの文字を押すと編集できる（Enter で終わる）
            session.ClickAt(tabs, 515, 167);
            FlaUI.Core.Input.Keyboard.Type("Edited");
            FlaUI.Core.Input.Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.ENTER);
            FlaUI.Core.Input.Wait.UntilInputIsProcessed();
            Assert.Equal("Edited", AttributeOf(SaveTo("edited.fsml"), "Comment", "Text"));

            // ノードをドラッグで動かす
            var before = AttributeOf(SaveTo("before.fsml"), "Source", "Y");
            session.DragAt(tabs, 240, 165, 240, 185);
            Assert.NotEqual(before, AttributeOf(SaveTo("moved.fsml"), "Source", "Y"));

            // コメントをスコープの中へ動かして Ctrl を押すと、スコープに入る
            session.DragAt(tabs, 515, 167, 230, 245, PressLeftControl);
            Assert.NotNull(AttributeOf(SaveTo("joined.fsml"), "Comment", "ScopeID"));

            // もう一度 Ctrl を押しながら外へ動かすと、スコープから出る
            session.DragAt(tabs, 230, 245, 500, 350, PressLeftControl);
            Assert.Null(AttributeOf(SaveTo("dejoined.fsml"), "Comment", "ScopeID"));
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
