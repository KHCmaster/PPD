using System.Drawing;
using System.Linq;

namespace BezierDrawControl
{
    /// <summary>
    /// 中心（<see cref="BezierController.Center"/>）からの位置で描くための描画先です。描く位置に中心を足します。
    /// </summary>
    /// <remarks>
    /// マウスの位置は中心からの位置に直して扱うので、点や線も中心からの位置で描く。
    /// </remarks>
    sealed class CenteredDrawContext(IBezierDrawContext context, Point center) : IBezierDrawContext
    {
        private PointF Shift(PointF point)
        {
            return new PointF(point.X + center.X, point.Y + center.Y);
        }

        public void DrawString(string text, Color color, float fontSize, PointF point)
        {
            context.DrawString(text, color, fontSize, Shift(point));
        }

        public void DrawLine(Color color, PointF p1, PointF p2)
        {
            context.DrawLine(color, Shift(p1), Shift(p2));
        }

        public void DrawLines(Color color, PointF[] points)
        {
            context.DrawLines(color, [.. points.Select(Shift)]);
        }

        public void DrawEllipse(Color color, RectangleF rect)
        {
            context.DrawEllipse(color, new RectangleF(Shift(rect.Location), rect.Size));
        }

        public void DrawRectangle(Color color, float x, float y, float width, float height)
        {
            context.DrawRectangle(color, x + center.X, y + center.Y, width, height);
        }

        public void FillRectangle(Color color, float x, float y, float width, float height)
        {
            context.FillRectangle(color, x + center.X, y + center.Y, width, height);
        }
    }
}
