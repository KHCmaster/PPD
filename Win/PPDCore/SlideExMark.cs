using PPDCoreModel.Data;
using PPDFramework;
using PPDFramework.PPDStructure.EVDData;
using PPDFramework.PPDStructure.PPDData;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PPDCore
{
    class SlideExMark : ExMarkBase
    {
        public const float ExFrameSec = 1 / 24f;
        public const float ExFrameInvert = 24;
        const int ExMarkPosDiff = 15;
        const int ExMarkPosDiffStart = 24;
        GameComponent[] exMarks;
        SlideColorMarkInfo[] colorExMarkInfos;
        float lastPressedTime = float.MaxValue;
        float slideScale;

        public ColorMarkInfo FirstSlideColorMarkInfo
        {
            get
            {
                if (colorExMarkInfos.Length == 0)
                {
                    return colorMarkInfo;
                }
                return colorExMarkInfos[0];
            }
        }

        public ColorMarkInfo LastSlideColorMarkInfo
        {
            get
            {
                if (colorExMarkInfos.Length == 0)
                {
                    return colorMarkInfo;
                }
                return colorExMarkInfos[colorExMarkInfos.Length - 1];
            }
        }

        public SlideExMark(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, MarkImagePathsBase filenames, ExMarkData emd, ButtonType type, float[] eval,
            float gap, DisplayState dState, NoteType noteType, float slideScale, int sameTimingMarks, KeyValuePair<string, string>[] parameters, PostDrawManager postDrawManager)
            : base(device, resourceManager, filenames, emd, type, eval, gap, dState, noteType, sameTimingMarks, parameters, postDrawManager)
        {
            this.slideScale = slideScale;
        }

        public override void Initialize(CreateMarkFunc createMark, GetScriptAllowedButtonsFunc getScriptAllowedButtons)
        {
            base.Initialize(createMark, getScriptAllowedButtons);

            exMarks = new GameComponent[ExCount];
            colorExMarkInfos = new SlideColorMarkInfo[ExCount];
            int diff = IsRight ? ExMarkPosDiff : -ExMarkPosDiff;
            int currentDiff = IsRight ? ExMarkPosDiffStart : -ExMarkPosDiffStart;
            for (int i = 0; i < exMarks.Length; i++)
            {
                createMark(this, out GameComponent mark, out GameComponent colorMark, out GameComponent axis, out GameComponent slide, out GameComponent slideColor, out PictureObject trace);
                exMarks[i] = slide ?? new PictureObject(device, resourceManager, filenames.GetMarkImagePath(GetButtonType() + 2), true);
                exMarks[i].Position = new Vector2(currentDiff, 0);
                colorExMarkInfos[i] = new SlideColorMarkInfo(
                    device, this,
                    slideColor ?? new PictureObject(device, resourceManager, filenames.GetMarkColorImagePath(GetButtonType() + 2), true),
                    exMarks[i].Position, trace ?? this.trace, Time + ExFrameSec / slideScale * (i + 1))
                {
                    PostDrawManager = postDrawManager
                };
                colorExMarkInfos[i].ColorMark.PostDrawManager = postDrawManager;
                currentDiff += diff;
            }
            var markIndex = this.IndexOf(mark);
            for (var i = 0; i < exMarks.Length; i++)
            {
                this.InsertChild(exMarks[i], i + markIndex);
            }
            for (var i = 0; i < exMarks.Length; i++)
            {
                this.InsertChild(colorExMarkInfos[i].ColorMark, i + 1);
            }
            for (var i = 0; i < exMarks.Length; i++)
            {
                this.AddChild(colorExMarkInfos[i]);
            }
        }

        public override bool ExUpdate(float currentTime, float bpm, ref bool[] button, ref bool[] released,
            MarkResults results, bool auto, ref int soundType, MarkManager.EvaluateEffectHandler effectCallback)
        {
            if (fixedBPM >= 0)
            {
                bpm = fixedBPM;
            }
            bool ret = false;
            lastTime = currentTime;
            if (!auto)
            {
                if (exState == ExState.Waiting)
                {
                    if (ProcessPressedButton(button))
                    {
                        var pressEval = EvaluateImpl(Time, currentTime - adjustGapTime);
                        if (pressEval > EffectType.Worst)
                        {
                            UnsetPressedButton(button);
                        }
                        switch (pressEval)
                        {
                            // if more than safe,ok
                            case EffectType.Cool:
                                OnPrcessEvaluate(false, false, EffectType.Cool, effectCallback);
                                results.Add(EffectType.Cool);
                                exState = ExState.Pressed;
                                lastPressedTime = currentTime;
                                break;
                            case EffectType.Fine:
                                OnPrcessEvaluate(false, false, EffectType.Fine, effectCallback);
                                results.Add(EffectType.Fine);
                                exState = ExState.Pressed;
                                lastPressedTime = currentTime;
                                break;
                            case EffectType.Safe:
                                OnPrcessEvaluate(false, false, EffectType.Safe, effectCallback);
                                results.Add(EffectType.Safe);
                                state = State.Safe;
                                exState = ExState.Pressed;
                                lastPressedTime = currentTime;
                                ret = true;
                                break;
                            case EffectType.Sad:
                                OnPrcessEvaluate(false, false, EffectType.Sad, effectCallback);
                                results.Add(EffectType.Sad);
                                state = State.Sad;
                                exState = ExState.Pressed;
                                lastPressedTime = currentTime;
                                ret = true;
                                break;
                            case EffectType.Worst:
                                OnPrcessEvaluate(false, false, EffectType.Worst, effectCallback);
                                results.Add(EffectType.Worst);
                                state = State.Worst;
                                exState = ExState.Pressed;
                                lastPressedTime = currentTime;
                                ret = true;
                                break;
                            case 0:
                                break;
                        }
                        if (pressEval != EffectType.None)
                        {
                            Hidden = false;
                            ColorHidden = false;
                            Array.Clear(colorMarkInfo.Pos1, 0, colorMarkInfo.Pos1.Length);
                            Array.Clear(colorMarkInfo.Pos2, 0, colorMarkInfo.Pos2.Length);
                            colorMarkInfo.VerticesCount = 0;
                        }
                        if (!ret && exState == ExState.Pressed)
                        {
                            OnPrcessEvaluate(false, false, EffectType.SlideStart, effectCallback, FirstSlideColorMarkInfo.BasePosition + Position);
                        }
                    }
                }
                else if (exState == ExState.Pressed)
                {
                    if (ProcessReleasedButton(released))
                    {
                        var last = colorExMarkInfos.LastOrDefault(c => c.IsSlided);
                        OnPrcessEvaluate(false, false, EffectType.SlideEnd, effectCallback, (last ?? FirstSlideColorMarkInfo).BasePosition + Position);
                        UnsetReleasedButton(button);
                        ret = true;
                    }
                    if (!ret)
                    {
                        if (currentTime - endTime >= 0)
                        {
                            OnPrcessEvaluate(false, false, EffectType.SlideEnd, effectCallback, LastSlideColorMarkInfo.BasePosition + Position);
                            results.Add(EffectType.Slide);
                            exState = ExState.Released;
                            soundType = (int)ButtonType;
                            Hidden = false;
                            ColorHidden = false;
                            ret = true;
                        }
                    }
                }
                if (exState == ExState.Waiting)
                {
                    if (currentTime - Time >= eval[3])
                    {
                        OnPrcessEvaluate(false, false, EffectType.Worst, null);
                        results.Add(EffectType.Worst);
                        ret = true;
                    }
                }
            }
            else
            {
                //when auto
                if (exState == ExState.Waiting)
                {
                    if (currentTime - Time >= -AutoWidth)
                    {
                        OnPrcessEvaluate(false, false, EffectType.Cool, effectCallback);
                        OnPrcessEvaluate(false, false, EffectType.SlideStart, effectCallback, FirstSlideColorMarkInfo.BasePosition + Position);
                        results.Add(EffectType.Cool);
                        exState = ExState.Pressed;
                        lastPressedTime = currentTime;
                        soundType = (int)ButtonType;
                        Hidden = false;
                        ColorHidden = false;
                        Array.Clear(colorMarkInfo.Pos1, 0, colorMarkInfo.Pos1.Length);
                        Array.Clear(colorMarkInfo.Pos2, 0, colorMarkInfo.Pos2.Length);
                        colorMarkInfo.VerticesCount = 0;
                    }
                }
                else if (exState == ExState.Pressed)
                {
                    if (currentTime - endTime >= 0)
                    {
                        OnPrcessEvaluate(false, false, EffectType.SlideEnd, effectCallback, LastSlideColorMarkInfo.BasePosition + Position);
                        results.Add(EffectType.Slide);
                        exState = ExState.Released;
                        soundType = (int)ButtonType;
                        Hidden = false;
                        ColorHidden = false;
                        ret = true;
                    }
                }
            }
            float scale = (float)bpm / DefaultBpm;
            if (state < State.Cool)
            {
                if (state == State.Moving && currentTime - Time >= 0)
                {
                    state = State.Disappering;
                }
                if (state == State.Disappering)
                {
                    mark.Scale = axis.Scale = colorMarkInfo.ColorMark.Scale = new Vector2((eval[3] + Time - currentTime) / eval[3]);
                    if (mark.Scale.X <= 0)
                    {
                        mark.Scale = axis.Scale = colorMarkInfo.Scale = Vector2.Zero;
                    }
                }
                if (Hidden)
                {
                    if (Time - currentTime <= 0.5f / scale)
                    {
                        if (displayState == DisplayState.Sudden)
                        {
                            Hidden = false;
                            ColorHidden = false;
                            OnPrcessEvaluate(false, false, EffectType.Appear, effectCallback);
                            Array.Clear(colorMarkInfo.Pos1, 0, colorMarkInfo.Pos1.Length);
                            Array.Clear(colorMarkInfo.Pos2, 0, colorMarkInfo.Pos2.Length);
                            colorMarkInfo.VerticesCount = 0;
                        }
                    }
                    else if (Time - currentTime <= 2 / scale)
                    {
                        if (displayState == DisplayState.Normal || displayState == DisplayState.Hidden || displayState == DisplayState.HiddenColor || displayState == DisplayState.SuddenColor)
                        {
                            Hidden = false;
                            ColorHidden = false;
                            OnPrcessEvaluate(false, false, EffectType.Appear, effectCallback);
                            ColorHidden |= displayState == DisplayState.SuddenColor;
                        }
                    }
                }
                else
                {
                    if (Time - currentTime <= 0.25f / scale)
                    {
                        if (displayState == DisplayState.Hidden)
                        {
                            displayState = (DisplayState)(-1);
                            Hidden = true;
                        }
                        else ColorHidden |= (exState == ExState.Waiting && displayState == DisplayState.HiddenColor);
                    }
                    else if (Time - currentTime <= 0.5f / scale)
                    {
                        if (ColorHidden && displayState == DisplayState.SuddenColor)
                        {
                            Array.Clear(colorMarkInfo.Pos1, 0, colorMarkInfo.Pos1.Length);
                            Array.Clear(colorMarkInfo.Pos2, 0, colorMarkInfo.Pos2.Length);
                            colorMarkInfo.VerticesCount = 0;
                            ColorHidden = false;
                        }
                    }
                }
                if (state == State.Appearing && !Hidden)
                {
                    float sx = mark.Scale.X + 0.05f;
                    mark.Scale = new Vector2(sx, sx);
                    if (mark.Scale.X > 1.2f)
                    {
                        mark.Scale = Vector2.One;
                        state = State.Moving;
                    }
                }
                UpdateColorPosition(currentTime, bpm);
                axis.Rotation = -1 * (float)Math.PI * (Time - currentTime) * scale;
                if (axis.Rotation >= 0)
                {
                    axis.Rotation = 0;
                }
            }
            for (int i = 0; i < colorExMarkInfos.Length; i++)
            {
                switch (colorExMarkInfos[i].ShowState)
                {
                    case SlideColorMarkInfo.State.None:

                        if (colorExMarkInfos[i].MarkTime - currentTime <= 0.5f / scale)
                        {
                            if (displayState == DisplayState.Sudden)
                            {
                                colorExMarkInfos[i].ShowState = SlideColorMarkInfo.State.Appearing;
                                OnPrcessEvaluate(false, false, EffectType.Appear, effectCallback, colorExMarkInfos[i].BasePosition + Position);
                            }
                        }
                        if (colorExMarkInfos[i].MarkTime - currentTime <= 2 / scale)
                        {
                            if (displayState == DisplayState.Normal || displayState == DisplayState.Hidden || displayState == DisplayState.HiddenColor || displayState == DisplayState.SuddenColor)
                            {
                                colorExMarkInfos[i].ShowState = SlideColorMarkInfo.State.Appearing;
                                OnPrcessEvaluate(false, false, EffectType.Appear, effectCallback, colorExMarkInfos[i].BasePosition + Position);
                            }
                        }
                        break;
                    case SlideColorMarkInfo.State.Appearing:
                        float sx = exMarks[i].Scale.X + 0.05f;
                        exMarks[i].Scale = new Vector2(sx, sx);
                        if (exMarks[i].Scale.X > 1.2f)
                        {
                            exMarks[i].Scale = Vector2.One;
                            colorExMarkInfos[i].ShowState = SlideColorMarkInfo.State.Moving;
                        }
                        break;
                    case SlideColorMarkInfo.State.Moving:
                        if (currentTime - colorExMarkInfos[i].MarkTime >= 0)
                        {
                            colorExMarkInfos[i].ShowState = SlideColorMarkInfo.State.Disappearing;
                        }
                        break;
                    case SlideColorMarkInfo.State.Disappearing:
                        colorExMarkInfos[i].ColorMark.Scale = exMarks[i].Scale = new Vector2((eval[3] + colorExMarkInfos[i].MarkTime - currentTime) / eval[3]);
                        if (exMarks[i].Scale.X <= 0)
                        {
                            colorExMarkInfos[i].ColorMark.Scale = exMarks[i].Scale = Vector2.Zero;
                        }
                        break;
                }
                var vec = CalculateColorPosition(colorExMarkInfos[i], colorExMarkInfos[i].SlideMarkInfo, colorExMarkInfos[i].MarkTime, currentTime, bpm);
                UpdateColorMarkInfo(colorExMarkInfos[i], colorExMarkInfos[i].SlideMarkInfo, colorExMarkInfos[i].MarkTime, currentTime, bpm, vec);
                var prevVisible = !colorExMarkInfos[i].ColorMark.Hidden;
                colorExMarkInfos[i].DrawTrace = exState < ExState.Pressed || currentTime < colorExMarkInfos[i].MarkTime;
                colorExMarkInfos[i].ColorMark.Hidden = exMarks[i].Hidden = !colorExMarkInfos[i].DrawTrace;
                var currentVisible = !colorExMarkInfos[i].ColorMark.Hidden;
                if (prevVisible != currentVisible)
                {
                    if ((exState == ExState.Pressed || (results[EffectType.Slide] && ret)) && !colorExMarkInfos[i].IsSlided)
                    {
                        PressingFrames++;
                        OnPrcessEvaluate(false, false, EffectType.Slide, effectCallback, colorExMarkInfos[i].BasePosition + Position);
                        colorExMarkInfos[i].IsSlided = true;
                        if (i < colorExMarkInfos.Length - 1)
                        {
                            results.Add(EffectType.Slide);
                        }
                    }
                }
            }
            if (exState == ExState.Pressed)
            {
                if (currentTime - lastPressedTime >= ExFrameSec)
                {
                    lastPressedTime += ExFrameSec;
                    soundType = (int)ButtonType;
                }
            }
            if (ret && IsMaxSlide)
            {
                OnPrcessEvaluate(false, false, EffectType.MaxSlide, effectCallback, LastSlideColorMarkInfo.BasePosition + Position);
            }

            axis.Hidden = mark.Hidden = state >= State.Cool || exState >= ExState.Pressed;
            colorMarkInfo.ColorMark.Hidden = ColorHidden || state >= State.Cool || exState >= ExState.Pressed;
            colorMarkInfo.DrawTrace = (state == State.Moving || state == State.Appearing) && exState == ExState.Waiting;
            for (var i = 0; i < exMarks.Length; i++)
            {
                exMarks[i].Hidden |= colorExMarkInfos[i].ShowState == SlideColorMarkInfo.State.None || !(state < State.Cool);
            }
            for (var i = 0; i < colorExMarkInfos.Length; i++)
            {
                colorExMarkInfos[i].Hidden = Hidden || ColorHidden;
                colorExMarkInfos[i].ColorMark.Hidden |= Hidden || ColorHidden;
            }
            colorMarkInfo.Hidden = Hidden || ColorHidden;
            return ret;
        }

        public int ExCount
        {
            get
            {
                return (int)(ExFrameInvert * length * slideScale);
            }
        }

        public int PressingFrames
        {
            get;
            private set;
        }

        public int ProcessedPressingFrames
        {
            get;
            set;
        }

        public bool IsMaxSlide
        {
            get
            {
                return colorExMarkInfos.All(i => i.IsSlided);
            }
        }

        public Vector2 GetPressedSlidePosition(int pressingFrame)
        {
            return exMarks[pressingFrame - 1].Position + Position;
        }

        public override bool DoNotDrawConnect
        {
            get
            {
                return exState == ExState.Pressed;
            }
        }
        public override float SlideScale
        {
            get
            {
                return slideScale;
            }
        }
    }
}
