using System.Drawing;

namespace BezierDrawControl
{
    public interface IBezierDrawContext
    {
        void DrawString(string text, Color color, float fontSize, PointF point);
        void DrawLine(Color color, PointF p1, PointF p2);
        void DrawLines(Color color, PointF[] points);
        void DrawEllipse(Color color, RectangleF rect);
        void DrawRectangle(Color color, float x, float y, float width, float height);
        void FillRectangle(Color color, float x, float y, float width, float height);
    }
}
