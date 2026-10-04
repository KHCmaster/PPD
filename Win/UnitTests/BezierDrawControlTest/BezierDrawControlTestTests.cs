using BezierCaliculator;
using BezierDrawControl;
using BezierDrawControlTest.Models;
using BezierDrawControlTest.ViewModels;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Xunit;

namespace UnitTests.BezierDrawControlTest
{
    public class SquareGridTests
    {
        [Theory]
        [InlineData(203, 198, 200, 200)]
        [InlineData(210, 200, 210, 200)]
        [InlineData(0, 0, 0, 0)]
        // 負の座標でも吸い付ける
        [InlineData(-48, -52, -50, -50)]
        [InlineData(-3, 4, 0, 0)]
        public void Snap_WithoutOffset(float x, float y, float expectedX, float expectedY)
        {
            var grid = new SquareGrid { Width = 50, Height = 50, OffsetX = 200, OffsetY = 200 };

            Assert.Equal(new PointF(expectedX, expectedY), grid.Snap(new PointF(x, y), 6));
        }

        [Theory]
        // 格子は 30, 80, 130 ...（ずれが半分より大きくても、一番近い格子点に吸い付ける）
        [InlineData(81, 78, 80, 80)]
        [InlineData(29, 33, 30, 30)]
        [InlineData(55, 80, 55, 80)]
        public void Snap_WithOffset(float x, float y, float expectedX, float expectedY)
        {
            var grid = new SquareGrid { Width = 50, Height = 50, OffsetX = 30, OffsetY = -20 };

            Assert.Equal(new PointF(expectedX, expectedY), grid.Snap(new PointF(x, y), 6));
        }

        [Fact]
        public void NormalizedOffset_IsWithinCell()
        {
            var grid = new SquareGrid { Width = 50, Height = 40, OffsetX = -20, OffsetY = 130 };

            Assert.Equal((30, 10), (grid.NormalizedOffsetX, grid.NormalizedOffsetY));
            Assert.Equal((5, 5), (new SquareGrid().Width, new SquareGrid().Height));
        }
    }

    /// <summary>
    /// 描いたものを数えるだけの描画先です。
    /// </summary>
    sealed class CountingContext : IBezierDrawContext
    {
        public List<(PointF P1, PointF P2)> Lines { get; } = [];
        public int Other { get; private set; }

        public void DrawString(string text, Color color, float fontSize, PointF point) => Other++;

        public void DrawLine(Color color, PointF p1, PointF p2) => Lines.Add((p1, p2));

        public void DrawLines(Color color, PointF[] points) => Other++;

        public void DrawEllipse(Color color, RectangleF rect) => Other++;

        public void DrawRectangle(Color color, float x, float y, float width, float height) => Other++;

        public void FillRectangle(Color color, float x, float y, float width, float height) => Other++;
    }

    public class BezierDrawControlTestViewModelTests
    {
        private static (MainWindowViewModel ViewModel, BezierController Controller) Create()
        {
            var controller = new BezierController();
            var viewModel = new MainWindowViewModel();
            viewModel.Attach(controller);
            return (viewModel, controller);
        }

        private static void Click(BezierController controller, float x, float y)
        {
            controller.MouseDown(x, y, true, BezierModifierKeys.None);
            controller.MouseUp();
        }

        [Fact]
        public void Commands_WithoutController_DoNothing()
        {
            var viewModel = new MainWindowViewModel();

            viewModel.UndoCommand.Execute(null);
            viewModel.RedoCommand.Execute(null);
            viewModel.StartTransformCommand.Execute(null);
            viewModel.EndTransformCommand.Execute(null);
            viewModel.DeleteCommand.Execute(null);

            Assert.Equal((800, 450), (viewModel.CanvasWidth, viewModel.CanvasHeight));
        }

        [Fact]
        public void DragPoint_SnapsToGrid()
        {
            var (_, controller) = Create();
            Click(controller, 120, 130);
            Click(controller, 320, 260);

            // Ctrl を押して点をつかみ、格子点の近くに動かす（吸い付けるのは動かしたときだけ）
            controller.MouseDown(120, 130, true, BezierModifierKeys.Control);
            controller.MouseMove(203, 198, BezierModifierKeys.Control);
            controller.MouseUp();

            Assert.Equal([new PointF(200, 200), new PointF(320, 260)], controller.BCPS.Select(b => b.Second));
            Assert.Equal(3, controller.RestrictAngleSplit);
        }

        [Fact]
        public void UndoRedo_GoThroughHistory()
        {
            var (viewModel, controller) = Create();
            Click(controller, 100, 100);
            Click(controller, 300, 100);

            viewModel.UndoCommand.Execute(null);
            Assert.Equal(1, controller.BCPSCount);
            viewModel.RedoCommand.Execute(null);
            Assert.Equal(2, controller.BCPSCount);
        }

        [Fact]
        public void Transform_StartsOnlyWithTwoPoints()
        {
            var (viewModel, controller) = Create();
            Click(controller, 100, 100);

            viewModel.StartTransformCommand.Execute(null);
            Assert.False(controller.IsTransformMode);

            Click(controller, 300, 100);
            viewModel.StartTransformCommand.Execute(null);
            Assert.True(controller.IsTransformMode);
            viewModel.EndTransformCommand.Execute(null);
            Assert.False(controller.IsTransformMode);
            Assert.Equal(2, controller.BCPSCount);
        }

        [Fact]
        public void Delete_RemovesSelectedPoint()
        {
            var (viewModel, controller) = Create();
            Click(controller, 100, 100);
            Click(controller, 300, 100);

            // 最後に加えた点を選んでいる
            viewModel.DeleteCommand.Execute(null);

            Assert.Equal([new PointF(100, 100)], controller.BCPS.Select(b => b.Second));
        }

        [Fact]
        public void Draw_DrawsGridAfterCurve()
        {
            var (_, controller) = Create();
            var context = new CountingContext();
            controller.Draw(context);
            // 格子の縦の線（x = 0 から 800 まで 50 ごと）と横の線（y = 0 から 450 まで 50 ごと）
            Assert.Equal(21 + 14, context.Lines.Count);
            Assert.Contains((new PointF(200, 0), new PointF(200, 450)), context.Lines);
            Assert.Contains((new PointF(0, 450), new PointF(800, 450)), context.Lines);

            Click(controller, 100, 100);
            Click(controller, 300, 100);
            var withCurve = new CountingContext();
            controller.Draw(withCurve);

            Assert.True(withCurve.Other > 0);
            Assert.True(withCurve.Lines.Count >= 35);
        }
    }
}
