namespace Effect2D
{
    /// <summary>
    /// 線形的な比作成
    /// </summary>
    public class LinearRatioMaker : IRatioMaker
    {
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
            if (CurrentFrame < Set.StartFrame) return 1;
            if (CurrentFrame > Set.EndFrame) return 0;
            return 1 - (CurrentFrame - Set.StartFrame) / (Set.EndFrame - Set.StartFrame);
        }
        /// <summary>
        /// クローンメソッド
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var ret = new LinearRatioMaker();
            return ret;
        }
    }
}
