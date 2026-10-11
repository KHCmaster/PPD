using BezierCaliculator;
using System;
using System.Drawing;
using System.Linq;
using Xunit;

namespace UnitTests.Libraries
{
    public class BezierControlPointTests
    {
        [Fact]
        public void Handles_AreStoredAsDirectionAndLength()
        {
            var bcp = new BezierControlPoint { Second = new PointF(10, 10) };
            Assert.Equal((false, true, false), (bcp.ValidFirst, bcp.ValidSecond, bcp.ValidThird));

            bcp.First = new PointF(13, 14);
            bcp.Third = new PointF(10, 10);

            Assert.Equal(new PointF(0.6f, 0.8f), bcp.FirstDirection);
            Assert.Equal(5, bcp.FirstLength);
            Assert.Equal(new PointF(13, 14), bcp.First);
            // 長さ 0 のハンドルは向きも 0
            Assert.Equal((new PointF(0, 0), 0f), (bcp.ThirdDirection, bcp.ThirdLength));

            // アンカーを動かすと、ハンドルもいっしょに動く
            bcp.Second = new PointF(0, 0);
            Assert.Equal(new PointF(3, 4), bcp.First);
        }

        [Fact]
        public void CloneAndSerialize_RoundTrip()
        {
            var bcp = new BezierControlPoint
            {
                Second = new PointF(1.5f, -2.25f),
                ValidFirst = true,
                ValidSecond = false,
                ValidThird = true,
                First = new PointF(4.5f, 1.75f),
                Third = new PointF(-3, -2.25f)
            };

            var clone = bcp.Clone();
            var text = bcp.Serialize();
            var restored = BezierControlPoint.Deserialize(text);

            foreach (var other in new[] { clone, restored })
            {
                Assert.NotSame(bcp, other);
                Assert.Equal((bcp.ValidFirst, bcp.ValidSecond, bcp.ValidThird), (other.ValidFirst, other.ValidSecond, other.ValidThird));
                Assert.Equal((bcp.First, bcp.Second, bcp.Third), (other.First, other.Second, other.Third));
                Assert.Equal((bcp.FirstLength, bcp.ThirdLength), (other.FirstLength, other.ThirdLength));
            }
            Assert.StartsWith("VF=1 VS=0 VT=1 FDX=", text);
            Assert.Contains("SX=1.5 SY=-2.25", text);
        }

        [Fact]
        public void Deserialize_DefaultsAndErrors()
        {
            var bcp = BezierControlPoint.Deserialize("SX=3 SY=4");
            Assert.Equal(new PointF(3, 4), bcp.Second);
            Assert.Equal(new PointF(1, 0), bcp.FirstDirection);
            Assert.Equal(new PointF(1, 0), bcp.ThirdDirection);

            Assert.Throws<ArgumentException>(() => BezierControlPoint.Deserialize("SX3"));
            Assert.Throws<ArgumentException>(() => BezierControlPoint.Deserialize("XX=1"));
            Assert.Throws<FormatException>(() => BezierControlPoint.Deserialize("SX=a"));
        }
    }

    public class BezierCaliculateTests
    {
        static BezierControlPoint Point(float x, float y, PointF? first = null, PointF? third = null)
        {
            var bcp = new BezierControlPoint { Second = new PointF(x, y) };
            if (first.HasValue)
            {
                bcp.First = first.Value;
                bcp.ValidFirst = true;
            }
            if (third.HasValue)
            {
                bcp.Third = third.Value;
                bcp.ValidThird = true;
            }
            return bcp;
        }

        static void Near(PointF expected, PointF actual, float precision = 0.01f)
        {
            Assert.InRange(actual.X, expected.X - precision, expected.X + precision);
            Assert.InRange(actual.Y, expected.Y - precision, expected.Y + precision);
        }

        [Fact]
        public void GetBezeirPoint_RatioOneIsStart()
        {
            PointF p1 = new(0, 0), p2 = new(0, 100), p3 = new(100, 100), p4 = new(100, 0);
            float one = 1, zero = 0, half = 0.5f;

            Assert.Equal(p1, BezierCaliculate.GetBezeirPoint(ref p1, ref p2, ref p3, ref p4, ref one));
            Assert.Equal(p4, BezierCaliculate.GetBezeirPoint(ref p1, ref p2, ref p3, ref p4, ref zero));
            Assert.Equal(new PointF(50, 75), BezierCaliculate.GetBezeirPoint(ref p1, ref p2, ref p3, ref p4, ref half));
            Assert.Equal(300, BezierCaliculate.AboutBezeirCount(ref p1, ref p2, ref p3, ref p4));

            var points = BezierCaliculate.GetPoints(p1, p2, p3, p4).ToList();
            Assert.Equal(301, points.Count);
            Assert.Equal(p1, points[0]);
            Assert.Equal(p4, points[^1]);
        }

        [Fact]
        public void Length_StraightAndCurved()
        {
            // ハンドルのない 2 点は直線
            Assert.Equal(50, BezierCaliculate.GetBezeierLength(Point(0, 0), Point(30, 40)));

            // ハンドルが直線の上にあるときも、長さは同じ
            var straight = BezierCaliculate.GetBezeierLength(Point(0, 0, third: new PointF(10, 0)), Point(100, 0, first: new PointF(90, 0)));
            Assert.InRange(straight, 99.9f, 100.1f);

            // 半円に近い曲線は直線より長い
            var curved = BezierCaliculate.GetBezeierLength(Point(0, 0, third: new PointF(0, 55)), Point(100, 0, first: new PointF(100, 55)));
            Assert.InRange(curved, 140, 160);

            // 片方だけハンドルがあるときも曲線
            Assert.True(BezierCaliculate.GetBezeierLength(Point(0, 0, third: new PointF(0, 50)), Point(100, 0)) > 100);
            Assert.True(BezierCaliculate.GetBezeierLength(Point(0, 0), Point(100, 0, first: new PointF(100, 50))) > 100);
            Assert.Equal(0, BezierCaliculate.GetBezeierLength(Point(0, 0, third: new PointF(0.1f, 0)), Point(0.2f, 0, first: new PointF(0.1f, 0))));
        }

        [Fact]
        public void SplitPoint_WalksAlongCurve()
        {
            BezierCaliculate.GetBezeirSplitPoint(Point(0, 0), Point(0, 100), 30, out var pos, out var dir);
            Assert.Equal((new PointF(0, 30), new PointF(0, 1)), (pos, dir));

            BezierCaliculate.GetBezeirSplitPoint(Point(0, 0, third: new PointF(10, 0)), Point(100, 0, first: new PointF(90, 0)), 40, out pos, out dir);
            Near(new PointF(40, 0), pos, 1);
            Near(new PointF(1, 0), dir);

            // 長さより先は終わりの点
            BezierCaliculate.GetBezeirSplitPoint(Point(0, 0, third: new PointF(0, 10)), Point(100, 0, first: new PointF(100, 10)), 1000, out pos, out dir);
            Assert.Equal(new PointF(100, 0), pos);
            Near(new PointF(0, -1), dir, 0.1f);

            BezierCaliculate.GetBezeirSplitPoint(Point(0, 0), Point(100, 0, first: new PointF(100, 10)), 10, out pos, out _);
            BezierCaliculate.GetBezeirSplitPoint(Point(0, 0, third: new PointF(0, 10)), Point(100, 0), 10, out var pos2, out _);
            Assert.True(pos.X > 0 && pos2.Y > 0);
        }

        [Fact]
        public void SplitPoint_TinyCurve_HasNoNaN()
        {
            // すべての点が 1 ピクセルより近いと、数える点が 0 になる
            BezierCaliculate.GetBezeirSplitPoint(Point(0, 0, third: new PointF(0.1f, 0)), Point(0.3f, 0, first: new PointF(0.2f, 0)), 0.1f, out var pos, out var dir);

            Assert.False(float.IsNaN(pos.X) || float.IsNaN(pos.Y) || float.IsNaN(dir.X) || float.IsNaN(dir.Y));
        }

        [Fact]
        public void OnBezeier_FindsRatio()
        {
            PointF p1 = new(0, 0), p2 = new(0, 0), p3 = new(100, 0), p4 = new(100, 0);

            Assert.True(BezierCaliculate.OnBezeier(p1, p2, p3, p4, new PointF(50, 1), out var t));
            Assert.InRange(t, 0.4f, 0.6f);
            Assert.False(BezierCaliculate.OnBezeier(p1, p2, p3, p4, new PointF(50, 10), out _));
            // 短い曲線でも、点の上なら見つける
            Assert.True(BezierCaliculate.OnBezeier(new PointF(0, 0), new PointF(0, 0), new PointF(0.5f, 0), new PointF(0.5f, 0), new PointF(0, 0), out _));
        }

        [Fact]
        public void GetArea_ContainsCurve()
        {
            BezierCaliculate.GetArea(Point(0, 0), Point(30, -40), out var minx, out var maxx, out var miny, out var maxy);
            Assert.Equal((0f, 30f, -40f, 0f), (minx, maxx, miny, maxy));

            BezierCaliculate.GetArea(Point(0, 0, third: new PointF(0, 100)), Point(100, 0, first: new PointF(100, 100)), out minx, out maxx, out miny, out maxy);
            Assert.Equal((0f, 100f, 0f), (minx, maxx, miny));
            Assert.InRange(maxy, 74, 76);

            BezierCaliculate.GetArea(Point(0, 0, third: new PointF(0, 100)), Point(100, 0), out _, out _, out _, out maxy);
            Assert.True(maxy > 0);
            BezierCaliculate.GetArea(Point(0, 0), Point(100, 0, first: new PointF(100, -100)), out _, out _, out miny, out _);
            Assert.True(miny < 0);
        }

        [Fact]
        public void GetDevidedBeziers_KeepsShape()
        {
            var a = Point(0, 0, third: new PointF(0, 100));
            var b = Point(100, 0, first: new PointF(100, 100));

            BezierCaliculate.GetDevidedBeziers(a, b, 0.5f, out var o1, out var o2, out var o3);

            // 真ん中で分けた点は、もとの曲線の真ん中
            Near(new PointF(50, 75), o2.Second);
            Assert.True(o2.ValidFirst && o2.ValidThird);
            Assert.Equal(a.Second, o1.Second);
            Assert.Equal(b.Second, o3.Second);
            var whole = BezierCaliculate.GetBezeierLength(a, b);
            var parts = BezierCaliculate.GetBezeierLength(o1, o2) + BezierCaliculate.GetBezeierLength(o2, o3);
            Assert.InRange(parts, whole - 1, whole + 1);

            // ハンドルのない点で分けても、真ん中の点にはハンドルを付ける（割合は終わりの点からなので、0.25 は始めから 3/4 の所）
            BezierCaliculate.GetDevidedBeziers(Point(0, 0), Point(100, 0), 0.25f, out o1, out o2, out o3);
            Assert.Equal(new PointF(84.375f, 0), o2.Second);
            Assert.False(o1.ValidThird || o3.ValidFirst);
            Assert.Equal(new PointF(0.6f, 0.8f), BezierCaliculate.GetNormalizePoint(new PointF(3, 4)));
            Assert.Equal(new PointF(0, 0), BezierCaliculate.GetNormalizePoint(new PointF(0, 0)));
        }
    }
}
