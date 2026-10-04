using BezierDrawControl;
using PPDEditor;
using PPDEditor.Command.PPDSheet;
using PPDEditor.Models;
using PPDEditor.ViewModels.Docks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnitTests.BezierDrawControlTest;
using Xunit;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// 曲線の上にマークを並べるドックのテストです。曲線の編集は本物の BezierController を使い、描画は数えるだけにする。
    /// </summary>
    public class GeometryCreatorViewModelTests : IDisposable
    {
        sealed class FakeGeometryHost : IGeometryHost
        {
            public PPDSheet SelectedSheet { get; set; } = new();
            public float Bpm => 120;
            public (int Width, int Height) CanvasSize => (600, 325);
            public List<string> Messages { get; } = [];
            public Func<SquareGrid, SquareGrid> Edit { get; set; } = _ => null;
            public int Redrawn { get; private set; }

            public void ShowMessage(string text) => Messages.Add(text);

            public SquareGrid EditGrid(SquareGrid grid) => Edit(grid);

            public void Redraw() => Redrawn++;
        }

        readonly TempFolder folder = new(setCurrentDirectory: true);
        readonly FakeGeometryHost host = new();
        readonly BezierController controller = new();
        readonly GeometryCreatorViewModel viewModel;

        public GeometryCreatorViewModelTests()
        {
            AppFiles.CopyTo("PPDEditor", folder);
            Utility.ChangeLanguage("jp");
            viewModel = new GeometryCreatorViewModel(host);
            viewModel.Attach(controller);
        }

        public void Dispose()
        {
            folder.Dispose();
        }

        private void Click(float x, float y)
        {
            controller.MouseDown(x, y, true, BezierModifierKeys.None);
            controller.MouseUp();
        }

        // Ctrl を押して点をつかみ、動かす（吸い付けるのは動かしたときだけ）
        private PointF Drag(PointF from, float x, float y)
        {
            controller.MouseDown(from.X, from.Y, true, BezierModifierKeys.Control);
            controller.MouseMove(x, y, BezierModifierKeys.Control);
            controller.MouseUp();
            return controller.BCPS[0].Second;
        }

        [Fact]
        public void Generate_CircleAndPolygon()
        {
            viewModel.GenerateCommand.Execute(null);
            Assert.Equal(5, controller.BCPSCount);
            Assert.Equal(new PointF(360, 162), controller.BCPS[0].Second);

            viewModel.CreatePolygon = true;
            viewModel.PolygonCount = "x";
            viewModel.GenerateCommand.Execute(null);
            viewModel.PolygonCount = "2";
            viewModel.GenerateCommand.Execute(null);
            Assert.Equal([Utility.Language["GCError1"], Utility.Language["GCError2"]], host.Messages);

            viewModel.NudgePolygonCount(4);
            viewModel.GenerateCommand.Execute(null);
            Assert.Equal(7, controller.BCPSCount);

            viewModel.PolygonCount = "x";
            viewModel.NudgePolygonCount(1);
            Assert.Equal("x", viewModel.PolygonCount);

            viewModel.ClearCommand.Execute(null);
            Assert.Equal(0, controller.BCPSCount);
            viewModel.UndoCommand.Execute(null);
            Assert.Equal(7, controller.BCPSCount);
            viewModel.RedoCommand.Execute(null);
            Assert.Equal(0, controller.BCPSCount);
        }

        [Fact]
        public void Snap_OnlyWhenGridShown()
        {
            Click(203, 98);
            Assert.Equal(new PointF(201, 99), Drag(new PointF(203, 98), 201, 99));

            viewModel.SquareGridEnabled = true;
            // 赤い枠の左上（100, 50）から 25 ごとの格子
            Assert.Equal(new PointF(200, 100), Drag(new PointF(201, 99), 203, 98));
            Assert.Equal(new PointF(212, 112), Drag(new PointF(200, 100), 212, 112));
        }

        [Theory]
        // 枠より左や上（負の座標）でも吸い付ける
        [InlineData(0, 0, 76, 27, 75, 25)]
        // ずれが格子の半分より大きくても、一番近い格子点に吸い付ける（格子は 15, 40, 65...）
        [InlineData(15, 15, 139, 91, 140, 90)]
        public void Snap_NegativeAndLargeOffset(int offsetX, int offsetY, float x, float y, float expectedX, float expectedY)
        {
            viewModel.SquareGrid = new SquareGrid { Width = 25, Height = 25, OffsetX = offsetX, OffsetY = offsetY, GridColor = Color.Green };
            viewModel.SquareGridEnabled = true;
            Click(300, 300);

            Assert.Equal(new PointF(expectedX, expectedY), Drag(new PointF(300, 300), x, y));
        }

        [Fact]
        public void EditGrid_ShowsDoubleSize()
        {
            viewModel.EditGridCommand.Execute(null);
            Assert.Equal(25, viewModel.SquareGrid.Width);

            host.Edit = grid =>
            {
                Assert.Equal((50, 50), (grid.Width, grid.Height));
                return new SquareGrid { Width = 40, Height = 60, OffsetX = 10, OffsetY = 20, GridColor = Color.Red };
            };
            viewModel.EditGridCommand.Execute(null);

            var result = viewModel.SquareGrid;
            Assert.Equal((20, 30, 5, 10, Color.Red), (result.Width, result.Height, result.OffsetX, result.OffsetY, result.GridColor));
        }

        [Fact]
        public void Draw_GridFrameAndPlacement()
        {
            var context = new CountingContext();
            controller.Draw(context);
            // 赤い枠だけ
            Assert.Equal(1, context.Other);
            Assert.Empty(context.Lines);

            viewModel.SquareGridEnabled = true;
            viewModel.GenerateCommand.Execute(null);
            context = new CountingContext();
            controller.Draw(context);
            Assert.True(context.Lines.Count > 20);

            // 選んでいる範囲がないので、マークの位置は描かない
            host.SelectedSheet = null;
            viewModel.SquareGrid = null;
            context = new CountingContext();
            controller.Draw(context);
            Assert.True(context.Other >= 1);
        }

        [Fact]
        public void Transform_ShowsAndAppliesValues()
        {
            Assert.Equal(Utility.Language["GCHint1"], viewModel.HintText);
            viewModel.GenerateCommand.Execute(null);
            viewModel.StartTransformCommand.Execute(null);
            Assert.True(viewModel.IsTransforming);
            Assert.Equal(Utility.Language["GCHint2"], viewModel.HintText);

            controller.Draw(new CountingContext());
            Assert.Equal(("100%", "100%", "0", "0px", "0px"), (viewModel.TransformWidth, viewModel.TransformHeight, viewModel.TransformRotation, viewModel.TransformX, viewModel.TransformY));

            viewModel.TransformWidth = "200%";
            viewModel.TransformX = "15px";
            viewModel.TransformRotation = "abc";
            Assert.Equal(new PointF(2, 1), controller.TransformScale);
            Assert.Equal(new PointF(15, 0), controller.TransformTranslation);
            Assert.Equal(0, controller.TransformRotation);

            viewModel.ResetTransformCommand.Execute(null);
            Assert.Equal(new PointF(1, 1), controller.TransformScale);
            // 変形している間は消さない
            viewModel.ClearCommand.Execute(null);
            Assert.Equal(5, controller.BCPSCount);

            viewModel.EndTransformCommand.Execute(null);
            Assert.False(viewModel.IsTransforming);
            viewModel.ResetTransformCommand.Execute(null);
        }

        [Fact]
        public void Options_RedrawAndRestrictAngle()
        {
            var before = host.Redrawn;
            viewModel.DrawPos = false;
            viewModel.DrawAngle = false;
            viewModel.AngleMode = GeometryAngleMode.Normal1;
            viewModel.IntervalMode = GeometryIntervalMode.FixedDistance;
            Assert.True(viewModel.IsFixedDistanceEnabled);
            viewModel.FixedDistanceText = "abc";
            Assert.Equal(0, viewModel.FixedDistance);
            viewModel.FixedDistance = 40;
            Assert.Equal("40", viewModel.FixedDistanceText);
            Assert.Equal(before + 5, host.Redrawn);

            viewModel.AngleRestriction = 15;
            Assert.Equal(6, controller.RestrictAngleSplit);
            viewModel.AngleRestriction = 0;
            Assert.Equal(0, controller.RestrictAngleSplit);
        }

        [Fact]
        public void Apply_OnlyWithCurveAndSheet()
        {
            Assert.True(viewModel.ApplyCommand.CanExecute(null));
            viewModel.ApplyMoved = true;
            Assert.False(viewModel.ApplyCommand.CanExecute(null));
            viewModel.ApplyMoved = false;

            viewModel.ApplyCommand.Execute(null);
            viewModel.GenerateCommand.Execute(null);
            viewModel.ApplyCommand.Execute(null);
            host.SelectedSheet = null;
            viewModel.ApplyCommand.Execute(null);

            Click(10, 10);
            viewModel.DeleteCommand.Execute(null);
            Assert.Equal(5, controller.BCPSCount);
        }
    }
}
