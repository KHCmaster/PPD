using BezierCaliculator;
using System;
using System.Numerics;
using PointF = System.Drawing.PointF;

namespace PPDEditor.Models
{
    /// <summary>
    /// 曲線の上にマークを並べるときの間隔の決め方です。
    /// </summary>
    public enum GeometryIntervalMode
    {
        /// <summary>
        /// 時間の間隔に合わせる
        /// </summary>
        Time,

        /// <summary>
        /// 均等にする（最後のマークを曲線の終わりに置く）
        /// </summary>
        Even1,

        /// <summary>
        /// 均等にする（最後のマークを曲線の終わりに置かない）
        /// </summary>
        Even2,

        /// <summary>
        /// 4 分音符ごとに決まった距離にする
        /// </summary>
        FixedDistance
    }

    /// <summary>
    /// マークの角度の決め方です。
    /// </summary>
    public enum GeometryAngleMode
    {
        Tangent1,
        Tangent2,
        Normal1,
        Normal2,

        /// <summary>
        /// 角度には当てはめない
        /// </summary>
        None
    }

    /// <summary>
    /// ベジエ曲線の上に、選んでいる範囲のマークを並べたときの位置と向きを求めます。
    /// </summary>
    static class GeometryPlacement
    {
        /// <summary>
        /// マークの位置と向きを求めます。曲線の点が 2 つ未満か、マークが 2 つ未満のときは、すべて 0 です。時間の間隔に合わせるときにすべて同じ時間なら、空です。
        /// </summary>
        /// <param name="markTimes">マークの時間（順に並んだもの）。</param>
        /// <param name="bpm">4 分固定距離のときの BPM。</param>
        /// <param name="fixedDistance">4 分固定距離のときの、4 分音符 1 つ分の距離。</param>
        public static (Vector2[] Positions, Vector2[] Directions) Compute(BezierControlPoint[] bcps, float[] markTimes,
            GeometryIntervalMode intervalMode, GeometryAngleMode angleMode, float bpm, int fixedDistance)
        {
            var positions = new Vector2[markTimes.Length];
            var directions = new Vector2[markTimes.Length];
            if (bcps.Length < 2 || markTimes.Length < 2)
            {
                return (positions, directions);
            }

            float all = markTimes[^1] - markTimes[0];
            if (intervalMode == GeometryIntervalMode.Time && all == 0)
            {
                return ([], []);
            }

            var ba = new BezierAnalyzer(bcps);
            for (int i = 0; i < markTimes.Length; i++)
            {
                float ratio;
                switch (intervalMode)
                {
                    case GeometryIntervalMode.Time:
                        ratio = 99.9999f * (markTimes[i] - markTimes[0]) / all;
                        break;
                    case GeometryIntervalMode.Even1:
                        ratio = i * 99.9999f / (markTimes.Length - 1);
                        break;
                    case GeometryIntervalMode.Even2:
                        ratio = i * 99.9999f / markTimes.Length;
                        break;
                    default:
                        float beat = 60 / (bpm <= 10 ? 10 : bpm);
                        ratio = (markTimes[i] - markTimes[0]) / beat * fixedDistance / ba.Length * BezierAnalyzer.MaxRatio;
                        ratio = Math.Clamp(ratio, 0, BezierAnalyzer.MaxRatio);
                        break;
                }
                ba.GetPoint(ratio, out PointF pos, out PointF dir);
                positions[i] = new Vector2(pos.X, pos.Y);
                directions[i] = TransformDirection(new Vector2(dir.X, dir.Y), angleMode);
            }
            return (positions, directions);
        }

        private static Vector2 TransformDirection(Vector2 dir, GeometryAngleMode angleMode)
        {
            return angleMode switch
            {
                GeometryAngleMode.Normal1 => Rotate(dir, (float)Math.PI / 2),
                GeometryAngleMode.Tangent2 => Rotate(dir, (float)Math.PI),
                GeometryAngleMode.Normal2 => Rotate(dir, (float)Math.PI / 2 * 3),
                _ => dir,
            };
        }

        public static Vector2 Rotate(Vector2 p, float r)
        {
            return new Vector2((float)(Math.Cos(r) * p.X - Math.Sin(r) * p.Y), (float)(Math.Sin(r) * p.X + Math.Cos(r) * p.Y));
        }
    }
}
