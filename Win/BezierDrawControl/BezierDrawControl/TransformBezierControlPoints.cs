using BezierCaliculator;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BezierDrawControl
{
    class TransformBezierControlPoints
    {
        TransformRectangle TransRec;
        BezierControlPoint[] DefaultPoints;
        public TransformBezierControlPoints(BezierControlPoint[] points)
        {
            DefaultPoints = new BezierControlPoint[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                DefaultPoints[i] = points[i].Clone();
            }
            var first = DefaultPoints[0] as BezierControlPoint;
            float top = first.Second.Y;
            float left = first.Second.X;
            float bottom = first.Second.Y;
            float right = first.Second.X;
            for (int i = 1; i < DefaultPoints.Length; i++)
            {
                var temp = DefaultPoints[i] as BezierControlPoint;
                BezierCaliculate.GetArea(first, temp, out float minx, out float maxx, out float miny, out float maxy);
                top = Math.Min(top, miny);
                bottom = Math.Max(bottom, maxy);
                left = Math.Min(left, minx);
                right = Math.Max(right, maxx);
                first = temp;
            }
            TransRec = new TransformRectangle(
                new PointF(left, top),
                new PointF(right, top),
                new PointF(left, bottom),
                new PointF(right, bottom)
            );
        }
        private int Length
        {
            get
            {
                return DefaultPoints == null ? 0 : DefaultPoints.Length;
            }
        }
        public IEnumerable<BezierControlPoint> BCPS
        {
            get
            {
                BezierControlPoint[] ret = new BezierControlPoint[Length];
                for (int i = 0; i < Length; i++)
                {
                    ret[i] = DefaultPoints[i].Clone();
                    GetTransFormed(ret[i]);
                    yield return ret[i];
                }
            }
        }
        public int BCPSCount
        {
            get
            {
                return DefaultPoints.Length;
            }
        }
        public TransformRectangle TransformRectange
        {
            get
            {
                return TransRec;
            }
        }
        private void GetTransFormed(BezierControlPoint bcp)
        {
            var mat = new Matrix();
            mat.Translate(TransRec.Move.X, TransRec.Move.Y);
            mat.RotateAt(TransRec.Rotation, TransRec.DefaultCenter);
            mat.Translate(TransformRectange.DefaultCenter.X, TransformRectange.DefaultCenter.Y);
            mat.Scale(TransRec.Scale.X, TransRec.Scale.Y);
            mat.Translate(-TransformRectange.DefaultCenter.X, -TransformRectange.DefaultCenter.Y);
            var temp = new PointF[] { bcp.Second, bcp.First, bcp.Third };
            mat.TransformPoints(temp);
            bcp.Second = temp[0];
            if (bcp.ValidFirst)
            {
                bcp.First = temp[1];
            }
            if (bcp.ValidThird)
            {
                bcp.Third = temp[2];
            }
        }
        public void Expand(TransformRectangle.ExpandType type, PointF p, bool fixAspect)
        {
            switch (type)
            {
                case TransformRectangle.ExpandType.Bottom:
                    ExpandBottom(p, fixAspect);
                    break;
                case TransformRectangle.ExpandType.BottomLeft:
                    ExpandBottomLeft(p, fixAspect);
                    break;
                case TransformRectangle.ExpandType.BottomRight:
                    ExpandBottomRight(p, fixAspect);
                    break;
                case TransformRectangle.ExpandType.Left:
                    ExpandLeft(p, fixAspect);
                    break;
                case TransformRectangle.ExpandType.Right:
                    ExpandRight(p, fixAspect);
                    break;
                case TransformRectangle.ExpandType.Top:
                    ExpandTop(p, fixAspect);
                    break;
                case TransformRectangle.ExpandType.TopLeft:
                    ExpandTopLeft(p, fixAspect);
                    break;
                case TransformRectangle.ExpandType.TopRight:
                    ExpandTopRight(p, fixAspect);
                    break;
            }
        }
        private void ExpandTopLeft(PointF p, bool fixAspect)
        {
            TransRec.Expand(TransformRectangle.ExpandType.TopLeft, p, fixAspect);
        }
        private void ExpandTopRight(PointF p, bool fixAspect)
        {
            TransRec.Expand(TransformRectangle.ExpandType.TopRight, p, fixAspect);
        }
        private void ExpandBottomLeft(PointF p, bool fixAspect)
        {
            TransRec.Expand(TransformRectangle.ExpandType.BottomLeft, p, fixAspect);
        }
        private void ExpandBottomRight(PointF p, bool fixAspect)
        {
            TransRec.Expand(TransformRectangle.ExpandType.BottomRight, p, fixAspect);
        }
        private void ExpandTop(PointF p, bool fixAspect)
        {
            TransRec.Expand(TransformRectangle.ExpandType.Top, p, fixAspect);
        }
        private void ExpandLeft(PointF p, bool fixAspect)
        {
            TransRec.Expand(TransformRectangle.ExpandType.Left, p, fixAspect);
        }
        private void ExpandRight(PointF p, bool fixAspect)
        {
            TransRec.Expand(TransformRectangle.ExpandType.Right, p, fixAspect);
        }
        private void ExpandBottom(PointF p, bool fixAspect)
        {
            TransRec.Expand(TransformRectangle.ExpandType.Bottom, p, fixAspect);
        }
    }
}
