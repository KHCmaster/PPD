using BezierCaliculator;

namespace Effect2D
{
    /// <summary>
    /// ベジエ比作成
    /// </summary>
    public class BezierRatioMaker : IRatioMaker
    {
        /// <summary>
        /// 最大の長さ(X,Y)
        /// </summary>
        public const float MaxLength = 128;
        private BezierRatioMaker()
        {
        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="p1">ベジエ頂点１</param>
        /// <param name="p2">ベジエ頂点２</param>
        public BezierRatioMaker(BezierControlPoint p1, BezierControlPoint p2)
        {
            Analyzer = new BezierAnalyzer(new BezierControlPoint[] { p1, p2 });
        }
        /// <summary>
        /// ベジエアナライザー
        /// </summary>
        public BezierAnalyzer Analyzer { get; set; }
        /// <summary>
        /// 比状態セット
        /// </summary>
        public EffectStateRatioSet Set { get; set; }
        /// <summary>
        /// 割合を取得する
        /// </summary>
        /// <param name="CurrentFrame">現在のフレーム</param>
        /// <returns></returns>
        public float GetRatio(float CurrentFrame)
        {
            float X = MaxLength * (CurrentFrame - Set.StartFrame) / (Set.EndFrame - Set.StartFrame);
            if (X < 0) return 1;
            if (X >= MaxLength) return 0;
            return 1 - (MaxLength - Analyzer.GetYFromX(X)) / MaxLength;
        }
        /// <summary>
        /// クローンメソッド
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var ret = new BezierRatioMaker
            {
                Analyzer = (BezierAnalyzer)Analyzer.Clone()
            };
            return ret;
        }
    }
}
