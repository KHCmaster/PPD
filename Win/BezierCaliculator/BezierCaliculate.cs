using System;
using System.Collections.Generic;
using System.Drawing;

namespace BezierCaliculator
{
    /// <summary>
    /// ベジエ曲線汎用関数クラス
    /// </summary>
    public static class BezierCaliculate
    {
        /// <summary>
        /// ４頂点から指定した割合の位置を得る
        /// </summary>
        /// <param name="p1">頂点１</param>
        /// <param name="p2">頂点２</param>
        /// <param name="p3">頂点３</param>
        /// <param name="p4">頂点４</param>
        /// <param name="t">割合(0~1)</param>
        /// <returns></returns>
        public static PointF GetBezeirPoint(ref PointF p1, ref PointF p2, ref PointF p3, ref PointF p4, ref float t)
        {
            PointF p5, p6, p7, p8, p9, p10;
            p5 = CreateDevisionPoint(ref p1, ref p2, ref t);
            p6 = CreateDevisionPoint(ref p2, ref p3, ref t);
            p7 = CreateDevisionPoint(ref p3, ref p4, ref t);
            p8 = CreateDevisionPoint(ref p5, ref p6, ref t);
            p9 = CreateDevisionPoint(ref p6, ref p7, ref t);
            p10 = CreateDevisionPoint(ref p8, ref p9, ref t);
            return p10;
        }
        private static PointF CreateDevisionPoint(ref PointF p1, ref PointF p2, ref float t)
        {
            float invt = 1 - t;
            return new PointF(p1.X * t + p2.X * invt, p1.Y * t + p2.Y * invt);
        }
        /// <summary>
        /// ２頂点で指定されたベジエの長さを得る
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <returns></returns>
        public static int AboutBezeirCount(ref PointF p1, ref PointF p2, ref PointF p3, ref PointF p4)
        {
            var l1 = GetLength(new PointF(p2.X - p1.X, p2.Y - p1.Y));
            var l2 = GetLength(new PointF(p3.X - p2.X, p3.Y - p2.Y));
            var l3 = GetLength(new PointF(p4.X - p3.X, p4.Y - p3.Y));
            return (int)(l1 + l2 + l3);
        }
        private static float GetLength(PointF p1)
        {
            var length = (float)(Math.Sqrt(p1.X * p1.X + p1.Y * p1.Y));
            return length;
        }
        /// <summary>
        /// ベジエ曲線状に乗っているか調べる
        /// </summary>
        /// <param name="p1">頂点１</param>
        /// <param name="p2">頂点２</param>
        /// <param name="p3">頂点３</param>
        /// <param name="p4">頂点４</param>
        /// <param name="target">調べる対象</param>
        /// <param name="outt">乗っていた時の割合(0~1)</param>
        /// <returns></returns>
        public static bool OnBezeier(PointF p1, PointF p2, PointF p3, PointF p4, PointF target, out float outt)
        {
            PointF lastbezierpoint, bezierpoint;
            lastbezierpoint = p1;
            outt = -1;
            float best = float.MaxValue;
            var count = BezierCaliculate.AboutBezeirCount(ref p1, ref p2, ref p3, ref p4);
            for (int i = count; i >= 0; i--)
            {
                float t = (float)i / count;
                bezierpoint = BezierCaliculate.GetBezeirPoint(ref p1, ref p2, ref p3, ref p4, ref t);
                var temp = GetLength(new PointF(bezierpoint.X - target.X, bezierpoint.Y - target.Y));
                if (temp < best)
                {
                    best = temp;
                    outt = t;
                }
                lastbezierpoint = bezierpoint;
            }
            if (best <= 2)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// ベジエ曲線の長さを得る
        /// </summary>
        /// <param name="bcp1">一つ目のベジエ頂点</param>
        /// <param name="bcp2">二つ目のベジエ頂点</param>
        /// <returns></returns>
        public static float GetBezeierLength(BezierControlPoint bcp1, BezierControlPoint bcp2)
        {
            if (bcp1.ValidThird)
            {
                if (!bcp2.ValidFirst)
                {
                    //second bezier
                    return GetBezeierLength(bcp1.Second, bcp1.Third, bcp2.Second, bcp2.Second);
                }
                else
                {
                    //third bezier
                    return GetBezeierLength(bcp1.Second, bcp1.Third, bcp2.First, bcp2.Second);
                }
            }
            else
            {
                if (!bcp2.ValidFirst)
                {
                    //first bezier
                    return GetLength(new PointF(bcp1.Second.X - bcp2.Second.X, bcp1.Second.Y - bcp2.Second.Y));
                }
                else
                {
                    //second bezier
                    return GetBezeierLength(bcp1.Second, bcp1.Second, bcp2.First, bcp2.Second);
                }
            }
        }
        /// <summary>
        /// ベジエ頂点の分割位置と角度を得る
        /// </summary>
        /// <param name="bcp1">ベジエ頂点１</param>
        /// <param name="bcp2">ベジエ頂点２</param>
        /// <param name="length">欲しい長さ</param>
        /// <param name="pos">位置</param>
        /// <param name="direction">向き</param>
        public static void GetBezeirSplitPoint(BezierControlPoint bcp1, BezierControlPoint bcp2, float length, out PointF pos, out PointF direction)
        {
            if (bcp1.ValidThird)
            {
                if (!bcp2.ValidFirst)
                {
                    //second bezier
                    GetBezeirSplitPoint(bcp1.Second, bcp1.Third, bcp2.Second, bcp2.Second, length, out pos, out direction);
                    return;
                }
                else
                {
                    //third bezier
                    GetBezeirSplitPoint(bcp1.Second, bcp1.Third, bcp2.First, bcp2.Second, length, out pos, out direction);
                    return;
                }
            }
            else
            {
                if (!bcp2.ValidFirst)
                {
                    //first bezier
                    var nv = GetNormalizePoint(new PointF(bcp2.Second.X - bcp1.Second.X, bcp2.Second.Y - bcp1.Second.Y));
                    var vec = new PointF(nv.X * length, nv.Y * length);
                    pos = new PointF(bcp1.Second.X + vec.X, bcp1.Second.Y + vec.Y);
                    direction = nv;
                    return;
                }
                else
                {
                    //second bezier
                    GetBezeirSplitPoint(bcp1.Second, bcp1.Second, bcp2.First, bcp2.Second, length, out pos, out direction);
                    return;
                }
            }
        }
        private static float GetBezeierLength(PointF p1, PointF p2, PointF p3, PointF p4)
        {
            PointF lastbezierpoint, bezierpoint;
            lastbezierpoint = p1;
            float sum = 0;
            var count = BezierCaliculate.AboutBezeirCount(ref p1, ref p2, ref p3, ref p4);
            if (count == 0) return 0;
            for (int i = count; i >= 0; i--)
            {
                float t = (float)i / count;
                bezierpoint = BezierCaliculate.GetBezeirPoint(ref p1, ref p2, ref p3, ref p4, ref t);
                sum += GetLength(new PointF(bezierpoint.X - lastbezierpoint.X, bezierpoint.Y - lastbezierpoint.Y));
                lastbezierpoint = bezierpoint;
            }
            return sum;
        }
        /// <summary>
        /// 正規化した頂点を得る
        /// </summary>
        /// <param name="p1"></param>
        /// <returns></returns>
        public static PointF GetNormalizePoint(PointF p1)
        {
            var length = (float)(Math.Sqrt(p1.X * p1.X + p1.Y * p1.Y));
            if (length == 0)
            {
                return new PointF(0, 0);
            }
            else
            {
                return new PointF(p1.X / length, p1.Y / length);
            }
        }
        private static void GetBezeirSplitPoint(PointF p1, PointF p2, PointF p3, PointF p4, float length, out PointF pos, out PointF direction)
        {
            PointF lastbezierpoint, bezierpoint;
            lastbezierpoint = p1;
            pos = p1;
            direction = new PointF(1, 0);
            float sum = 0;
            var count = BezierCaliculate.AboutBezeirCount(ref p1, ref p2, ref p3, ref p4);
            for (int i = count; i >= 0; i--)
            {
                float t = (float)i / count;
                bezierpoint = BezierCaliculate.GetBezeirPoint(ref p1, ref p2, ref p3, ref p4, ref t);
                sum += GetLength(new PointF(bezierpoint.X - lastbezierpoint.X, bezierpoint.Y - lastbezierpoint.Y));
                if (sum >= length)
                {
                    pos = bezierpoint;
                    direction = GetNormalizePoint(new PointF(bezierpoint.X - lastbezierpoint.X, bezierpoint.Y - lastbezierpoint.Y));
                    if (direction.X == 0 && direction.Y == 0)
                    {
                        t = (float)(i - 1) / count;
                        lastbezierpoint = bezierpoint;
                        bezierpoint = BezierCaliculate.GetBezeirPoint(ref p1, ref p2, ref p3, ref p4, ref t);
                        direction = GetNormalizePoint(new PointF(bezierpoint.X - lastbezierpoint.X, bezierpoint.Y - lastbezierpoint.Y));
                    }
                    return;
                }
                lastbezierpoint = bezierpoint;
            }
            pos = p4;
            direction = GetNormalizePoint(new PointF(p4.X - lastbezierpoint.X, p4.Y - lastbezierpoint.Y));
            if (direction.X == 0 && direction.Y == 0)
            {
                float t = (float)1 / count;
                bezierpoint = BezierCaliculate.GetBezeirPoint(ref p1, ref p2, ref p3, ref p4, ref t);
                direction = GetNormalizePoint(new PointF(lastbezierpoint.X - bezierpoint.X, lastbezierpoint.Y - bezierpoint.Y));
            }
        }
        /// <summary>
        /// ベジエ曲線を囲む部分を得る
        /// </summary>
        /// <param name="bcp1">ベジエ頂点１</param>
        /// <param name="bcp2">ベジエ頂点２</param>
        /// <param name="minx">最小のx</param>
        /// <param name="maxx">最大のx</param>
        /// <param name="miny">最小のy</param>
        /// <param name="maxy">最大のy</param>
        public static void GetArea(BezierControlPoint bcp1, BezierControlPoint bcp2, out float minx, out float maxx, out float miny, out float maxy)
        {
            if (bcp1.ValidThird)
            {
                if (!bcp2.ValidFirst)
                {
                    //second bezier
                    GetArea(bcp1.Second, bcp1.Third, bcp2.Second, bcp2.Second, out minx, out maxx, out miny, out maxy);
                    return;
                }
                else
                {
                    //third bezier
                    GetArea(bcp1.Second, bcp1.Third, bcp2.First, bcp2.Second, out minx, out maxx, out miny, out maxy);
                    return;
                }
            }
            else
            {
                if (!bcp2.ValidFirst)
                {
                    //first bezier
                    maxx = Math.Max(bcp1.Second.X, bcp2.Second.X);
                    minx = Math.Min(bcp1.Second.X, bcp2.Second.X);
                    maxy = Math.Max(bcp1.Second.Y, bcp2.Second.Y);
                    miny = Math.Min(bcp1.Second.Y, bcp2.Second.Y);
                    return;
                }
                else
                {
                    //second bezier
                    GetArea(bcp1.Second, bcp1.Second, bcp2.First, bcp2.Second, out minx, out maxx, out miny, out maxy);
                    return;
                }
            }
        }
        private static void GetArea(PointF p1, PointF p2, PointF p3, PointF p4, out float minx, out float maxx, out float miny, out float maxy)
        {
            maxx = minx = p1.X;
            maxy = miny = p1.Y;
            foreach (PointF bezierpoint in GetPoints(p1, p2, p3, p4))
            {
                if (bezierpoint.X > maxx) maxx = bezierpoint.X;
                if (bezierpoint.X < minx) minx = bezierpoint.X;
                if (bezierpoint.Y > maxy) maxy = bezierpoint.Y;
                if (bezierpoint.Y < miny) miny = bezierpoint.Y;
            }
        }
        /// <summary>
        /// ４頂点からベジエ曲線上の位置を得る
        /// </summary>
        /// <param name="p1">頂点１</param>
        /// <param name="p2">頂点２</param>
        /// <param name="p3">頂点３</param>
        /// <param name="p4">頂点４</param>
        /// <returns></returns>
        public static IEnumerable<PointF> GetPoints(PointF p1, PointF p2, PointF p3, PointF p4)
        {
            var count = BezierCaliculate.AboutBezeirCount(ref p1, ref p2, ref p3, ref p4);
            for (int i = count; i >= 0; i--)
            {
                float t = (float)i / count;
                yield return BezierCaliculate.GetBezeirPoint(ref p1, ref p2, ref p3, ref p4, ref t);
            }
        }
        /// <summary>
        /// 分割されたベジエを得る
        /// </summary>
        /// <param name="bcp1">ベジエ頂点１</param>
        /// <param name="bcp2">ベジエ頂点２</param>
        /// <param name="t">割合(0~1)</param>
        /// <param name="obcp1">出力ベジエ頂点１</param>
        /// <param name="obcp2">出力ベジエ頂点２</param>
        /// <param name="obcp3">出力ベジエ頂点３</param>
        public static void GetDevidedBeziers(BezierControlPoint bcp1, BezierControlPoint bcp2, float t, out BezierControlPoint obcp1, out BezierControlPoint obcp2, out BezierControlPoint obcp3)
        {
            PointF p1, p2, p3, p4;
            p1 = bcp1.Second;
            if (bcp1.ValidThird)
            {
                p2 = bcp1.Third;
            }
            else
            {
                p2 = bcp1.Second;
            }
            if (bcp2.ValidFirst)
            {
                p3 = bcp2.First;
            }
            else
            {
                p3 = bcp2.Second;
            }
            p4 = bcp2.Second;
            GetFivePoint(p1, p2, p3, p4, out PointF outp1, out PointF outp2, out PointF outp3, out PointF outp4, out PointF outp5, t);
            obcp1 = bcp1.Clone();
            obcp2 = new BezierControlPoint();
            obcp3 = bcp2.Clone();
            if (obcp1.ValidThird)
            {
                obcp1.Third = outp1;
            }
            obcp2.Second = outp3;
            obcp2.First = outp2;
            obcp2.Third = outp4;
            obcp2.ValidFirst = true;
            obcp2.ValidThird = true;
            if (obcp3.ValidFirst)
            {
                obcp3.First = outp5;
            }
        }
        private static void GetFivePoint(PointF p1, PointF p2, PointF p3, PointF p4, out PointF outp1, out PointF outp2, out PointF outp3, out PointF outp4, out PointF outp5, float t)
        {
            outp1 = CreateDevisionPoint(ref p1, ref p2, ref t);
            outp3 = CreateDevisionPoint(ref p2, ref p3, ref t);
            outp5 = CreateDevisionPoint(ref p3, ref p4, ref t);
            outp2 = CreateDevisionPoint(ref outp1, ref outp3, ref t);
            outp4 = CreateDevisionPoint(ref outp3, ref outp5, ref t);
            outp3 = CreateDevisionPoint(ref outp2, ref outp4, ref t);
        }
    }
}
