using BezierCaliculator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Effect2D
{
    /// <summary>
    /// 描画ようのコールバック
    /// </summary>
    /// <param name="filename">リソース名</param>
    /// <param name="state">状態</param>
    public delegate void DrawEffectCallBack(string filename, EffectStateStructure state);


    /// <summary>
    /// エフェクトマネージャー
    /// </summary>
    public class EffectManager : IEffect
    {
        /// <summary>
        /// 終了イベント
        /// </summary>
        public event EventHandler Finish;

        /// <summary>
        /// 再生状態
        /// </summary>
        public enum PlayState
        {
            /// <summary>
            /// 停止
            /// </summary>
            Stop = 0,
            /// <summary>
            /// 再生中
            /// </summary>
            Playing = 1,
            /// <summary>
            /// 一時停止
            /// </summary>
            Pause = 2
        }

        /// <summary>
        /// 再生タイプ
        /// </summary>
        public enum PlayType
        {
            /// <summary>
            /// 一回
            /// </summary>
            Once = 0,
            /// <summary>
            /// リバース１回
            /// </summary>
            ReverseOnce = 1,
            /// <summary>
            /// ループ
            /// </summary>
            Loop = 2,
            /// <summary>
            /// リバースループ
            /// </summary>
            ReverseLoop = 3
        }

        PlayState state = PlayState.Stop;
        Stopwatch stopwatch;
        float currentFrame;
        PlayType playType = PlayType.Once;

        /// <summary>
        /// 再生状態
        /// </summary>
        public PlayState State
        {
            get
            {
                return state;
            }
        }

        /// <summary>
        /// FPS
        /// </summary>
        public float FPS
        {
            get;
            set;
        }

        /// <summary>
        /// 開始フレーム
        /// </summary>
        public int StartFrame
        {
            get;
            set;
        }

        /// <summary>
        /// 現在のフレーム
        /// </summary>
        public float CurrentFrame
        {
            get { return currentFrame; }
        }

        /// <summary>
        /// 総フレーム数
        /// </summary>
        public int FrameLength
        {
            get;
            set;
        }

        /// <summary>
        /// 現在の状態
        /// </summary>
        public EffectStateStructure CurrentState
        {
            get;
            set;
        }

        /// <summary>
        /// 子エフェクト
        /// </summary>
        public List<IEffect> Effects
        {
            get;
            set;
        }

        /// <summary>
        /// 比状態セット
        /// </summary>
        public SortedList<int, EffectStateRatioSet> Sets
        {
            get;
            set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EffectManager()
        {
            stopwatch = new Stopwatch();
            FPS = 60;
            Effects = new List<IEffect>();
            Sets = new SortedList<int, EffectStateRatioSet>();
            var set = new EffectStateRatioSet
            {
                StartState = new EffectStateStructure(),
                EndState = new EffectStateStructure(),
                StartFrame = 0,
                EndFrame = 1
            };
            set.SetDefaultMaker();
            Sets.Add(0, set);
        }

        /// <summary>
        /// 再生する
        /// </summary>
        /// <param name="playtype">再生タイプ</param>
        public void Play(PlayType playtype)
        {
            this.playType = playtype;
            state = PlayState.Playing;
            stopwatch.Start();
        }

        /// <summary>
        /// 停止する
        /// </summary>
        public void Stop()
        {
            state = PlayState.Stop;
            stopwatch.Stop();
            stopwatch.Reset();
            currentFrame = 0;
        }

        /// <summary>
        /// 一時停止する
        /// </summary>
        public void Pause()
        {
            state = PlayState.Pause;
            stopwatch.Stop();
        }

        /// <summary>
        /// シークする
        /// </summary>
        /// <param name="frame"></param>
        public void Seek(float frame)
        {
            currentFrame = frame;
            InnerUpdate();
        }

        /// <summary>
        /// 更新する
        /// </summary>
        /// <param name="parentFrame">親のフレーム</param>
        /// <param name="parentFps">親のFPS</param>
        /// <param name="parentState">親の状態</param>
        public void Update(float parentFrame, float parentFps, EffectStateStructure parentState)
        {
            currentFrame = parentFrame;
            InnerUpdate();
        }

        /// <summary>
        /// 更新する
        /// </summary>
        /// <param name="parentFrame">親のフレーム</param>
        /// <param name="parentState">親の状態</param>
        public void Update(float parentFrame, EffectStateStructure parentState)
        {
            currentFrame = parentFrame;
            InnerUpdate();
        }

        /// <summary>
        /// 更新する
        /// </summary>
        public void Update()
        {
            if (state != PlayState.Playing) return;
            switch (playType)
            {
                case PlayType.Once:
                    currentFrame = FPS * stopwatch.ElapsedMilliseconds / 1000;
                    break;
                case PlayType.ReverseOnce:
                    currentFrame = FrameLength - FPS * stopwatch.ElapsedMilliseconds / 1000;
                    break;
                case PlayType.Loop:
                    currentFrame = NormalizeValue(FPS * stopwatch.ElapsedMilliseconds / 1000, FrameLength);
                    break;
                case PlayType.ReverseLoop:
                    currentFrame = ReverseValue(NormalizeValue(FPS * stopwatch.ElapsedMilliseconds / 1000, FrameLength * 2), FrameLength);
                    break;
            }
            if (currentFrame > FrameLength && playType == PlayType.Once)
            {
                state = PlayState.Stop;
                stopwatch.Stop();
                stopwatch.Reset();
                currentFrame = FrameLength;
                if (Finish != null) Finish.Invoke(this, EventArgs.Empty);
            }
            else if (currentFrame < 0 && playType == PlayType.ReverseOnce)
            {
                state = PlayState.Stop;
                stopwatch.Stop();
                stopwatch.Reset();
                currentFrame = 0;
                if (Finish != null) Finish.Invoke(this, EventArgs.Empty);
            }
            InnerUpdate();
        }

        private void InnerUpdate()
        {
            EffectStateRatioSet set = Sets.Values[BinaryFinder.FindNearest(Sets.Keys, ref currentFrame)];
            CurrentState = set.StartState.GetMixedState(set.GetRatios(CurrentFrame), set.EndState);
            Parallel.ForEach(Effects, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, effect =>
            {
                if (effect.Effects.Count == 0) effect.Update(currentFrame, CurrentState);
                else effect.Update(currentFrame * effect.FPS / FPS, CurrentState);
            });
        }

        private float NormalizeValue(float val, float baseval)
        {
            val = (float)Math.IEEERemainder(val, baseval);
            if (val < 0) val += baseval;
            return val;
        }

        private float ReverseValue(float val, float baseval)
        {
            if (val > baseval) return baseval * 2 - val;
            else return val;
        }

        /// <summary>
        /// 描画する
        /// </summary>
        /// <param name="callback">描画コールバック</param>
        public void Draw(DrawEffectCallBack callback)
        {
            foreach (IEffect effect in Effects)
            {
                effect.Draw(callback);
            }
        }

        /// <summary>
        /// 全体の長さを調べる
        /// </summary>
        public void CheckFrameLength()
        {
            int min = int.MaxValue, max = int.MinValue;
            foreach (IEffect effect in Effects)
            {
                effect.CheckFrameLength();
            }
            foreach (IEffect effect in Effects)
            {
                min = Math.Min(min, effect.StartFrame);
                max = Math.Max(max, effect.FrameLength + effect.StartFrame);
            }
            StartFrame = min;
            FrameLength = max;
        }

        /// <summary>
        /// クローンメソッド
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var ret = new EffectManager
            {
                FPS = FPS,
                StartFrame = StartFrame,
                currentFrame = currentFrame,
                FrameLength = FrameLength
            };
            if (CurrentState != null) ret.CurrentState = (EffectStateStructure)CurrentState.Clone();
            ret.state = state;
            foreach (IEffect effect in Effects)
            {
                ret.Effects.Add((IEffect)effect.Clone());
            }
            EffectStateStructure effectState = null;
            foreach (KeyValuePair<int, EffectStateRatioSet> set in Sets)
            {
                if (!ret.Sets.ContainsKey(set.Key))
                {
                    var newset = (EffectStateRatioSet)set.Value.CloneExceptState();
                    if (effectState != null)
                    {
                        newset.StartState = effectState;
                    }
                    else
                    {
                        newset.StartState = (EffectStateStructure)set.Value.StartState.Clone();
                    }
                    newset.EndState = (EffectStateStructure)set.Value.EndState.Clone();
                    effectState = newset.EndState;
                    ret.Sets.Add(set.Key, newset);
                }
            }
            return ret;
        }

        /// <summary>
        /// 保存する
        /// </summary>
        /// <param name="filename">エフェクトパス</param>
        public void Save(string filename)
        {
            try
            {
                var setting = new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8,
                    Indent = true,
                    IndentChars = "   "
                };
                var writer = XmlWriter.Create(filename, setting);
                writer.WriteStartDocument();
                writer.WriteStartElement("Effects");
                writer.WriteStartAttribute("FPS");
                writer.WriteString(this.FPS.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndAttribute();
                foreach (IEffect effect in this.Effects)
                {
                    WriteEffect(effect, writer);
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Close();
            }
            catch (Exception e)
            {
                throw new Exception("An error ocurred while saving", e);
            }
        }

        private void WriteEffect(IEffect effect, XmlWriter writer)
        {
            if (effect.Effects.Count == 0)
            {
                if (effect is BaseEffect be)
                {
                    writer.WriteStartElement("Effect");
                    writer.WriteStartAttribute("FilePath");
                    writer.WriteString(Path.GetFileName(be.Filename));
                    writer.WriteEndAttribute();
                    writer.WriteStartAttribute("StartFrame");
                    writer.WriteString(effect.StartFrame.ToString(CultureInfo.InvariantCulture));
                    writer.WriteEndAttribute();
                    writer.WriteStartAttribute("FrameLength");
                    writer.WriteString(effect.FrameLength.ToString(CultureInfo.InvariantCulture));
                    writer.WriteEndAttribute();
                    WriteSets(effect.Sets, writer);
                    writer.WriteEndElement();
                }
            }
            else
            {
                if (effect is BaseEffect be)
                {
                    writer.WriteStartElement("EffectReference");
                    writer.WriteStartAttribute("FilePath");
                    writer.WriteString(Path.GetFileName(be.Filename));
                    writer.WriteEndAttribute();
                    writer.WriteStartAttribute("StartFrame");
                    writer.WriteString(be.StartFrame.ToString(CultureInfo.InvariantCulture));
                    writer.WriteEndAttribute();
                    writer.WriteStartAttribute("FrameLength");
                    writer.WriteString(be.FrameLength.ToString(CultureInfo.InvariantCulture));
                    writer.WriteEndAttribute();
                    WriteSets(be.Sets, writer);
                    writer.WriteEndElement();
                }
            }
        }

        private void WriteSets(SortedList<int, EffectStateRatioSet> sets, XmlWriter writer)
        {
            bool first = true;
            foreach (EffectStateRatioSet set in sets.Values)
            {
                if (first)
                {
                    WriteState(set.StartState, writer);
                }
                writer.WriteStartElement("RatioMakers");
                writer.WriteStartAttribute("StartFrame");
                writer.WriteString(set.StartFrame.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndAttribute();
                writer.WriteStartAttribute("EndFrame");
                writer.WriteString(set.EndFrame.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndAttribute();
                foreach (RatioType type in Utility.RatioTypeArray)
                {
                    WriteRaioMaker(set[type], type, writer);
                }
                writer.WriteEndElement();
                if (set.IsBezierPosition) WriteBezierPosition(set.BAnalyzer, writer);
                WriteState(set.EndState, writer);
                first = false;
            }
        }

        private void WriteBezierPosition(BezierAnalyzer BAnalyzer, XmlWriter writer)
        {
            writer.WriteStartElement("BezierPositions");
            foreach (BezierControlPoint bcp in BAnalyzer.BCPS)
            {
                writer.WriteStartElement("BezierPosition");
                writer.WriteStartAttribute("Position");
                writer.WriteString(bcp.Serialize());
                writer.WriteEndAttribute();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private void WriteState(EffectStateStructure state, XmlWriter writer)
        {
            writer.WriteStartElement("State");
            foreach (RatioType type in Utility.RatioTypeArray)
            {
                writer.WriteStartAttribute(type.ToString());
                writer.WriteString(state[type].ToString(CultureInfo.InvariantCulture));
                writer.WriteEndAttribute();
            }
            writer.WriteStartAttribute("Blend");
            writer.WriteString(state.BlendMode.ToString());
            writer.WriteEndAttribute();
            writer.WriteEndElement();
        }

        private void WriteRaioMaker(IRatioMaker ratiomaker, RatioType type, XmlWriter writer)
        {
            writer.WriteStartElement("RatioMaker");
            writer.WriteStartAttribute("Type");
            writer.WriteString(type.ToString());
            writer.WriteEndAttribute();
            writer.WriteStartAttribute("MakerType");
            switch (ratiomaker.GetType().Name)
            {
                case "ConstantRatioMaker":
                    writer.WriteString("ConstantRatioMaker");
                    break;
                case "BezierRatioMaker":
                    var brm = ratiomaker as BezierRatioMaker;
                    writer.WriteString("BezierRatioMaker");
                    writer.WriteEndAttribute();
                    writer.WriteStartAttribute("P1");
                    writer.WriteString(brm.Analyzer.BCPS[0].Serialize());
                    writer.WriteEndAttribute();
                    writer.WriteStartAttribute("P2");
                    writer.WriteString(brm.Analyzer.BCPS[1].Serialize());
                    break;
                case "LinearRatioMaker":
                    writer.WriteString("LinearRatioMaker");
                    break;
                default:
                    writer.WriteString("");
                    break;
            }
            writer.WriteEndAttribute();
            writer.WriteEndElement();
        }
    }
}
