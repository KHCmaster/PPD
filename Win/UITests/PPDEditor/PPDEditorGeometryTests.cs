using FlaUI.Core.AutomationElements;
using System.IO;
using System.Linq;
using Xunit;

namespace UITests.PPDEditor
{
    /// <summary>
    /// PPDEditor の図形のドックで、図形（円や多角形）を作って、範囲で選んだマークを図形の上に並べるテストです。
    /// </summary>
    public sealed class PPDEditorGeometryTests() : PPDEditorProjectTestBase("PPDEditor.GeometryCreator")
    {
        [Fact]
        public void MarksArePlacedOnGeneratedShapes()
        {
            CreateProject();
            SetBeatWidth();
            var timeline = session.Find("Seekmain");
            foreach (var beat in new[] { 1, 2, 3 })
            {
                session.DoubleClickAt(timeline, Beat(beat), Row(2));
            }
            Save();
            var before = File.ReadAllBytes(LayerFile);

            // 円を作って、範囲で選んだマークを並べる
            session.DragAt(timeline, Beat(0.5), Row(1), Beat(3.5), Row(3));
            session.Find("GeometryGenerateButton").AsButton().Invoke();
            session.Find("GeometryApplyButton").AsButton().Invoke();
            Save();
            var onCircle = File.ReadAllBytes(LayerFile);
            Assert.False(before.SequenceEqual(onCircle), "マークの位置が変わりません");

            // 多角形（数がおかしいときは知らせる）
            session.Find("GeometryPolygonRadio").AsRadioButton().IsChecked = true;
            session.Find("GeometryGenerateButton").AsButton().Invoke();
            session.Find("GeometryApplyButton").AsButton().Invoke();
            Save();
            Assert.False(onCircle.SequenceEqual(File.ReadAllBytes(LayerFile)), "マークの位置が変わりません");

            session.Find("GeometryClearButton").AsButton().Invoke();
            Assert.Empty(session.ModalWindowsOf());
        }
    }
}
