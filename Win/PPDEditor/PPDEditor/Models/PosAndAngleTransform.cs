using PPDMath;
using System;
using System.Numerics;

namespace PPDEditor.Models
{
    /// <summary>
    /// 保存した位置と角度に当てはめる変換です。
    /// </summary>
    public enum PosAndAngleApplyMode
    {
        /// <summary>
        /// そのまま
        /// </summary>
        None,

        /// <summary>
        /// 点対称
        /// </summary>
        Point,

        /// <summary>
        /// 線対称
        /// </summary>
        Line
    }

    /// <summary>
    /// 保存した位置と角度を、点対称や線対称に移します。
    /// </summary>
    static class PosAndAngleTransform
    {
        static readonly Vector2 ScreenSize = new(800, 450);

        /// <summary>
        /// 変換した位置と角度を求めます。位置は画面の中に収め、角度は 0 から 2π にします。
        /// </summary>
        /// <param name="point">点対称の中心です。</param>
        /// <param name="lineStart">線対称の線の始点です。</param>
        /// <param name="lineEnd">線対称の線の終点です。</param>
        /// <param name="symmetricAngle">角度も対称にするかです。</param>
        /// <param name="hasAngle">角度があるかです（ないときは角度を 0 にする）。</param>
        public static (Vector2[] Positions, float[] Angles) Apply(PosAndAngleInfo[] infos, PosAndAngleApplyMode mode,
            Vector2 point, Vector2 lineStart, Vector2 lineEnd, bool symmetricAngle, bool hasAngle)
        {
            var positions = new Vector2[infos.Length];
            var angles = new float[infos.Length];
            var basicvec = MathCompat.NormalizeSharpDX(lineEnd - lineStart);
            for (int i = 0; i < infos.Length; i++)
            {
                Vector2 position = infos[i].Position ?? Vector2.Zero;
                float angle = infos[i].Rotation ?? 0;
                switch (mode)
                {
                    case PosAndAngleApplyMode.None:
                        positions[i] = position;
                        break;
                    case PosAndAngleApplyMode.Point:
                        positions[i] = new Vector2(position.X + 2 * (point.X - position.X), position.Y + 2 * (point.Y - position.Y));
                        if (symmetricAngle)
                        {
                            angle = (float)(GetNormalAngle(angle) - Math.PI);
                        }
                        break;
                    case PosAndAngleApplyMode.Line:
                        var targetvec = position - lineStart;
                        var targetvecnormal = MathCompat.NormalizeSharpDX(targetvec);
                        var betweenangle = Vector2.Dot(basicvec, targetvecnormal);
                        var answervec = Vector2.Add(targetvecnormal, 2 * (betweenangle * basicvec - targetvecnormal));
                        var length = Vector2.Distance(Vector2.Zero, targetvec);
                        positions[i] = new Vector2(answervec.X * length + lineStart.X, answervec.Y * length + lineStart.Y);
                        if (symmetricAngle)
                        {
                            var na = GetNormalAngle(angle);
                            var d = (float)Math.Acos(-basicvec.X);
                            var trans = (float)GetNormalAngle(na - d);
                            angle = (float)(Math.PI * 2 - trans) + d;
                        }
                        break;
                }
                positions[i] = MathCompat.ClampSharpDX(positions[i], Vector2.Zero, ScreenSize);
                angles[i] = NormalizeAngle(hasAngle ? angle : 0);
            }
            return (positions, angles);
        }

        private static double GetNormalAngle(double angle)
        {
            var shou = (int)Math.Floor(angle / (Math.PI * 2));
            return angle - Math.PI * 2 * shou;
        }

        private static float NormalizeAngle(float angle)
        {
            var val = Math.IEEERemainder(angle, Math.PI * 2);
            if (val < 0) val += Math.PI * 2;
            return (float)val;
        }
    }
}
