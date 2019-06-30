using PPDCoreModel;
using PPDCoreModel.Data;
using PPDFramework;
using PPDFramework.PPDStructure.EVDData;
using PPDFramework.PPDStructure.PPDData;
using SharpDX;
using System;
using System.Collections.Generic;

namespace PPDCore
{
    class NormalExMark : ExMarkBase
    {
        PictureObject outCircle;
        PictureObject outAxis;
        NormalExMarkGage gage;
        Vector2[] circlePoints;

        public NormalExMark(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, MarkImagePathsBase filenames, ExMarkData emd, ButtonType type, float[] eval,
            float gap, Vector2[] circlepoints, DisplayState dState, NoteType noteType, int sameTimingMarks, KeyValuePair<string, string>[] parameters, PostDrawManager postDrawManager)
            : base(device, resourceManager, filenames, emd, type, eval, gap, dState, noteType, sameTimingMarks, parameters, postDrawManager)
        {
            this.circlePoints = circlepoints;
        }

        public override void Initialize(CreateMarkFunc createMark, GetScriptAllowedButtonsFunc getScriptAllowedButtons)
        {
            base.Initialize(createMark, getScriptAllowedButtons);
            filenames.GetLongNoteCircleInfo(out PathObject fn, out float a, out float b);
            outCircle = new PictureObject(device, resourceManager, fn, true);
            outAxis = new PictureObject(device, resourceManager, filenames.GetCircleAxisImagePath(), true);
            outCircle.Scale = outAxis.Scale = Vector2.Zero;
            gage = new NormalExMarkGage(device, trace, circlePoints)
            {
                Hidden = true
            };
            this.AddChild(outAxis);
            this.AddChild(outCircle);
            this.AddChild(gage);
        }

        public override Vector2 CalculateColorPosition(ColorMarkInfo colorMarkInfo, IMarkInfo markInfo, float markTime, float currentTime, float bpm)
        {
            var vec = base.CalculateColorPosition(colorMarkInfo, markInfo, markTime, currentTime, bpm);

            if (markTime < currentTime)
            {
                return Vector2.Zero;
            }
            return vec;
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
                                break;
                            case EffectType.Fine:
                                OnPrcessEvaluate(false, false, EffectType.Fine, effectCallback);
                                results.Add(EffectType.Fine);
                                exState = ExState.Pressed;
                                break;
                            case EffectType.Safe:
                                OnPrcessEvaluate(false, false, EffectType.Safe, effectCallback);
                                results.Add(EffectType.Safe);
                                state = State.Safe;
                                exState = ExState.Pressed;
                                ret = true;
                                break;
                            case EffectType.Sad:
                                OnPrcessEvaluate(false, false, EffectType.Sad, effectCallback);
                                results.Add(EffectType.Sad);
                                state = State.Sad;
                                exState = ExState.Pressed;
                                ret = true;
                                break;
                            case EffectType.Worst:
                                OnPrcessEvaluate(false, false, EffectType.Worst, effectCallback);
                                results.Add(EffectType.Worst);
                                state = State.Worst;
                                exState = ExState.Pressed;
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
                    }
                }
                else if (exState == ExState.Pressed)
                {
                    if (ProcessReleasedButton(released))
                    {
                        UnsetReleasedButton(button);
                        var pressEval = EvaluateImpl(endTime, currentTime - adjustGapTime);
                        switch (pressEval)
                        {
                            // final evaluate
                            case EffectType.Cool:
                                results.Add(EffectType.Cool);
                                state = State.Cool;
                                OnPrcessEvaluate(false, true, EffectType.Cool, effectCallback);
                                exState = ExState.Released;
                                ret = true;
                                break;
                            case EffectType.Fine:
                                results.Add(EffectType.Fine);
                                state = State.Fine;
                                OnPrcessEvaluate(false, true, EffectType.Fine, effectCallback);
                                exState = ExState.Released;
                                ret = true;
                                break;
                            case EffectType.Safe:
                                results.Add(EffectType.Safe);
                                state = State.Safe;
                                OnPrcessEvaluate(false, true, EffectType.Safe, effectCallback);
                                exState = ExState.Released;
                                ret = true;
                                break;
                            case EffectType.Sad:
                                results.Add(EffectType.Sad);
                                state = State.Sad;
                                OnPrcessEvaluate(false, true, EffectType.Sad, effectCallback);
                                exState = ExState.Released;
                                ret = true;
                                break;
                            case EffectType.Worst:
                                results.Add(EffectType.Worst);
                                OnPrcessEvaluate(false, true, EffectType.Worst, null);
                                state = State.Worst;
                                exState = ExState.Released;
                                ret = true;
                                break;
                            case 0:
                                results.Add(EffectType.Worst);
                                OnPrcessEvaluate(false, true, EffectType.Worst, null);
                                state = State.Worst;
                                exState = ExState.Released;
                                ret = true;
                                break;
                        }
                        if (state >= State.Cool && state <= State.Sad)
                        {
                            //離したとき用の音
                            soundType = (int)ButtonType;
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
                else if (exState == ExState.Pressed)
                {
                    if (currentTime - endTime >= eval[3])
                    {
                        OnPrcessEvaluate(false, true, EffectType.Worst, null);
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
                        results.Add(EffectType.Cool);
                        exState = ExState.Pressed;
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
                    if (currentTime - endTime >= -AutoWidth)
                    {
                        results.Add(EffectType.Cool);
                        exState = ExState.Released;
                        soundType = (int)ButtonType + 10;
                        state = State.Cool;
                        OnPrcessEvaluate(false, true, EffectType.Cool, effectCallback);
                        Hidden = false;
                        ColorHidden = false;
                        ret = true;
                    }
                }
            }
            float scale = (float)bpm / DefaultBpm;
            if (state < State.Cool)
            {
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
                    outCircle.Scale = new Vector2(sx, sx);
                    outAxis.Scale = new Vector2(sx, sx);
                    if (mark.Scale.X > 1.2f)
                    {
                        mark.Scale = Vector2.One;
                        outCircle.Scale = Vector2.One;
                        outAxis.Scale = Vector2.One;
                        state = State.Moving;
                    }
                }
                UpdateColorPosition(currentTime, bpm);
                if (Time < currentTime)
                {
                    float tempscale = axis.Scale.X - 0.05f;
                    if (tempscale < 0)
                    {
                        axis.Scale = Vector2.Zero;                     }
                    else
                    {
                        axis.Scale = new Vector2(tempscale, tempscale);
                    }
                }
                axis.Rotation = -1 * (float)Math.PI * (Time - currentTime) * scale;
                if (axis.Rotation >= 0)
                {
                    axis.Rotation = 0;
                }
                outAxis.Rotation = -2 * (float)Math.PI * (Time - currentTime) / length;
            }
            colorMarkInfo.DrawTrace = (state == State.Moving || state == State.Appearing) && exState == ExState.Waiting;
            mark.Hidden = axis.Hidden = !(state < State.Cool);
            colorMarkInfo.ColorMark.Hidden = ColorHidden || !(state < State.Cool);
            colorMarkInfo.Hidden = ColorHidden;
            gage.Hidden = !(exState == ExState.Pressed && lastTime >= Time);
            gage.DisplayCount = (int)(Math.Min(1, (lastTime - Time) / length) * circlePoints.Length / 2);
            return ret;
        }

        protected override void OnPrcessEvaluate(bool missPress, bool released, EffectType effectType, MarkManager.EvaluateEffectHandler effectcallback)
        {
            base.OnPrcessEvaluate(missPress, released, effectType, released ? effectcallback : null);
        }
    }
}
