using BezierCaliculator;
using BezierDrawControl;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Xunit;
using static BezierDrawControl.BezierModifierKeys;

namespace UnitTests.Libraries
{
    /// <summary>
    /// ベジエ曲線をマウスで作る BezierController（画面を持たない部分）のテストです。
    /// </summary>
    public class BezierControllerTests
    {
        readonly BezierController controller = new();
        readonly List<(EditType Type, int Index)> edits = [];

        public BezierControllerTests()
        {
            controller.Edited += (s, e) => edits.Add((e.EditType, e.EditIndex));
        }

        void Click(float x, float y, BezierModifierKeys keys = None)
        {
            controller.MouseDown(x, y, true, keys);
            controller.MouseUp();
        }

        void Drag(float fromX, float fromY, float toX, float toY, BezierModifierKeys keys)
        {
            controller.MouseDown(fromX, fromY, true, keys);
            controller.MouseMove(toX, toY, keys);
            controller.MouseUp();
        }

        PointF[] Anchors => [.. controller.BCPS.Select(b => b.Second)];

        [Fact]
        public void Click_AddsPoints()
        {
            Click(10, 10);
            Click(110, 10);

            Assert.Equal([new PointF(10, 10), new PointF(110, 10)], Anchors);
            Assert.Equal(1, controller.SelectedIndex);
            Assert.Equal([(EditType.Add, 0), (EditType.Add, 1)], edits);
        }

        [Fact]
        public void ClickOnLine_InsertsPointBetween()
        {
            Click(10, 10);
            Click(110, 10);

            Click(60, 11);

            Assert.Equal(3, controller.BCPSCount);
            Assert.Equal(60, controller.BCPS[1].Second.X, 1);
            Assert.Equal((EditType.Add, 1), edits[^1]);
        }

        [Fact]
        public void ControlDrag_MovesPointAndCanUndo()
        {
            Click(10, 10);
            Click(110, 10);

            Drag(10, 10, 30, 40, Control);
            Assert.Equal(new PointF(30, 40), Anchors[0]);
            Assert.Equal((EditType.Anchor, 0), edits[^1]);

            controller.Undo();
            Assert.Equal(new PointF(10, 10), Anchors[0]);
            controller.Redo();
            Assert.Equal(new PointF(30, 40), Anchors[0]);
        }

        [Fact]
        public void AltDrag_MakesSymmetricHandles()
        {
            Click(100, 100);

            Drag(100, 100, 130, 100, Alt);
            var point = controller.BCPS[0];
            Assert.True(point.ValidFirst && point.ValidThird);
            Assert.Equal(new PointF(130, 100), point.First);
            Assert.Equal(new PointF(70, 100), point.Third);

            // Ctrl で片方を動かすと、もう片方は反対の向きになる（長さはそのまま）
            Drag(130, 100, 100, 130, Control);
            point = controller.BCPS[0];
            Assert.Equal(100, point.First.X, 2);
            Assert.Equal(130, point.First.Y, 2);
            Assert.Equal(100, point.Third.X, 2);
            Assert.Equal(70, point.Third.Y, 2);

            // Alt で片方だけ動かす
            Drag(100, 70, 140, 70, Alt);
            point = controller.BCPS[0];
            Assert.Equal(new PointF(100, 130), point.First);
            Assert.Equal(140, point.Third.X, 2);
            Assert.Equal((EditType.Handle, 0), edits[^1]);

            // Alt で点そのものを押すとハンドルを消し、その場で離すとなし
            Drag(100, 100, 101, 101, Alt);
            Assert.False(controller.BCPS[0].ValidFirst);
        }

        [Fact]
        public void ShiftDrag_SnapsAngle()
        {
            controller.RestrictAngleSplit = 2;
            Click(0, 0);
            Click(100, 0);

            // 45 度ごとにそろえる
            Drag(100, 0, 100, 90, Shift);
            var p = Anchors[1];
            Assert.Equal(p.X, p.Y, 1);

            controller.RestrictAngleSplit = 0;
            Drag(p.X, p.Y, 50, 70, Shift);
            Assert.Equal(new PointF(50, 70), Anchors[1]);
        }

        [Fact]
        public void DeleteAndClear()
        {
            Click(10, 10);
            Click(110, 10);
            Click(210, 10);

            controller.Delete();
            Assert.Equal(2, controller.BCPSCount);
            Assert.Equal(-1, controller.SelectedIndex);
            Assert.Equal((EditType.Delete, 2), edits[^1]);
            // 選んでいなければ何もしない
            controller.Delete();
            Assert.Equal(2, controller.BCPSCount);

            controller.Clear();
            Assert.Equal(0, controller.BCPSCount);
            controller.Undo();
            Assert.Equal(2, controller.BCPSCount);
        }

        [Fact]
        public void Transform_MoveAndRotate()
        {
            Click(0, 0);
            controller.StartTransform();
            // 2 つより少ないときは変形しない
            Assert.False(controller.IsTransformMode);
            Assert.Equal(PointF.Empty, controller.TransformScale);
            Assert.Equal(0, controller.TransformRotation);
            Click(100, 50);

            controller.StartTransform();
            Assert.True(controller.IsTransformMode);
            // 枠の中を Ctrl でドラッグすると動く
            Drag(50, 25, 60, 35, Control);
            Assert.Equal(new PointF(10, 10), controller.TransformTranslation);
            Assert.Equal((EditType.Transform, -1), edits[^1]);
            Assert.Equal(new PointF(10, 10), controller.BCPS[0].Second);
            // 変形の間は Undo しない
            controller.Undo();
            Assert.True(controller.IsTransformMode);

            controller.EndTransform();
            Assert.False(controller.IsTransformMode);
            Assert.Equal([new PointF(10, 10), new PointF(110, 60)], Anchors);
            Assert.Equal((EditType.TransformEnd, -1), edits[^1]);

            controller.StartTransform();
            controller.TransformRotation = 180;
            controller.TransformScale = new PointF(1, 1);
            controller.TransformTranslation = PointF.Empty;
            controller.EndTransform();
            var anchors = Anchors;
            Assert.Equal(110, anchors[0].X, 1);
            Assert.Equal(60, anchors[0].Y, 1);
        }

        [Fact]
        public void Transform_DragOutsideRotates()
        {
            Click(0, 0);
            Click(100, 0);
            controller.StartTransform();

            Drag(50, -60, 110, 0, None);

            Assert.NotEqual(0, controller.TransformRotation);
        }

        [Fact]
        public void Cursors()
        {
            Click(0, 0);
            Click(100, 0);

            Assert.Equal(BezierCursor.AnchorPlus, controller.MouseMove(50, 1, None));
            Assert.Equal(BezierCursor.Default, controller.MouseMove(50, 50, None));
            Assert.Equal(BezierCursor.Anchor, controller.MouseMove(1, 1, Control));
            Assert.Equal(BezierCursor.Default, controller.MouseMove(50, 50, Control));
            Assert.Equal(BezierCursor.Anchor, controller.MouseMove(99, 1, Shift));
            Assert.Equal(BezierCursor.Handle, controller.MouseMove(99, 1, Alt));
            Assert.Equal(BezierCursor.Default, controller.MouseMove(50, 50, Alt));

            controller.StartTransform();
            Assert.Equal(BezierCursor.Rotate, controller.MouseMove(50, -80, None));
            Assert.Equal(BezierCursorKind.Arrow, controller.MouseMove(0, 0, None).Value.Kind);
            Assert.Equal(-90 % 180 + 180, BezierCursor.Arrow(-90).Angle);
        }

        [Fact]
        public void CenterAndPreprocess()
        {
            controller.Center = new Point(100, 100);
            // 点を 10 の倍数にそろえる
            controller.PreprocessPoint += p => new PointF(p.X - p.X % 10, p.Y - p.Y % 10);

            Click(110, 120);
            Drag(110, 120, 135, 146, Control);

            Assert.Equal(new PointF(30, 40), Anchors[0]);
        }

        [Fact]
        public void NotAllowed_IgnoresMouse()
        {
            controller.AllowMouseOperation = false;

            Click(10, 10);

            Assert.Equal(0, controller.BCPSCount);
            Assert.Null(controller.MouseMove(10, 10, None));
        }

        sealed class Recorder : IBezierDrawContext
        {
            public List<string> Calls { get; } = [];
            public void DrawString(string text, Color color, float fontSize, PointF point) => Calls.Add("string");
            public void DrawLine(Color color, PointF p1, PointF p2) => Calls.Add("line");
            public void DrawLines(Color color, PointF[] points) => Calls.Add("lines");
            public void DrawEllipse(Color color, RectangleF rect) => Calls.Add("ellipse");
            public void DrawRectangle(Color color, float x, float y, float width, float height) => Calls.Add("rectangle");
            public void FillRectangle(Color color, float x, float y, float width, float height) => Calls.Add("fill");
        }

        [Fact]
        public void Draw_UsesContext()
        {
            var painted = new List<string>();
            controller.BeforePaint += (s, e) => painted.Add("before");
            controller.AfterPaint += (s, e) => painted.Add("after");
            var draws = 0;
            controller.DrawRequired += () => draws++;
            controller.BCPS = [new BezierControlPoint { Second = new PointF(0, 0) }, new BezierControlPoint { Second = new PointF(100, 0) }];
            Drag(0, 0, 0, 0, Alt);
            Drag(1, 0, 30, 0, Alt);

            var normal = new Recorder();
            controller.Draw(normal);
            // 点（四角）、ハンドルの線、曲線（細かい線の集まり）を描く
            Assert.Contains("fill", normal.Calls);
            Assert.True(normal.Calls.Count(c => c == "line") > 10, string.Join(",", normal.Calls));

            controller.StartTransform();
            var transform = new Recorder();
            controller.Draw(transform);
            Assert.Equal("lines", transform.Calls[0]);
            Assert.Contains("ellipse", transform.Calls);
            Assert.Equal(["before", "after", "before", "after"], painted);
            Assert.True(draws > 0);
        }
    }
}
