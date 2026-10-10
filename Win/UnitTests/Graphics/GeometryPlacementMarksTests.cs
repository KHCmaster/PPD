using BezierDrawControl;
using PPDEditor;
using PPDEditor.Command.PPDSheet;
using PPDEditor.Models;
using PPDEditor.ViewModels.Docks;
using PPDFramework;
using System;
using System.Linq;
using System.Numerics;
using Xunit;
using EditorUtility = PPDEditor.Utility;

namespace UnitTests.Graphics
{
    /// <summary>
    /// 曲線の上に、範囲で選んだマークを並べるテストです（マークは本物）。
    /// </summary>
    [Collection(GraphicsCollection.Name)]
    public sealed class GeometryPlacementMarksTests : IDisposable
    {
        sealed class Host(PPDSheet sheet) : IGeometryHost
        {
            public PPDSheet SelectedSheet { get; } = sheet;
            public float Bpm => 120;
            public (int Width, int Height) CanvasSize => (600, 325);
            public void ShowMessage(string text) { }
            public SquareGrid EditGrid(SquareGrid grid) => null;
            public void Redraw() { }
        }

        readonly EditorMarkEnvironment environment;
        readonly TempFolder folder = new(setCurrentDirectory: true);
        readonly PPDSheet sheet = new() { GetVisibleRows = () => [.. Enumerable.Range(0, 10)] };
        readonly BezierController controller = new();
        readonly GeometryCreatorViewModel viewModel;
        readonly Mark[] marks;

        public GeometryPlacementMarksTests(GraphicsDevice graphics)
        {
            environment = new EditorMarkEnvironment(graphics);
            AppFiles.CopyTo("PPDEditor", folder);
            EditorUtility.ChangeLanguage("jp");
            marks = [.. Enumerable.Range(0, 3).Select(i => sheet.AddMark(i, (int)ButtonType.Circle, false))];
            sheet.ClearHistory();
            sheet.RecStart = new CustomPoint(-0.5f, (int)ButtonType.Circle);
            sheet.RecEnd = new CustomPoint(2.5f, (int)ButtonType.Circle);
            viewModel = new GeometryCreatorViewModel(new Host(sheet));
            viewModel.Attach(controller);
        }

        public void Dispose()
        {
            folder.Dispose();
            environment.Dispose();
        }

        void Click(float x, float y)
        {
            controller.MouseDown(x, y, true, BezierModifierKeys.None);
            controller.MouseUp();
        }

        static void Near(Vector2 expected, Vector2 actual)
        {
            Assert.Equal(expected.X, actual.X, 0.01f);
            Assert.Equal(expected.Y, actual.Y, 0.01f);
        }

        [Fact]
        public void Apply_PlacesSelectedMarksOnTheCurve()
        {
            // 枠（100,50）-（500,275）が画面全体（800x450）になる
            Click(100, 50);
            Click(500, 275);
            // 曲線を描くときに、並べる位置を求める
            controller.Draw(new UnitTests.BezierDrawControlTest.CountingContext());

            viewModel.ApplyCommand.Execute(null);

            Near(Vector2.Zero, marks[0].Position);
            Near(new Vector2(800, 450), marks[2].Position);
            Assert.InRange(marks[1].Position.X, 1, 799);
            Assert.Equal(marks[1].Position.X * 450 / 800, marks[1].Position.Y, 1);
            sheet.Undo();
            Assert.All(marks, m => Assert.Equal(new Vector2(400, 225), m.Position));
        }

        [Fact]
        public void ApplyMoved_PlacesAsTheCurveChanges()
        {
            viewModel.ApplyMoved = true;
            Assert.False(viewModel.ApplyCommand.CanExecute(null));

            Click(100, 50);
            Click(500, 50);
            controller.Draw(new UnitTests.BezierDrawControlTest.CountingContext());

            Near(Vector2.Zero, marks[0].Position);
            Near(new Vector2(800, 0), marks[2].Position);
        }
    }
}
