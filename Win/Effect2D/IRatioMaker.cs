using System;

namespace Effect2D
{
    /// <summary>
    /// 比作成のインターフェース
    /// </summary>
    public interface IRatioMaker : ICloneable
    {
        /// <summary>
        /// 比状態
        /// </summary>
        EffectStateRatioSet Set { get; set; }
        /// <summary>
        /// 割合を取得する
        /// </summary>
        /// <param name="CurrentFrame"></param>
        /// <returns></returns>
        float GetRatio(float CurrentFrame);
    }
}
