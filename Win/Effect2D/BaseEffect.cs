using System;
using System.Collections.Generic;
using System.Drawing;

namespace Effect2D
{
    /// <summary>
    /// 基本のエフェクト
    /// </summary>
    public class BaseEffect : IEffect
    {
        float currentframe;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BaseEffect()
        {
            Effects = new List<IEffect>();
            Sets = new SortedList<int, EffectStateRatioSet>();
        }

        /// <summary>
        /// 長さ１の線形状態にする
        /// </summary>
        public void SetDefault()
        {
            var set = new EffectStateRatioSet
            {
                StartState = new EffectStateStructure(),
                EndState = new EffectStateStructure(),
                StartFrame = 0,
                EndFrame = 1
            };
            set.SetDefaultMaker();
            FrameLength = 1;
            Sets.Add(0, set);
        }

        /// <summary>
        /// 更新する
        /// </summary>
        /// <param name="parentframe">親のフレーム</param>
        /// <param name="parentstate">親の状態</param>
        public void Update(float parentframe, EffectStateStructure parentstate)
        {
            if (!InnerUpdate(parentframe, parentstate)) return;
            foreach (IEffect effect in Effects)
            {
                if (effect.Effects.Count == 0) effect.Update(currentframe, CurrentState);
                else effect.Update(currentframe, FPS, CurrentState);
            }
        }

        /// <summary>
        /// 更新する
        /// </summary>
        /// <param name="parentframe">親のフレーム</param>
        /// <param name="parentfps">親のFPS</param>
        /// <param name="parentstate">親の状態</param>
        public void Update(float parentframe, float parentfps, EffectStateStructure parentstate)
        {
            if (!InnerUpdate(parentframe, parentstate)) return;
            foreach (IEffect effect in Effects)
            {
                if (effect.Effects.Count == 0) effect.Update(currentframe * effect.FPS / parentfps, CurrentState);
                else effect.Update(currentframe * effect.FPS / parentfps, FPS, CurrentState);
            }
        }

        /// <summary>
        /// 描画する
        /// </summary>
        /// <param name="callback">描画コールバック</param>
        public void Draw(DrawEffectCallBack callback)
        {
            if (CurrentFrame >= 0 && CurrentFrame <= FrameLength && CurrentState != null)
            {
                if (Effects.Count == 0) { if (CurrentState.Alpha > 0) callback(Filename, CurrentState); }
                else
                {
                    foreach (IEffect effect in Effects)
                    {
                        effect.Draw(callback);
                    }
                }
            }
        }

        private bool InnerUpdate(float parentframe, EffectStateStructure parentState)
        {
            currentframe = parentframe - StartFrame;
            if (currentframe > FrameLength || currentframe < 0)
            {
                return false;
            }
            EffectStateRatioSet set = Sets.Values[BinaryFinder.FindNearest(Sets.Keys, ref parentframe)];
            CurrentState = set.StartState.GetMixedState(set.GetRatios(parentframe), set.EndState);
            if (set.IsBezierPosition)
            {
                var bratio = set.GetRatio(RatioType.BezierPosition, parentframe);
                set.BAnalyzer.GetPoint(BezierCaliculator.BezierAnalyzer.MaxRatio * (1 - bratio), out PointF outp, out PointF outdir);
                CurrentState.X = outp.X;
                CurrentState.Y = outp.Y;
            }
            CurrentState.Compose(parentState);
            return true;
        }

        /// <summary>
        /// クローンメソッド
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var ret = new BaseEffect
            {
                FPS = FPS,
                StartFrame = StartFrame,
                currentframe = currentframe,
                FrameLength = FrameLength,
                Filename = Filename
            };
            if (CurrentState != null) ret.CurrentState = (EffectStateStructure)CurrentState.Clone();
            foreach (IEffect effect in Effects)
            {
                ret.Effects.Add((IEffect)effect.Clone());
            }
            EffectStateStructure state = null;
            foreach (KeyValuePair<int, EffectStateRatioSet> set in Sets)
            {
                var newset = (EffectStateRatioSet)set.Value.CloneExceptState();
                if (state != null)
                {
                    newset.StartState = state;
                }
                else
                {
                    newset.StartState = (EffectStateStructure)set.Value.StartState.Clone();
                }
                newset.EndState = (EffectStateStructure)set.Value.EndState.Clone();
                state = newset.EndState;
                ret.Sets.Add(set.Key, newset);
            }
            return ret;
        }
        /// <summary>
        /// 全体の長さを調べる
        /// </summary>
        public void CheckFrameLength()
        {
            int min = int.MaxValue, max = int.MinValue;
            foreach (KeyValuePair<int, EffectStateRatioSet> set in Sets)
            {
                min = Math.Min(min, set.Value.StartFrame);
                max = Math.Max(max, set.Value.EndFrame);
            }
            StartFrame = min;
            FrameLength = max - min;
        }
        /// <summary>
        /// FPS
        /// </summary>
        public float FPS { get; set; }
        /// <summary>
        /// ファイルパス
        /// </summary>
        public string Filename { get; set; }
        /// <summary>
        /// 開始フレーム
        /// </summary>
        public int StartFrame { get; set; }
        /// <summary>
        /// 現在のフレーム
        /// </summary>
        public float CurrentFrame { get { return currentframe; } }
        /// <summary>
        /// フレームの長さ
        /// </summary>
        public int FrameLength { get; set; }
        /// <summary>
        /// 現在の状態
        /// </summary>
        public EffectStateStructure CurrentState { get; set; }
        /// <summary>
        /// 子エフェクト
        /// </summary>
        public List<IEffect> Effects { get; set; }
        /// <summary>
        /// 比状態セット
        /// </summary>
        public SortedList<int, EffectStateRatioSet> Sets { get; set; }
    }
}
