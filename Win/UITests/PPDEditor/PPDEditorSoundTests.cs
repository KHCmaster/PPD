using FlaUI.Core.AutomationElements;
using System.IO;
using System.Linq;
using Xunit;

namespace UITests.PPDEditor
{
    /// <summary>
    /// PPDEditor の音のドックで、音のファイルを足し、今の時間に音の割り当てを足して消すテストです。
    /// </summary>
    public sealed class PPDEditorSoundTests() : PPDEditorProjectTestBase("PPDEditor.SoundManager")
    {
        [Fact]
        public void SoundsAndChangesAreAddedAndDeleted()
        {
            CreateProject();

            // 音のファイルを足す（ファイルを選ぶダイアログで、作業のフォルダの WAV を選ぶ）
            AppSession.InvokeAsync(session.Find("SoundAddFilesButton"));
            FileDialogs.Accept(session.WaitForFileDialog(), Path.Combine(workDirectory, "sound.wav"));
            Assert.True(session.WaitForNoModal(), session.DescribeModalWindows());
            Save();
            Assert.Contains("sound.wav", File.ReadAllText(Path.Combine(ProjectDirectory, "soundset.txt")));
            session.Find("SoundPlayButton").AsButton().Invoke();

            // 今の時間に割り当てを足して、消す
            var grid = session.Find("changeGrid").AsGrid();
            var before = grid.Rows.Length;
            PlayALittle();
            session.Find("SoundAddChangeButton").AsButton().Invoke();
            Assert.True(AppSession.WaitUntil(() => grid.Rows.Length == before + 1), "割り当てが足されません");
            grid.Rows.Last().Select();
            session.Find("SoundDeleteChangeButton").AsButton().Invoke();
            Assert.True(AppSession.WaitUntil(() => grid.Rows.Length == before), "割り当てが消えません");

            // 最初の割り当ては消せない
            grid.Rows[0].Select();
            AppSession.InvokeAsync(session.Find("SoundDeleteChangeButton"));
            Assert.NotEqual("", MessageBoxes.ReadAndClose(session.WaitForModal()));
            Assert.True(session.WaitForNoModal(), session.DescribeModalWindows());
            Save();
        }
    }
}
