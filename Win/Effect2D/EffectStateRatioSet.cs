using BezierCaliculator;
using System;

namespace Effect2D
{
    /// <summary>
    /// エフェクトの比と状態のセット
    /// </summary>
    public class EffectStateRatioSet
    {
        private readonly IRatioMaker[] RatioMakers;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EffectStateRatioSet()
        {
            RatioMakers = new IRatioMaker[Utility.RatioTypeArray.Length];
        }
        /// <summary>
        /// 全てに線形比作成器をあてる
        /// </summary>
        public void SetDefaultMaker()
        {
            for (int i = 0; i < RatioMakers.Length; i++)
            {
                RatioMakers[i] = new LinearRatioMaker
                {
                    Set = this
                };
            }
        }
        /// <summary>
        /// ヌルの比作成器を持つものに線形比作成器をあてる
        /// </summary>
        public void SetDefaultToNullMaker()
        {
            for (int i = 0; i < RatioMakers.Length; i++)
            {
                if (RatioMakers[i] == null)
                {
                    RatioMakers[i] = new LinearRatioMaker
                    {
                        Set = this
                    };
                }
            }
        }
        /// <summary>
        /// ベジエ位置が設定されているか
        /// </summary>
        public bool IsBezierPosition
        {
            get
            {
                if (BAnalyzer == null || BAnalyzer.BCPS == null || BAnalyzer.BCPS.Length < 2) return false;
                return true;
            }
        }
        /// <summary>
        /// ベジエアナライザー
        /// </summary>
        public BezierAnalyzer BAnalyzer { get; set; }
        /// <summary>
        /// 開始フレーム
        /// </summary>
        public int StartFrame { get; set; }
        /// <summary>
        /// 終了フレーム
        /// </summary>
        public int EndFrame { get; set; }
        /// <summary>
        /// 開始状態
        /// </summary>
        public EffectStateStructure StartState
        {
            get;
            set;
        }
        /// <summary>
        /// 終了状態
        /// </summary>
        public EffectStateStructure EndState
        {
            get;
            set;
        }
        /// <summary>
        /// 比作成器を取得する
        /// </summary>
        /// <param name="type">タイプ</param>
        /// <returns></returns>
        public IRatioMaker this[RatioType type]
        {
            get
            {
                return RatioMakers[(int)type];
            }
            set
            {
                RatioMakers[(int)type] = value;
                value.Set = this;
            }
        }
        /// <summary>
        /// 比を取得する
        /// </summary>
        /// <param name="CurrentFrame">現在のフレーム</param>
        /// <returns></returns>
        public float[] GetRatios(float CurrentFrame)
        {
            float[] ret = new float[Utility.RatioTypeArray.Length];
            GetRatios(CurrentFrame, ret);
            return ret;
        }

        /// <summary>
        /// 比を、渡された領域に書き込みます（RatioType の値の順）。
        /// </summary>
        /// <param name="currentFrame">現在のフレーム</param>
        /// <param name="ratios">書き込み先</param>
        internal void GetRatios(float currentFrame, Span<float> ratios)
        {
            foreach (RatioType type in Utility.RatioTypeArray)
            {
                ratios[(int)type] = this[type].GetRatio(currentFrame);
            }
        }
        /// <summary>
        /// 比を取得する
        /// </summary>
        /// <param name="type">タイプ</param>
        /// <param name="CurrentFrame">現在のフレーム</param>
        /// <returns></returns>
        public float GetRatio(RatioType type, float CurrentFrame)
        {
            return this[type].GetRatio(CurrentFrame);
        }

        /// <summary>
        /// クローンメソッド
        /// </summary>
        /// <returns></returns>
        public object CloneExceptState()
        {
            var ret = new EffectStateRatioSet
            {
                StartFrame = StartFrame,
                EndFrame = EndFrame
            };
            if (BAnalyzer != null) ret.BAnalyzer = (BezierAnalyzer)BAnalyzer.Clone();
            ret.StartState = StartState;
            ret.EndState = EndState;
            //ret.StartState = (EffectStateStructure)StartState.Clone();
            //ret.EndState = (EffectStateStructure)EndState.Clone();
            for (int i = 0; i < RatioMakers.Length; i++)
            {
                ret.RatioMakers[i] = (IRatioMaker)RatioMakers[i].Clone();
                ret.RatioMakers[i].Set = ret;
            }
            return ret;
        }
    }
}
