using System;
using System.Collections.Generic;

namespace Effect2D
{
    /// <summary>
    /// エフェクトのインターフェース
    /// </summary>
    public interface IEffect : ICloneable
    {
        /// <summary>
        /// 更新する
        /// </summary>
        /// <param name="parentframe">親のフレーム</param>
        /// <param name="parentstate">親の状態</param>
        void Update(float parentframe, EffectStateStructure parentstate);
        /// <summary>
        /// 更新する
        /// </summary>
        /// <param name="parentframe">親のフレーム</param>
        /// <param name="parentfps">親のFPS</param>
        /// <param name="parentstate">親の状態</param>
        void Update(float parentframe, float parentfps, EffectStateStructure parentstate);
        /// <summary>
        /// 描画する
        /// </summary>
        /// <param name="callback">コールバック</param>
        void Draw(DrawEffectCallBack callback);
        /// <summary>
        /// 全体の長さを計算する
        /// </summary>
        void CheckFrameLength();
        /// <summary>
        /// 開始フレーム
        /// </summary>
        int StartFrame { get; set; }
        /// <summary>
        /// 現在のフレーム
        /// </summary>
        float CurrentFrame { get; }
        /// <summary>
        /// フレームの長さ
        /// </summary>
        int FrameLength { get; set; }
        /// <summary>
        /// FPS
        /// </summary>
        float FPS { get; set; }
        /// <summary>
        /// 現在の状態
        /// </summary>
        EffectStateStructure CurrentState { get; set; }
        /// <summary>
        /// 子エフェクト
        /// </summary>
        List<IEffect> Effects { get; set; }
        /// <summary>
        /// 比状態セット
        /// </summary>
        SortedList<int, EffectStateRatioSet> Sets { get; set; }
    }
}
