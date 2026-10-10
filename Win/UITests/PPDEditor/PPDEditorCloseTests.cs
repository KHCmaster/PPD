using Xunit;

namespace UITests.PPDEditor
{
    /// <summary>
    /// 保存していない変更があるときに、閉じるボタンで閉じるテストです（終了のメニューと同じく確かめる）。
    /// </summary>
    public sealed class PPDEditorCloseTests() : PPDEditorProjectTestBase()
    {
        [Fact]
        public void CloseButton_WithUnsavedChanges_AsksToSave()
        {
            CreateProject();
            SetBeatWidth();
            session.DoubleClickAt(session.Find("Seekmain"), Beat(1), Row(2));

            // 保存するかを聞く。キャンセルすると閉じない
            session.CloseWindow(session.MainWindow);
            MessageBoxes.Press(session.WaitForModal(), MessageBoxButton.Cancel);
            Assert.True(session.WaitForNoModal(), session.DescribeModalWindows());
            System.Threading.Thread.Sleep(1000);
            Assert.False(session.HasExited);

            // 保存しないで閉じる
            session.CloseWindow(session.MainWindow);
            MessageBoxes.Press(session.WaitForModal(), MessageBoxButton.No);
            Assert.True(session.WaitForExit(), "プロセスが終わりません");
        }
    }
}
