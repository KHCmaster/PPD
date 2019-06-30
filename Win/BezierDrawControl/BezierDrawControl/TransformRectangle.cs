using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BezierDrawControl
{
    class TransformRectangle
    {
        public enum ExpandType
        {
            TopLeft = 0,
            TopRight = 1,
            BottomLeft = 2,
            BottomRight = 3,
            Top = 4,
            Left = 5,
            Right = 6,
            Bottom = 7
        }
        [Flags]
        private enum ScaleType
        {
            None = 0,
            X = 1,
            Y = 2,
            FixAspect = 4
        }
        PointF topLeft;
        PointF topRight;
        PointF bottomLeft;
        PointF bottomRight;
        float rotation;
        float scaleX;
        float scaleY;
        float moveX;
        float moveY;

        public TransformRectangle(PointF topLeft, PointF topRight, PointF bottomLeft, PointF bottomRight)
        {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomLeft = bottomLeft;
            this.bottomRight = bottomRight;
            rotation = 0;
            scaleX = 1;
            scaleY = 1;
            moveX = 0;
            moveY = 0;
        }
        public PointF TopLeft
        {
            get
            {
                return GetTransFormed(topLeft);
            }
        }
        public PointF TopRight
        {
            get
            {
                return GetTransFormed(topRight);
            }
        }
        public PointF BottomLeft
        {
            get
            {
                return GetTransFormed(bottomLeft);
            }
        }
        public PointF BottomRight
        {
            get
            {
                return GetTransFormed(bottomRight);
            }
        }
        private PointF GetMiddlePoint(PointF p1, PointF p2)
        {
            return new PointF((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        }
        public PointF TopMiddle
        {
            get
            {
                return GetTransFormed(GetMiddlePoint(topLeft, topRight));
            }
        }
        public PointF LeftMiddle
        {
            get
            {
                return GetTransFormed(GetMiddlePoint(topLeft, bottomLeft));
            }
        }
        public PointF RightMiddle
        {
            get
            {
                return GetTransFormed(GetMiddlePoint(topRight, bottomRight));
            }
        }
        public PointF BottomMiddle
        {
            get
            {
                return GetTransFormed(GetMiddlePoint(bottomLeft, bottomRight));
            }
        }
        public PointF Center
        {
            get
            {
                return GetTransFormed(DefaultCenter);
            }
        }
        public PointF DefaultCenter
        {
            get
            {
                return GetMiddlePoint(topLeft, bottomRight);
            }
        }
        public PointF Scale
        {
            get
            {
                return new PointF(scaleX, scaleY);
            }
            set
            {
                scaleX = value.X;
                scaleY = value.Y;
            }
        }
        public float Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
                rotation = NormalizeAngle(rotation);
            }
        }
        public PointF Move
        {
            get
            {
                return new PointF(moveX, moveY);
            }
            set
            {
                moveX = value.X;
                moveY = value.Y;
            }
        }
        private float NormalizeAngle(float rotation)
        {
            var val = Math.IEEERemainder(rotation, 360);
            if (val < 0) val += 360;
            return (float)val;
        }
        private PointF GetTransFormed(PointF p)
        {
            var mat = new Matrix();
            mat.Translate(moveX, moveY);
            mat.RotateAt(rotation, DefaultCenter);
            mat.Translate(DefaultCenter.X, DefaultCenter.Y);
            mat.Scale(scaleX, scaleY);
            mat.Translate(-DefaultCenter.X, -DefaultCenter.Y);
            var temp = new PointF[] { p };
            mat.TransformPoints(temp);
            return temp[0];
        }
        public void Expand(ExpandType type, PointF p, bool fixAspect)
        {
            switch (type)
            {
                case ExpandType.Bottom:
                    ExpandBottom(p);
                    break;
                case ExpandType.BottomLeft:
                    ExpandBottomLeft(p, fixAspect);
                    break;
                case ExpandType.BottomRight:
                    ExpandBottomRight(p, fixAspect);
                    break;
                case ExpandType.Left:
                    ExpandLeft(p);
                    break;
                case ExpandType.Right:
                    ExpandRight(p);
                    break;
                case ExpandType.Top:
                    ExpandTop(p);
                    break;
                case ExpandType.TopLeft:
                    ExpandTopLeft(p, fixAspect);
                    break;
                case ExpandType.TopRight:
                    ExpandTopRight(p, fixAspect);
                    break;
            }
        }
        private PointF GetBeforeRotate(PointF p)
        {
            var mat = new Matrix();
            mat.Rotate(-rotation);
            var temp = new PointF[] { p };
            mat.TransformPoints(temp);
            return temp[0];
        }
        private void ScaleAtPoint(PointF basePoint, PointF p, ScaleType type)
        {
            PointF center = Center;
            var before = GetBeforeRotate(new PointF(p.X - center.X, p.Y - center.Y));
            var bp = new PointF(basePoint.X - DefaultCenter.X, basePoint.Y - DefaultCenter.Y);
            if ((type & ScaleType.FixAspect) == ScaleType.FixAspect)
            {
                float ratio = scaleY == 0 ? 0 : scaleX / scaleY;
                scaleY = bp.Y == 0 ? 0 : before.Y / bp.Y;
                scaleX = ratio * scaleY;
            }
            else
            {
                if ((type & ScaleType.X) == ScaleType.X) scaleX = bp.X == 0 ? 0 : before.X / bp.X;
                if ((type & ScaleType.Y) == ScaleType.Y) scaleY = bp.Y == 0 ? 0 : before.Y / bp.Y;
            }
        }
        private void ExpandTopLeft(PointF p, bool fixAspect)
        {
            ScaleAtPoint(topLeft, p, fixAspect ? ScaleType.FixAspect : (ScaleType.X | ScaleType.Y));
        }
        private void ExpandTopRight(PointF p, bool fixAspect)
        {
            ScaleAtPoint(topRight, p, fixAspect ? ScaleType.FixAspect : (ScaleType.X | ScaleType.Y));
        }
        private void ExpandBottomLeft(PointF p, bool fixAspect)
        {
            ScaleAtPoint(bottomLeft, p, fixAspect ? ScaleType.FixAspect : (ScaleType.X | ScaleType.Y));
        }
        private void ExpandBottomRight(PointF p, bool fixAspect)
        {
            ScaleAtPoint(bottomRight, p, fixAspect ? ScaleType.FixAspect : (ScaleType.X | ScaleType.Y));
        }
        private void ExpandTop(PointF p)
        {
            ScaleAtPoint(GetMiddlePoint(topLeft, topRight), p, ScaleType.Y);
        }
        private void ExpandLeft(PointF p)
        {
            ScaleAtPoint(GetMiddlePoint(topLeft, bottomLeft), p, ScaleType.X);
        }
        private void ExpandRight(PointF p)
        {
            ScaleAtPoint(GetMiddlePoint(topRight, bottomRight), p, ScaleType.X);
        }
        private void ExpandBottom(PointF p)
        {
            ScaleAtPoint(GetMiddlePoint(bottomLeft, bottomRight), p, ScaleType.Y);
        }
    }
}
