namespace Effect2D
{
    /// <summary>
    /// コンスタントな比作成
    /// </summary>
    public class ConstantRatioMaker : IRatioMaker
    {
        /// <summary>
        /// 比状態のセット
        /// </summary>
        public EffectStateRatioSet Set { get; set; }
        /// <summary>
        /// 割合を取得する
        /// </summary>
        /// <param name="CurrentFrame">現在のフレーム数</param>
        /// <returns></returns>
        public float GetRatio(float CurrentFrame)
        {
            if (CurrentFrame >= Set.EndFrame) return 0;
            else return 1;
        }
        /// <summary>
        /// クローンメソッド
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var ret = new ConstantRatioMaker();
            return ret;
        }
    }
}
