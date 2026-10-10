using FlaUI.Core.AutomationElements;
using System.IO;
using System.Linq;
using Xunit;

namespace UITests.PPDEditor
{
    /// <summary>
    /// PPDEditor のスクリプトのドック（スクリプトの一覧）で、譜面のスクリプトを作る、名前を変える、無効にする、開く、消すテストです。
    /// </summary>
    public sealed class PPDEditorScriptTests() : PPDEditorProjectTestBase("PPDEditor.ScriptManager")
    {
        string ScriptsDirectory => Path.Combine(ProjectDirectory, "Base_Scripts");

        Tree scriptTree;

        // 木は一度だけ探して使い回す（PPDEditor のウィンドウ全体を何度も探すと、時間がかかる）
        Tree ScriptTree => scriptTree ??= session.Find("scriptTree").AsTree();

        /// <summary>
        /// 名前の項目です（名前は項目の中の文字にあるので、親の項目をたどる）。
        /// </summary>
        TreeItem ItemNamed(string name)
        {
            AutomationElement element = null;
            Assert.True(AppSession.WaitUntil(() => (element = ScriptTree.FindFirstDescendant(cf => cf.ByName(name))) != null), name + " がありません");
            while (element.ControlType != FlaUI.Core.Definitions.ControlType.TreeItem)
            {
                element = element.Parent;
            }
            return element.AsTreeItem();
        }

        void ClickMenu(TreeItem item, string menuItemId)
        {
            item.Select();
            AppSession.InvokeAsync(session.RightClickForPopup(item, 40, 8, menuItemId));
        }

        [Fact]
        public void ScriptsAreCreatedRenamedDisabledAndDeleted()
        {
            CreateProject();
            var root = ItemNamed("Scripts");

            // 新しいスクリプトとフォルダーを作る
            ClickMenu(root, "CreateScriptMenuItem");
            Assert.True(AppSession.WaitUntil(() => File.Exists(Path.Combine(ScriptsDirectory, "New Script.fsml"))), "スクリプトができません");
            ClickMenu(ItemNamed("Scripts"), "CreateScriptFolderMenuItem");
            Assert.True(AppSession.WaitUntil(() => Directory.Exists(Path.Combine(ScriptsDirectory, "New Folder"))), "フォルダーができません");

            // 名前を変える（Enter で決める）
            ClickMenu(ItemNamed("New Script"), "RenameScriptMenuItem");
            // 名前の欄は、出たときに自分でフォーカスを持ち、全部を選ぶ（PPDEditor では UI Automation の呼び出しを減らす）
            System.Threading.Thread.Sleep(1000);
            FlaUI.Core.Input.Keyboard.Type("renamed");
            FlaUI.Core.Input.Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.ENTER);
            Assert.True(AppSession.WaitUntil(() => File.Exists(Path.Combine(ScriptsDirectory, "renamed.fsml"))), "名前が変わりません");

            // 有効なスクリプトは、保存すると譜面（Base.ppd）の中に入る
            Save();
            Assert.True(session.WaitForNoModal(), session.DescribeModalWindows());
            Assert.Contains("renamed", System.Text.Encoding.UTF8.GetString(File.ReadAllBytes(Path.Combine(ProjectDirectory, "Base.ppd"))));

            // 無効にすると、プロジェクトに書かれる
            ClickMenu(ItemNamed("renamed"), "ToggleScriptMenuItem");
            Save();
            Assert.Contains("renamed", ProjectValue("Base.disabledscript"));

            // ダブルクリックで開く
            session.DoubleClickAt(ItemNamed("renamed"), 30, 8);

            // 消す（確かめるメッセージで OK）
            ClickMenu(ItemNamed("renamed"), "DeleteScriptMenuItem");
            MessageBoxes.Press(session.WaitForModal(), MessageBoxButton.Ok);
            Assert.True(session.WaitForNoModal(), session.DescribeModalWindows());
            Assert.True(AppSession.WaitUntil(() => !File.Exists(Path.Combine(ScriptsDirectory, "renamed.fsml"))), "消えません");
            Assert.True(Directory.Exists(Path.Combine(ScriptsDirectory, "New Folder")));
        }
    }
}
