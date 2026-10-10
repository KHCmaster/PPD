using System;
using System.Numerics;
using Vortice.Mathematics;

namespace PPDMath
{
    /// <summary>
    /// SharpDX.Mathematics にあって System.Numerics / Vortice.Mathematics にないメンバーを、同じ名前・同じ計算で提供します。
    /// 計算は SharpDX 4.2.0 の実装を移したものです。結果を変えないため、三角関数は Math（double）で計算し、
    /// 行列の積も SharpDX と同じ順序のスカラー演算（<see cref="Multiply"/>）で行います。
    /// </summary>
    public static class MathCompat
    {
        /// <summary>SharpDX の MathUtil.ZeroTolerance と同じ値です。</summary>
        public const float ZeroTolerance = 1E-06f;

        internal static bool IsZero(float a)
        {
            return Math.Abs(a) < ZeroTolerance;
        }

        /// <summary>
        /// 行列の積を SharpDX と同じ計算順序で求めます（System.Numerics の * 演算子は SIMD のため、最下位ビットが異なることがある）。
        /// </summary>
        public static Matrix4x4 Multiply(Matrix4x4 left, Matrix4x4 right)
        {
            return new Matrix4x4(
                left.M11 * right.M11 + left.M12 * right.M21 + left.M13 * right.M31 + left.M14 * right.M41,
                left.M11 * right.M12 + left.M12 * right.M22 + left.M13 * right.M32 + left.M14 * right.M42,
                left.M11 * right.M13 + left.M12 * right.M23 + left.M13 * right.M33 + left.M14 * right.M43,
                left.M11 * right.M14 + left.M12 * right.M24 + left.M13 * right.M34 + left.M14 * right.M44,
                left.M21 * right.M11 + left.M22 * right.M21 + left.M23 * right.M31 + left.M24 * right.M41,
                left.M21 * right.M12 + left.M22 * right.M22 + left.M23 * right.M32 + left.M24 * right.M42,
                left.M21 * right.M13 + left.M22 * right.M23 + left.M23 * right.M33 + left.M24 * right.M43,
                left.M21 * right.M14 + left.M22 * right.M24 + left.M23 * right.M34 + left.M24 * right.M44,
                left.M31 * right.M11 + left.M32 * right.M21 + left.M33 * right.M31 + left.M34 * right.M41,
                left.M31 * right.M12 + left.M32 * right.M22 + left.M33 * right.M32 + left.M34 * right.M42,
                left.M31 * right.M13 + left.M32 * right.M23 + left.M33 * right.M33 + left.M34 * right.M43,
                left.M31 * right.M14 + left.M32 * right.M24 + left.M33 * right.M34 + left.M34 * right.M44,
                left.M41 * right.M11 + left.M42 * right.M21 + left.M43 * right.M31 + left.M44 * right.M41,
                left.M41 * right.M12 + left.M42 * right.M22 + left.M43 * right.M32 + left.M44 * right.M42,
                left.M41 * right.M13 + left.M42 * right.M23 + left.M43 * right.M33 + left.M44 * right.M43,
                left.M41 * right.M14 + left.M42 * right.M24 + left.M43 * right.M34 + left.M44 * right.M44);
        }

        private static Matrix4x4 Mul(params ReadOnlySpan<Matrix4x4> matrices)
        {
            var result = matrices[0];
            for (var i = 1; i < matrices.Length; i++)
            {
                result = Multiply(result, matrices[i]);
            }
            return result;
        }

        /// <summary>
        /// RGBA の 32 ビット整数から色を作ります（SharpDX の new Color4(int rgba) と同じ。各成分を 255 で割る）。
        /// </summary>
        public static Color4 Color4FromRgba(int rgba)
        {
            return new Color4(
                (float)(rgba & 0xFF) / 255f,
                (float)((rgba >> 8) & 0xFF) / 255f,
                (float)((rgba >> 16) & 0xFF) / 255f,
                (float)((rgba >> 24) & 0xFF) / 255f);
        }

        /// <summary>
        /// ベクトルを正規化します（SharpDX の Vector2.Normalize と同じ。長さが 0 に近いときはそのまま返す）。
        /// System.Numerics には同じ名前の静的メソッドがあるため、インスタンスの Normalize() は使えない。
        /// </summary>
        public static Vector2 NormalizeSharpDX(Vector2 value)
        {
            var length = (float)Math.Sqrt(value.X * value.X + value.Y * value.Y);
            if (IsZero(length))
            {
                return value;
            }
            var inv = 1f / length;
            return new Vector2(value.X * inv, value.Y * inv);
        }

        private static Vector3 ToVector3(Vector2 value)
        {
            // SharpDX の (Vector3)Vector2 と同じ（Z = 0）
            return new Vector3(value.X, value.Y, 0f);
        }

        /// <summary>
        /// 平行移動の行列を作ります（SharpDX の Matrix.Translation）。
        /// System.Numerics の Matrix4x4 には同じ名前のプロパティ（Translation）があるため、拡張メンバーにできない。
        /// </summary>
        public static Matrix4x4 Translation(float x, float y, float z)
        {
            var result = Matrix4x4.Identity;
            result.M41 = x;
            result.M42 = y;
            result.M43 = z;
            return result;
        }

        /// <summary>平行移動の行列を作ります（SharpDX の Matrix.Translation）。</summary>
        public static Matrix4x4 Translation(Vector3 value)
        {
            return Translation(value.X, value.Y, value.Z);
        }

        /// <summary>
        /// ベクトルを行列で変換し、w を含む結果を返します（SharpDX の Vector2.Transform(Vector2, Matrix)）。
        /// System.Numerics の Vector2.Transform は Vector2 を返すので、名前を変えている。
        /// </summary>
        public static Vector4 TransformToVector4(Vector2 vector, Matrix4x4 transform)
        {
            return new Vector4(
                vector.X * transform.M11 + vector.Y * transform.M21 + transform.M41,
                vector.X * transform.M12 + vector.Y * transform.M22 + transform.M42,
                vector.X * transform.M13 + vector.Y * transform.M23 + transform.M43,
                vector.X * transform.M14 + vector.Y * transform.M24 + transform.M44);
        }

        /// <summary>ベクトルを行列で変換し、w を含む結果を返します（SharpDX の Vector3.Transform(Vector3, Matrix)）。</summary>
        public static Vector4 TransformToVector4(Vector3 vector, Matrix4x4 transform)
        {
            return new Vector4(
                vector.X * transform.M11 + vector.Y * transform.M21 + vector.Z * transform.M31 + transform.M41,
                vector.X * transform.M12 + vector.Y * transform.M22 + vector.Z * transform.M32 + transform.M42,
                vector.X * transform.M13 + vector.Y * transform.M23 + vector.Z * transform.M33 + transform.M43,
                vector.X * transform.M14 + vector.Y * transform.M24 + vector.Z * transform.M34 + transform.M44);
        }

        /// <summary>ベクトルを正規化します（SharpDX の Vector4.Normalize と同じ。長さが 0 に近いときはそのまま返す）。</summary>
        public static Vector4 NormalizeSharpDX(Vector4 value)
        {
            var length = (float)Math.Sqrt(value.X * value.X + value.Y * value.Y + value.Z * value.Z + value.W * value.W);
            if (IsZero(length))
            {
                return value;
            }
            var inv = 1f / length;
            return new Vector4(value.X * inv, value.Y * inv, value.Z * inv, value.W * inv);
        }

        /// <summary>クォータニオンを正規化します（SharpDX の Quaternion.Normalize と同じ。長さが 0 に近いときはそのまま返す）。</summary>
        public static Quaternion NormalizeSharpDX(Quaternion value)
        {
            var length = (float)Math.Sqrt(value.X * value.X + value.Y * value.Y + value.Z * value.Z + value.W * value.W);
            if (IsZero(length))
            {
                return value;
            }
            var inv = 1f / length;
            return new Quaternion(value.X * inv, value.Y * inv, value.Z * inv, value.W * inv);
        }

        private static float ClampComponent(float value, float min, float max)
        {
            // SharpDX と同じ順序（先に max、次に min。min > max のときは min になる）
            value = value > max ? max : value;
            return value < min ? min : value;
        }

        /// <summary>範囲に収めます（SharpDX の Vector2.Clamp と同じ。System.Numerics とは min &gt; max のときの結果が違う）。</summary>
        public static Vector2 ClampSharpDX(Vector2 value, Vector2 min, Vector2 max)
        {
            return new Vector2(ClampComponent(value.X, min.X, max.X), ClampComponent(value.Y, min.Y, max.Y));
        }

        /// <summary>範囲に収めます（SharpDX の Vector3.Clamp と同じ）。</summary>
        public static Vector3 ClampSharpDX(Vector3 value, Vector3 min, Vector3 max)
        {
            return new Vector3(ClampComponent(value.X, min.X, max.X), ClampComponent(value.Y, min.Y, max.Y), ClampComponent(value.Z, min.Z, max.Z));
        }

        /// <summary>範囲に収めます（SharpDX の Vector4.Clamp と同じ）。</summary>
        public static Vector4 ClampSharpDX(Vector4 value, Vector4 min, Vector4 max)
        {
            return new Vector4(ClampComponent(value.X, min.X, max.X), ClampComponent(value.Y, min.Y, max.Y), ClampComponent(value.Z, min.Z, max.Z), ClampComponent(value.W, min.W, max.W));
        }

        /// <summary>ベクトルを正規化します（SharpDX の Vector3.Normalize と同じ。長さが 0 に近いときはそのまま返す）。</summary>
        public static Vector3 NormalizeSharpDX(Vector3 value)
        {
            var length = (float)Math.Sqrt(value.X * value.X + value.Y * value.Y + value.Z * value.Z);
            if (IsZero(length))
            {
                return value;
            }
            var inv = 1f / length;
            return new Vector3(value.X * inv, value.Y * inv, value.Z * inv);
        }

        extension(Matrix4x4)
        {
            public static Matrix4x4 RotationX(float angle)
            {
                var cos = (float)Math.Cos(angle);
                var sin = (float)Math.Sin(angle);
                var result = Matrix4x4.Identity;
                result.M22 = cos;
                result.M23 = sin;
                result.M32 = 0f - sin;
                result.M33 = cos;
                return result;
            }

            public static Matrix4x4 RotationY(float angle)
            {
                var cos = (float)Math.Cos(angle);
                var sin = (float)Math.Sin(angle);
                var result = Matrix4x4.Identity;
                result.M11 = cos;
                result.M13 = 0f - sin;
                result.M31 = sin;
                result.M33 = cos;
                return result;
            }

            public static Matrix4x4 RotationZ(float angle)
            {
                var cos = (float)Math.Cos(angle);
                var sin = (float)Math.Sin(angle);
                var result = Matrix4x4.Identity;
                result.M11 = cos;
                result.M12 = sin;
                result.M21 = 0f - sin;
                result.M22 = cos;
                return result;
            }

            public static Matrix4x4 RotationAxis(Vector3 axis, float angle)
            {
                var x = axis.X;
                var y = axis.Y;
                var z = axis.Z;
                var cos = (float)Math.Cos(angle);
                var sin = (float)Math.Sin(angle);
                var xx = x * x;
                var yy = y * y;
                var zz = z * z;
                var xy = x * y;
                var xz = x * z;
                var yz = y * z;
                var result = Matrix4x4.Identity;
                result.M11 = xx + cos * (1f - xx);
                result.M12 = xy - cos * xy + sin * z;
                result.M13 = xz - cos * xz - sin * y;
                result.M21 = xy - cos * xy - sin * z;
                result.M22 = yy + cos * (1f - yy);
                result.M23 = yz - cos * yz + sin * x;
                result.M31 = xz - cos * xz + sin * y;
                result.M32 = yz - cos * yz - sin * x;
                result.M33 = zz + cos * (1f - zz);
                return result;
            }

            public static Matrix4x4 RotationQuaternion(Quaternion rotation)
            {
                var xx = rotation.X * rotation.X;
                var yy = rotation.Y * rotation.Y;
                var zz = rotation.Z * rotation.Z;
                var xy = rotation.X * rotation.Y;
                var zw = rotation.Z * rotation.W;
                var zx = rotation.Z * rotation.X;
                var yw = rotation.Y * rotation.W;
                var yz = rotation.Y * rotation.Z;
                var xw = rotation.X * rotation.W;
                var result = Matrix4x4.Identity;
                result.M11 = 1f - 2f * (yy + zz);
                result.M12 = 2f * (xy + zw);
                result.M13 = 2f * (zx - yw);
                result.M21 = 2f * (xy - zw);
                result.M22 = 1f - 2f * (zz + xx);
                result.M23 = 2f * (yz + xw);
                result.M31 = 2f * (zx + yw);
                result.M32 = 2f * (yz - xw);
                result.M33 = 1f - 2f * (yy + xx);
                return result;
            }

            public static Matrix4x4 Scaling(float x, float y, float z)
            {
                var result = Matrix4x4.Identity;
                result.M11 = x;
                result.M22 = y;
                result.M33 = z;
                return result;
            }

            public static Matrix4x4 Scaling(Vector3 scale)
            {
                return Matrix4x4.Scaling(scale.X, scale.Y, scale.Z);
            }

            public static Matrix4x4 Scaling(float scale)
            {
                return Matrix4x4.Scaling(scale, scale, scale);
            }

            public static Matrix4x4 Transformation2D(Vector2 scalingCenter, float scalingRotation, Vector2 scaling, Vector2 rotationCenter, float rotation, Vector2 translation)
            {
                var result = Mul(
                    Translation(ToVector3(-scalingCenter)),
                    Matrix4x4.RotationZ(0f - scalingRotation),
                    Matrix4x4.Scaling(ToVector3(scaling)),
                    Matrix4x4.RotationZ(scalingRotation),
                    Translation(ToVector3(scalingCenter)),
                    Translation(ToVector3(-rotationCenter)),
                    Matrix4x4.RotationZ(rotation),
                    Translation(ToVector3(rotationCenter)),
                    Translation(ToVector3(translation)));
                result.M33 = 1f;
                result.M44 = 1f;
                return result;
            }

            public static Matrix4x4 AffineTransformation2D(float scaling, float rotation, Vector2 translation)
            {
                return Mul(Matrix4x4.Scaling(scaling, scaling, 1f), Matrix4x4.RotationZ(rotation), Translation(ToVector3(translation)));
            }

            public static Matrix4x4 AffineTransformation2D(float scaling, Vector2 rotationCenter, float rotation, Vector2 translation)
            {
                return Mul(
                    Matrix4x4.Scaling(scaling, scaling, 1f),
                    Translation(ToVector3(-rotationCenter)),
                    Matrix4x4.RotationZ(rotation),
                    Translation(ToVector3(rotationCenter)),
                    Translation(ToVector3(translation)));
            }

            public static Matrix4x4 Transformation(Vector3 scalingCenter, Quaternion scalingRotation, Vector3 scaling, Vector3 rotationCenter, Quaternion rotation, Vector3 translation)
            {
                var matrix = Matrix4x4.RotationQuaternion(scalingRotation);
                return Mul(
                    Translation(-scalingCenter),
                    Matrix4x4.Transpose(matrix),
                    Matrix4x4.Scaling(scaling),
                    matrix,
                    Translation(scalingCenter),
                    Translation(-rotationCenter),
                    Matrix4x4.RotationQuaternion(rotation),
                    Translation(rotationCenter),
                    Translation(translation));
            }

            public static Matrix4x4 AffineTransformation(float scaling, Quaternion rotation, Vector3 translation)
            {
                return Mul(Matrix4x4.Scaling(scaling), Matrix4x4.RotationQuaternion(rotation), Translation(translation));
            }

            public static Matrix4x4 AffineTransformation(float scaling, Vector3 rotationCenter, Quaternion rotation, Vector3 translation)
            {
                return Mul(
                    Matrix4x4.Scaling(scaling),
                    Translation(-rotationCenter),
                    Matrix4x4.RotationQuaternion(rotation),
                    Translation(rotationCenter),
                    Translation(translation));
            }

            /// <summary>行列の各要素を割ります（SharpDX の Matrix / float。逆数を掛ける）。</summary>
            public static Matrix4x4 operator /(Matrix4x4 left, float right)
            {
                var inv = 1f / right;
                return new Matrix4x4(
                    left.M11 * inv, left.M12 * inv, left.M13 * inv, left.M14 * inv,
                    left.M21 * inv, left.M22 * inv, left.M23 * inv, left.M24 * inv,
                    left.M31 * inv, left.M32 * inv, left.M33 * inv, left.M34 * inv,
                    left.M41 * inv, left.M42 * inv, left.M43 * inv, left.M44 * inv);
            }
        }

        extension(Matrix4x4 m)
        {
            /// <summary>行列式を求めます（SharpDX の Matrix.Determinant と同じ計算）。</summary>
            public float Determinant()
            {
                var a = m.M33 * m.M44 - m.M34 * m.M43;
                var b = m.M32 * m.M44 - m.M34 * m.M42;
                var c = m.M32 * m.M43 - m.M33 * m.M42;
                var d = m.M31 * m.M44 - m.M34 * m.M41;
                var e = m.M31 * m.M43 - m.M33 * m.M41;
                var f = m.M31 * m.M42 - m.M32 * m.M41;
                return m.M11 * (m.M22 * a - m.M23 * b + m.M24 * c) - m.M12 * (m.M21 * a - m.M23 * d + m.M24 * e) + m.M13 * (m.M21 * b - m.M22 * d + m.M24 * f) - m.M14 * (m.M21 * c - m.M22 * e + m.M23 * f);
            }

            public Vector4 Row1 => new(m.M11, m.M12, m.M13, m.M14);
            public Vector4 Row2 => new(m.M21, m.M22, m.M23, m.M24);
            public Vector4 Row3 => new(m.M31, m.M32, m.M33, m.M34);
            public Vector4 Row4 => new(m.M41, m.M42, m.M43, m.M44);
            public Vector4 Column1 => new(m.M11, m.M21, m.M31, m.M41);
            public Vector4 Column2 => new(m.M12, m.M22, m.M32, m.M42);
            public Vector4 Column3 => new(m.M13, m.M23, m.M33, m.M43);
            public Vector4 Column4 => new(m.M14, m.M24, m.M34, m.M44);
        }

        extension(Vector2)
        {
            /// <summary>座標を行列で変換し、w で割ります（SharpDX の Vector2.TransformCoordinate）。</summary>
            public static Vector2 TransformCoordinate(Vector2 coordinate, Matrix4x4 transform)
            {
                var x = coordinate.X * transform.M11 + coordinate.Y * transform.M21 + transform.M41;
                var y = coordinate.X * transform.M12 + coordinate.Y * transform.M22 + transform.M42;
                var w = 1f / (coordinate.X * transform.M14 + coordinate.Y * transform.M24 + transform.M44);
                return new Vector2(x * w, y * w);
            }
        }

        extension(Vector3)
        {
            /// <summary>座標を行列で変換し、w で割ります（SharpDX の Vector3.TransformCoordinate）。</summary>
            public static Vector3 TransformCoordinate(Vector3 coordinate, Matrix4x4 transform)
            {
                var x = coordinate.X * transform.M11 + coordinate.Y * transform.M21 + coordinate.Z * transform.M31 + transform.M41;
                var y = coordinate.X * transform.M12 + coordinate.Y * transform.M22 + coordinate.Z * transform.M32 + transform.M42;
                var z = coordinate.X * transform.M13 + coordinate.Y * transform.M23 + coordinate.Z * transform.M33 + transform.M43;
                var w = 1f / (coordinate.X * transform.M14 + coordinate.Y * transform.M24 + coordinate.Z * transform.M34 + transform.M44);
                return new Vector3(x * w, y * w, z * w);
            }
        }

        extension(Color4 c)
        {
            /// <summary>赤（SharpDX の Color4.Red）。</summary>
            public float Red => c.R;
            /// <summary>緑（SharpDX の Color4.Green）。</summary>
            public float Green => c.G;
            /// <summary>青（SharpDX の Color4.Blue）。</summary>
            public float Blue => c.B;
            /// <summary>不透明度（SharpDX の Color4.Alpha）。</summary>
            public float Alpha => c.A;

            /// <summary>
            /// BGRA の 32 ビット整数にします（SharpDX の Color4.ToBgra と同じ。各成分を 255 倍して切り捨てる）。
            /// Vortice の Color4.ToBgra は戻り値が uint なので、名前を変えている。
            /// </summary>
            public int ToBgraInt()
            {
                var a = (uint)(c.A * 255f) & 0xFF;
                var r = (uint)(c.R * 255f) & 0xFF;
                var g = (uint)(c.G * 255f) & 0xFF;
                return (int)(((uint)(c.B * 255f) & 0xFF) | (g << 8) | (r << 16) | (a << 24));
            }

            /// <summary>
            /// RGBA の 32 ビット整数にします（SharpDX の Color4.ToRgba と同じ。各成分を 255 倍して切り捨てる）。
            /// Mod の設定の保存に使っているので、値を変えてはいけない。
            /// </summary>
            public int ToRgbaInt()
            {
                var a = (uint)(c.A * 255f) & 0xFF;
                var r = (uint)(c.R * 255f) & 0xFF;
                var g = (uint)(c.G * 255f) & 0xFF;
                var b = (uint)(c.B * 255f) & 0xFF;
                return (int)(r | (g << 8) | (b << 16) | (a << 24));
            }
        }
    }
    /// <summary>
    /// SharpDX の Quaternion にあって System.Numerics にないメンバーです（Matrix の同名のメンバーと区別するため、クラスを分けている）。
    /// </summary>
    public static class QuaternionCompat
    {
        extension(Quaternion)
        {
            public static Quaternion Exponential(Quaternion value)
            {
                var angle = (float)Math.Sqrt(value.X * value.X + value.Y * value.Y + value.Z * value.Z);
                var sin = (float)Math.Sin(angle);
                Quaternion result;
                if (!MathCompat.IsZero(sin))
                {
                    var coeff = sin / angle;
                    result = new Quaternion(coeff * value.X, coeff * value.Y, coeff * value.Z, 0f);
                }
                else
                {
                    result = value;
                }
                result.W = (float)Math.Cos(angle);
                return result;
            }

            public static Quaternion Logarithm(Quaternion value)
            {
                Quaternion result;
                if ((double)Math.Abs(value.W) < 1.0)
                {
                    var angle = (float)Math.Acos(value.W);
                    var sin = (float)Math.Sin(angle);
                    if (!MathCompat.IsZero(sin))
                    {
                        var coeff = angle / sin;
                        result = new Quaternion(value.X * coeff, value.Y * coeff, value.Z * coeff, 0f);
                    }
                    else
                    {
                        result = value;
                    }
                }
                else
                {
                    result = value;
                }
                result.W = 0f;
                return result;
            }

            public static Quaternion RotationAxis(Vector3 axis, float angle)
            {
                var normalized = MathCompat.NormalizeSharpDX(axis);
                var half = angle * 0.5f;
                var sin = (float)Math.Sin(half);
                var cos = (float)Math.Cos(half);
                return new Quaternion(normalized.X * sin, normalized.Y * sin, normalized.Z * sin, cos);
            }

            public static Quaternion RotationMatrix(Matrix4x4 matrix)
            {
                Quaternion result;
                var trace = matrix.M11 + matrix.M22 + matrix.M33;
                if (trace > 0f)
                {
                    var s = (float)Math.Sqrt(trace + 1f);
                    result.W = s * 0.5f;
                    s = 0.5f / s;
                    result.X = (matrix.M23 - matrix.M32) * s;
                    result.Y = (matrix.M31 - matrix.M13) * s;
                    result.Z = (matrix.M12 - matrix.M21) * s;
                }
                else if (matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33)
                {
                    var s = (float)Math.Sqrt(1f + matrix.M11 - matrix.M22 - matrix.M33);
                    var half = 0.5f / s;
                    result.X = 0.5f * s;
                    result.Y = (matrix.M12 + matrix.M21) * half;
                    result.Z = (matrix.M13 + matrix.M31) * half;
                    result.W = (matrix.M23 - matrix.M32) * half;
                }
                else if (matrix.M22 > matrix.M33)
                {
                    var s = (float)Math.Sqrt(1f + matrix.M22 - matrix.M11 - matrix.M33);
                    var half = 0.5f / s;
                    result.X = (matrix.M21 + matrix.M12) * half;
                    result.Y = 0.5f * s;
                    result.Z = (matrix.M32 + matrix.M23) * half;
                    result.W = (matrix.M31 - matrix.M13) * half;
                }
                else
                {
                    var s = (float)Math.Sqrt(1f + matrix.M33 - matrix.M11 - matrix.M22);
                    var half = 0.5f / s;
                    result.X = (matrix.M31 + matrix.M13) * half;
                    result.Y = (matrix.M32 + matrix.M23) * half;
                    result.Z = 0.5f * s;
                    result.W = (matrix.M12 - matrix.M21) * half;
                }
                return result;
            }
        }

        extension(Quaternion q)
        {
            /// <summary>回転の角度（SharpDX の Quaternion.Angle）。</summary>
            public float Angle
            {
                get
                {
                    if (MathCompat.IsZero(q.X * q.X + q.Y * q.Y + q.Z * q.Z))
                    {
                        return 0f;
                    }
                    return (float)(2.0 * Math.Acos(Math.Clamp(q.W, -1f, 1f)));
                }
            }

            /// <summary>回転の軸（SharpDX の Quaternion.Axis）。</summary>
            public Vector3 Axis
            {
                get
                {
                    var lengthSquared = q.X * q.X + q.Y * q.Y + q.Z * q.Z;
                    if (MathCompat.IsZero(lengthSquared))
                    {
                        return Vector3.UnitX;
                    }
                    var inv = 1f / (float)Math.Sqrt(lengthSquared);
                    return new Vector3(q.X * inv, q.Y * inv, q.Z * inv);
                }
            }
        }
    }
}
