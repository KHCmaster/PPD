using SharpDX;
using System;
using System.Collections.Generic;

namespace PPDCore
{
    class XComparer : IComparer<Vector2>
    {
        static XComparer comparer = new XComparer();
        public static XComparer Comparer
        {
            get
            {
                return comparer;
            }
        }
        #region IComparer<PointF> メンバ

        public int Compare(Vector2 x, Vector2 y)
        {
            if (x.X - y.X < 0) return -1;
            else if (x.X - y.X > 0) return 1;
            else return 0;
        }

        #endregion
    }
    class ConvexHull
    {
        public static Vector2[] Convex_Hull(Vector2[] ps)
        {
            int n = ps.Length, k = 0;
            Array.Sort(ps, XComparer.Comparer);
            Vector2[] ch = new Vector2[n * 2];
            for (int i = 0; i < n; ch[k++] = ps[i++]) // lower-hull
                while (k >= 2 && ccw(ch[k - 2], ch[k - 1], ps[i]) <= 0) --k;
            for (int i = n - 2, t = k + 1; i >= 0; ch[k++] = ps[i--]) // upper-hull
                while (k >= t && ccw(ch[k - 2], ch[k - 1], ps[i]) <= 0) --k;
            Array.Resize(ref ch, k - 1);
            return ch;
        }

        static int ccw(Vector2 a, Vector2 b, Vector2 c)
        {
            b -= a; c -= a;
            if (Cross(b, c) > 0) return +1;       // counter clockwise
            if (Cross(b, c) < 0) return -1;       // clockwise
            if (Dot(b, c) < 0) return +2;       // c--a--b on line
            if (Norm(b) < Norm(c)) return -2;       // a--b--c on line
            return 0;
        }

        static float Cross(Vector2 p1, Vector2 p2)
        {
            return p1.X * p2.Y - p1.Y * p2.X;
        }

        static float Dot(Vector2 p1, Vector2 p2)
        {
            return p1.X * p2.X + p1.Y * p2.Y;
        }

        static float Norm(Vector2 p)
        {
            return (float)Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }
    }
}
