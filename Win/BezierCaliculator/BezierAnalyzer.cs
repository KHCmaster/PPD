using System;
using System.Drawing;

namespace BezierCaliculator
{
    /// <summary>
    /// 連結ベジエ曲線解析クラス
    /// </summary>
    public class BezierAnalyzer
    {
        /// <summary>
        /// 最大の割合
        /// </summary>
        public const float MaxRatio = 100;
        /// <summary>
        /// 半分の割合
        /// </summary>
        public const float MidRatio = 50;
        /// <summary>
        /// 最小の割合
        /// </summary>
        public const float MinRatio = 0;
        float length;
        BezierControlPoint[] bcps;
        float[] eachlength;
        private BezierAnalyzer()
        {
        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="points"></param>
        public BezierAnalyzer(BezierControlPoint[] points)
        {
            this.bcps = points;
            Analyze();
        }
        /// <summary>
        /// ベジエ頂点配列
        /// </summary>
        public BezierControlPoint[] BCPS
        {
            get
            {
                return bcps;
            }
        }
        private void Analyze()
        {
            eachlength = new float[bcps.Length - 1];
            BezierControlPoint previous = null;
            for (int i = 0; i < bcps.Length; i++)
            {
                BezierControlPoint bcp = bcps[i];
                if (previous != null)
                {
                    eachlength[i - 1] += BezierCaliculate.GetBezeierLength(previous, bcp);
                    length += eachlength[i - 1];
                }
                previous = bcp;
            }
        }
        /// <summary>
        /// ベジエ曲線の長さ
        /// </summary>
        public float Length
        {
            get
            {
                return length;
            }
        }
        /// <summary>
        /// 連結ベジエから特定の割合の場所を得るメソッド
        /// </summary>
        /// <param name="num">最大、最小はMaxRatio,MinRatioから</param>
        /// <param name="direction">位置</param>
        /// <param name="pos">向き</param>
        /// <returns></returns>
        public void GetPoint(float num, out PointF pos, out PointF direction)
        {
            pos = PointF.Empty;
            direction = new PointF(1, 0);
            if (bcps == null || bcps.Length == 0)
            {
                return;
            }
            if (num < MinRatio) num = MinRatio;
            else if (num > MaxRatio) num = MaxRatio;
            //find where 
            float sumnum = 0;
            float rest = 0;
            BezierControlPoint previous = null, next = null;
            int foundindex = -1;
            for (int i = 0; i < bcps.Length; i++)
            {
                BezierControlPoint bcp = bcps[i];
                if (previous != null)
                {
                    float ratio = eachlength[i - 1] / length * MaxRatio;
                    if (sumnum + ratio >= num || (sumnum + ratio) >= MaxRatio - 0.0009765625)
                    {
                        next = bcp;
                        rest = num - sumnum;
                        foundindex = i;
                        break;
                    }
                    sumnum += ratio;
                }
                previous = bcp;
            }
            if (foundindex != -1)
            {
                float targetlength = length * rest / MaxRatio;
                BezierCaliculate.GetBezeirSplitPoint(previous, next, targetlength, out pos, out direction);
                return;
            }
        }
        /// <summary>
        /// 指定された割合分割されたベジエを得るメソッド
        /// </summary>
        /// <param name="num">最大、最小はMaxRatio,MinRatioから</param>
        /// <param name="bcps1">分割１</param>
        /// <param name="bcps2">分割２</param>
        public void GetDevidedBeziers(float num, out BezierControlPoint[] bcps1, out BezierControlPoint[] bcps2)
        {
            bcps1 = null;
            bcps2 = null;
            if (bcps == null || bcps.Length == 0)
            {
                return;
            }
            if (num < MinRatio) num = MinRatio;
            else if (num > MaxRatio) num = MaxRatio;
            //find where 
            float sumnum = 0;
            float rest = 0;
            BezierControlPoint previous = null, next = null;
            int foundindex = -1;
            for (int i = 0; i < bcps.Length; i++)
            {
                BezierControlPoint bcp = bcps[i];
                if (previous != null)
                {
                    float ratio = eachlength[i - 1] / length * MaxRatio;
                    if (sumnum + ratio >= num || (sumnum + ratio) >= MaxRatio - 0.0009765625)
                    {
                        next = bcp;
                        rest = num - sumnum;
                        foundindex = i;
                        break;
                    }
                    sumnum += ratio;
                }
                previous = bcp;
            }
            if (foundindex != -1)
            {
                float targetratio = 1 - rest / MaxRatio * length / eachlength[foundindex - 1];
                BezierCaliculate.GetDevidedBeziers(previous, next, targetratio, out BezierControlPoint bcp1, out BezierControlPoint bcp2, out BezierControlPoint bcp3);
                bcps1 = new BezierControlPoint[foundindex + 1];
                bcps2 = new BezierControlPoint[BCPS.Length - foundindex + 1];
                for (int i = 0; i < foundindex - 1; i++) bcps1[i] = BCPS[i].Clone();
                bcps1[foundindex - 1] = bcp1;
                bcps1[foundindex] = bcp2.Clone();
                bcps2[0] = bcp2;
                bcps2[1] = bcp3;
                for (int i = foundindex + 1; i < BCPS.Length; i++) bcps2[i - foundindex + 1] = BCPS[i].Clone();
                return;
            }
        }
        /// <summary>
        /// 単調増加ベジエの場合にのみ使用可。X座標からY座標を得る
        /// </summary>
        /// <param name="X"></param>
        /// <returns></returns>
        public float GetYFromX(float X)
        {
            float ret = 0;
            PointF p1 = BCPS[0].Second, p2 = BCPS[0].Second, p3 = BCPS[1].Second, p4 = BCPS[1].Second;
            if (BCPS[0].ValidThird) p2 = BCPS[0].Third;
            if (BCPS[1].ValidFirst) p3 = BCPS[1].First;
            float s = 0, e = 1;
            for (int i = 0; i < 20; i++)
            {
                float m = (s + e) / 2;
                var bp = BezierCaliculate.GetBezeirPoint(ref p1, ref p2, ref p3, ref p4, ref m);
                if (bp.X < X)
                {
                    e = m;
                }
                else
                {
                    s = m;
                }
                ret = bp.Y;
            }
            return ret;
        }
        /// <summary>
        /// クローンメソッド
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var ret = new BezierAnalyzer
            {
                eachlength = new float[eachlength.Length]
            };
            Array.Copy(eachlength, ret.eachlength, eachlength.Length);
            ret.bcps = new BezierControlPoint[bcps.Length];
            Array.Copy(bcps, ret.bcps, bcps.Length);
            ret.length = length;
            return ret;
        }
    }
}
