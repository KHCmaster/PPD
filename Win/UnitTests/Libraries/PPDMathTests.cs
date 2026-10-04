using PPDMath;
using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;
using Color4 = Vortice.Mathematics.Color4;
using Dx = SharpDX;

namespace UnitTests.Libraries
{
    /// <summary>
    /// PPDMath（SharpDX から移した計算）が、元の SharpDX と同じ値を返すかを確かめます。
    /// </summary>
    /// <remarks>
    /// 譜面やスクリプトの結果を変えないように、近い値ではなく同じ値（ビットまで同じ）であることを確かめる。
    /// </remarks>
    public class PPDMathTests
    {
        const int Count = 200;

        // 毎回同じ値になるように、種を決めた乱数を使う
        static IEnumerable<Random> Randoms()
        {
            var random = new Random(12345);
            for (int i = 0; i < Count; i++)
            {
                yield return random;
            }
        }

        static float F(Random r, float range = 10) => (float)(r.NextDouble() * 2 - 1) * range;
        static Vector2 V2(Random r) => new(F(r), F(r));
        static Vector3 V3(Random r) => new(F(r), F(r), F(r));
        static Vector4 V4(Random r) => new(F(r), F(r), F(r), F(r));
        static Quaternion Q(Random r) => new(F(r, 1), F(r, 1), F(r, 1), F(r, 1));

        static Matrix4x4 M(Random r) => new(F(r), F(r), F(r), F(r), F(r), F(r), F(r), F(r), F(r), F(r), F(r), F(r), F(r), F(r), F(r), F(r));

        static Dx.Vector2 Dx2(Vector2 v) => new(v.X, v.Y);
        static Dx.Vector3 Dx3(Vector3 v) => new(v.X, v.Y, v.Z);
        static Dx.Vector4 Dx4(Vector4 v) => new(v.X, v.Y, v.Z, v.W);
        static Dx.Quaternion DxQ(Quaternion q) => new(q.X, q.Y, q.Z, q.W);

        static Dx.Matrix DxM(Matrix4x4 m) => new(m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44);

        static void Same(Dx.Matrix expected, Matrix4x4 actual)
        {
            Assert.Equal(expected.ToArray(), new[] { actual.M11, actual.M12, actual.M13, actual.M14, actual.M21, actual.M22, actual.M23, actual.M24, actual.M31, actual.M32, actual.M33, actual.M34, actual.M41, actual.M42, actual.M43, actual.M44 });
        }

        static void Same(Dx.Vector2 expected, Vector2 actual) => Assert.Equal((expected.X, expected.Y), (actual.X, actual.Y));

        static void Same(Dx.Vector3 expected, Vector3 actual) => Assert.Equal((expected.X, expected.Y, expected.Z), (actual.X, actual.Y, actual.Z));

        static void Same(Dx.Vector4 expected, Vector4 actual) => Assert.Equal((expected.X, expected.Y, expected.Z, expected.W), (actual.X, actual.Y, actual.Z, actual.W));

        static void Same(Dx.Quaternion expected, Quaternion actual) => Assert.Equal((expected.X, expected.Y, expected.Z, expected.W), (actual.X, actual.Y, actual.Z, actual.W));

        [Fact]
        public void Multiply_AndDivide()
        {
            foreach (var r in Randoms())
            {
                var a = M(r);
                var b = M(r);
                var s = F(r);
                Same(DxM(a) * DxM(b), MathCompat.Multiply(a, b));
                Same(DxM(a) / s, a / s);
                Assert.Equal(DxM(a).Determinant(), a.Determinant());
            }
        }

        [Fact]
        public void RowsAndColumns()
        {
            var m = M(new Random(1));
            var dx = DxM(m);

            Same(dx.Row1, m.Row1);
            Same(dx.Row2, m.Row2);
            Same(dx.Row3, m.Row3);
            Same(dx.Row4, m.Row4);
            Same(dx.Column1, m.Column1);
            Same(dx.Column2, m.Column2);
            Same(dx.Column3, m.Column3);
            Same(dx.Column4, m.Column4);
        }

        [Fact]
        public void RotationScalingTranslation()
        {
            foreach (var r in Randoms())
            {
                var angle = F(r, 7);
                var v = V3(r);
                var q = MathCompat.NormalizeSharpDX(Q(r));
                Same(Dx.Matrix.RotationX(angle), Matrix4x4.RotationX(angle));
                Same(Dx.Matrix.RotationY(angle), Matrix4x4.RotationY(angle));
                Same(Dx.Matrix.RotationZ(angle), Matrix4x4.RotationZ(angle));
                Same(Dx.Matrix.RotationAxis(Dx3(Vector3.Normalize(v)), angle), Matrix4x4.RotationAxis(Vector3.Normalize(v), angle));
                Same(Dx.Matrix.RotationQuaternion(DxQ(q)), Matrix4x4.RotationQuaternion(q));
                Same(Dx.Matrix.Scaling(v.X, v.Y, v.Z), Matrix4x4.Scaling(v.X, v.Y, v.Z));
                Same(Dx.Matrix.Scaling(Dx3(v)), Matrix4x4.Scaling(v));
                Same(Dx.Matrix.Scaling(angle), Matrix4x4.Scaling(angle));
                Same(Dx.Matrix.Translation(v.X, v.Y, v.Z), MathCompat.Translation(v.X, v.Y, v.Z));
                Same(Dx.Matrix.Translation(Dx3(v)), MathCompat.Translation(v));
            }
        }

        [Fact]
        public void Transformations()
        {
            foreach (var r in Randoms())
            {
                Vector2 a = V2(r), b = V2(r), c = V2(r), d = V2(r);
                float s = F(r), t = F(r, 7), u = F(r, 7);
                Vector3 e = V3(r), f = V3(r), g = V3(r), h = V3(r);
                Quaternion q1 = MathCompat.NormalizeSharpDX(Q(r)), q2 = MathCompat.NormalizeSharpDX(Q(r));
                Same(Dx.Matrix.Transformation2D(Dx2(a), t, Dx2(b), Dx2(c), u, Dx2(d)), Matrix4x4.Transformation2D(a, t, b, c, u, d));
                Same(Dx.Matrix.AffineTransformation2D(s, t, Dx2(a)), Matrix4x4.AffineTransformation2D(s, t, a));
                Same(Dx.Matrix.AffineTransformation2D(s, Dx2(a), t, Dx2(b)), Matrix4x4.AffineTransformation2D(s, a, t, b));
                Same(Dx.Matrix.Transformation(Dx3(e), DxQ(q1), Dx3(f), Dx3(g), DxQ(q2), Dx3(h)), Matrix4x4.Transformation(e, q1, f, g, q2, h));
                Same(Dx.Matrix.AffineTransformation(s, DxQ(q1), Dx3(e)), Matrix4x4.AffineTransformation(s, q1, e));
                Same(Dx.Matrix.AffineTransformation(s, Dx3(e), DxQ(q1), Dx3(f)), Matrix4x4.AffineTransformation(s, e, q1, f));
            }
        }

        [Fact]
        public void VectorTransforms()
        {
            foreach (var r in Randoms())
            {
                var m = M(r);
                var v2 = V2(r);
                var v3 = V3(r);
                Same(Dx.Vector2.Transform(Dx2(v2), DxM(m)), MathCompat.TransformToVector4(v2, m));
                Same(Dx.Vector3.Transform(Dx3(v3), DxM(m)), MathCompat.TransformToVector4(v3, m));
                Same(Dx.Vector2.TransformCoordinate(Dx2(v2), DxM(m)), Vector2.TransformCoordinate(v2, m));
                Same(Dx.Vector3.TransformCoordinate(Dx3(v3), DxM(m)), Vector3.TransformCoordinate(v3, m));
            }
        }

        [Fact]
        public void NormalizeAndClamp()
        {
            foreach (var r in Randoms())
            {
                Vector2 v2 = V2(r), min2 = V2(r), max2 = V2(r);
                Vector3 v3 = V3(r), min3 = V3(r), max3 = V3(r);
                Vector4 v4 = V4(r), min4 = V4(r), max4 = V4(r);
                var q = Q(r);
                Same(Dx.Vector2.Normalize(Dx2(v2)), MathCompat.NormalizeSharpDX(v2));
                Same(Dx.Vector3.Normalize(Dx3(v3)), MathCompat.NormalizeSharpDX(v3));
                Same(Dx.Vector4.Normalize(Dx4(v4)), MathCompat.NormalizeSharpDX(v4));
                Same(Dx.Quaternion.Normalize(DxQ(q)), MathCompat.NormalizeSharpDX(q));
                // 最小が最大より大きいときも同じ結果にする
                Same(Dx.Vector2.Clamp(Dx2(v2), Dx2(min2), Dx2(max2)), MathCompat.ClampSharpDX(v2, min2, max2));
                Same(Dx.Vector3.Clamp(Dx3(v3), Dx3(min3), Dx3(max3)), MathCompat.ClampSharpDX(v3, min3, max3));
                Same(Dx.Vector4.Clamp(Dx4(v4), Dx4(min4), Dx4(max4)), MathCompat.ClampSharpDX(v4, min4, max4));
            }
            // 長さが 0 に近いときはそのまま
            Same(Dx.Vector2.Normalize(new Dx.Vector2(1e-8f, 0)), MathCompat.NormalizeSharpDX(new Vector2(1e-8f, 0)));
            Same(Dx.Vector3.Normalize(Dx.Vector3.Zero), MathCompat.NormalizeSharpDX(Vector3.Zero));
            Same(Dx.Vector4.Normalize(Dx.Vector4.Zero), MathCompat.NormalizeSharpDX(Vector4.Zero));
            Same(Dx.Quaternion.Normalize(new Dx.Quaternion(0, 0, 0, 0)), MathCompat.NormalizeSharpDX(new Quaternion(0, 0, 0, 0)));
        }

        [Fact]
        public void Quaternions()
        {
            foreach (var r in Randoms())
            {
                var q = Q(r);
                var unit = MathCompat.NormalizeSharpDX(q);
                var axis = V3(r);
                var angle = F(r, 7);
                Same(Dx.Quaternion.Exponential(DxQ(q)), Quaternion.Exponential(q));
                Same(Dx.Quaternion.Logarithm(DxQ(unit)), Quaternion.Logarithm(unit));
                Same(Dx.Quaternion.RotationAxis(Dx3(axis), angle), Quaternion.RotationAxis(axis, angle));
                Assert.Equal(DxQ(unit).Angle, unit.Angle);
                Same(DxQ(unit).Axis, unit.Axis);

                // 回転の行列のいろいろな場合（trace が正、それぞれの軸が大きいとき）
                var rotation = Matrix4x4.RotationAxis(Vector3.Normalize(axis), angle);
                Same(Dx.Quaternion.RotationMatrix(DxM(rotation)), Quaternion.RotationMatrix(rotation));
            }
            foreach (var m in new[] { Matrix4x4.RotationX(3), Matrix4x4.RotationY(3), Matrix4x4.RotationZ(3), Matrix4x4.Scaling(-1, 2, -3), Matrix4x4.Scaling(-1, -2, 3) })
            {
                Same(Dx.Quaternion.RotationMatrix(DxM(m)), Quaternion.RotationMatrix(m));
            }
            // 特別な値
            var identity = Quaternion.Identity;
            Same(Dx.Quaternion.Exponential(Dx.Quaternion.Identity), Quaternion.Exponential(identity));
            Same(Dx.Quaternion.Logarithm(Dx.Quaternion.Identity), Quaternion.Logarithm(identity));
            Same(Dx.Quaternion.Logarithm(new Dx.Quaternion(1, 0, 0, 2)), Quaternion.Logarithm(new Quaternion(1, 0, 0, 2)));
            Assert.Equal(Dx.Quaternion.Identity.Angle, identity.Angle);
            Same(Dx.Quaternion.Identity.Axis, identity.Axis);
        }

        [Fact]
        public void Colors()
        {
            foreach (var r in Randoms())
            {
                var rgba = r.Next(int.MinValue, int.MaxValue);
                var expected = new Dx.Color4(rgba);
                var actual = MathCompat.Color4FromRgba(rgba);
                Assert.Equal((expected.Red, expected.Green, expected.Blue, expected.Alpha), (actual.Red, actual.Green, actual.Blue, actual.Alpha));
                Assert.Equal(expected.ToRgba(), actual.ToRgbaInt());
                Assert.Equal(expected.ToBgra(), actual.ToBgraInt());

                // 0 から 1 の外の値も、SharpDX と同じく切り捨てて下の 8 ビットにする
                var color = new Color4(F(r, 2), F(r, 2), F(r, 2), F(r, 2));
                var dx = new Dx.Color4(color.R, color.G, color.B, color.A);
                Assert.Equal(dx.ToRgba(), color.ToRgbaInt());
                Assert.Equal(dx.ToBgra(), color.ToBgraInt());
            }
        }
    }
}
