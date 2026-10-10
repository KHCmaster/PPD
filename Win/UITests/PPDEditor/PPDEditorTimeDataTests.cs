using FlaUI.Core.AutomationElements;
using System.IO;
using System.Linq;
using Xunit;

namespace UITests.PPDEditor
{
    /// <summary>
    /// PPDEditor の、時間ごとのデータのドック（歌詞とイベント）で、今の時間に足して保存し、消すテストです。
    /// </summary>
    public sealed class PPDEditorTimeDataTests() : PPDEditorProjectTestBase("PPDEditor.KasiEditor", "PPDEditor.EventManager")
    {
        [Fact]
        public void LyricsAreAddedSavedAndDeleted()
        {
            CreateProject();
            PlayALittle();

            session.Find("inputBox").AsTextBox().Text = "歌詞1";
            session.Find("KasiAddButton").AsButton().Invoke();
            Save();
            var kasi = File.ReadAllText(Path.Combine(ProjectDirectory, "kasi.txt"));
            Assert.Contains(":歌詞1", kasi);

            // 選んで消す
            var grid = session.Find("kasiGrid").AsGrid();
            Assert.True(AppSession.WaitUntil(() => grid.Rows.Length == 1), "歌詞の行がありません");
            grid.Rows[0].Select();
            session.Find("KasiDeleteButton").AsButton().Invoke();
            Save();
            Assert.DoesNotContain("歌詞1", File.ReadAllText(Path.Combine(ProjectDirectory, "kasi.txt")));
        }

        [Fact]
        public void EventsAreAddedAndDeleted()
        {
            CreateProject();
            var grid = session.Find("eventGrid").AsGrid();
            var before = grid.Rows.Length;
            PlayALittle();

            // 今の時間にイベントを足す（編集のダイアログで OK）
            AppSession.InvokeAsync(session.Find("EventAddButton"));
            // OK は既定のボタンなので Enter で押す（ダイアログの中を探すと、時間がかかることがある）
            session.WaitForModal();
            System.Threading.Thread.Sleep(500);
            FlaUI.Core.Input.Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.ENTER);
            Assert.True(session.WaitForNoModal(), session.DescribeModalWindows());
            Assert.True(AppSession.WaitUntil(() => grid.Rows.Length == before + 1), "イベントが足されません");

            // 同じ時間には足せない
            AppSession.InvokeAsync(session.Find("EventAddButton"));
            Assert.NotEqual("", MessageBoxes.ReadAndClose(session.WaitForModal()));
            Assert.True(session.WaitForNoModal(), session.DescribeModalWindows());

            // 選んで消す（確かめるメッセージで OK）
            grid.Rows.Last().Select();
            AppSession.InvokeAsync(session.Find("EventDeleteButton"));
            MessageBoxes.Press(session.WaitForModal(), MessageBoxButton.Ok);
            Assert.True(session.WaitForNoModal(), session.DescribeModalWindows());
            Assert.True(AppSession.WaitUntil(() => grid.Rows.Length == before), "イベントが消えません");
            Save();
        }
    }
}
