using PPDCoreModel.Data;
using PPDFramework;
using PPDFramework.PPDStructure.EVDData;
using PPDFramework.PPDStructure.PPDData;
using SharpDX;
using System;
using System.Collections.Generic;

namespace PPDCore
{
    class HoldExMark : ExMarkBase
    {
        public HoldExMark(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, MarkImagePathsBase filenames, ExMarkData emd, ButtonType type, float[] eval,
            float gap, DisplayState dState, NoteType noteType, int sameTimingMarks, KeyValuePair<string, string>[] parameters, PostDrawManager postDrawManager)
            : base(device, resourceManager, filenames, emd, type, eval, gap, dState, noteType, sameTimingMarks, parameters, postDrawManager)
        {
        }

        public override void Initialize(CreateMarkFunc createMark, GetScriptAllowedButtonsFunc getScriptAllowedButtons)
        {
            base.Initialize(createMark, getScriptAllowedButtons);
            if (!isMarkCreated)
            {
                var markIndex = -1;
                markIndex = this.IndexOf(mark);
                this.RemoveChild(mark);
                mark.Dispose();
                mark = new SpriteObject(device)
                {
                    Scale = Vector2.Zero
                };
                filenames.GetHoldInfo(out PathObject fn, out float x, out float y);
                mark.AddChild(new PictureObject(device, resourceManager, fn, true)
                {
                    Position = new Vector2(x, y)
                });
                mark.AddChild(new PictureObject(device, resourceManager, filenames.GetMarkImagePath(ButtonType), true));
                this.InsertChild(mark, markIndex);
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
                        if (results[EffectType.Cool] || results[EffectType.Fine])
                        {
                            state = State.Cool;
                            Hidden = true;
                        }
                    }
                }
                else if (exState == ExState.Pressed)
                {
                    if (released[(int)usedButton])
                    {
                        results.Add(EffectType.PressReleased);
                        ret = true;
                    }
                    else
                    {
                        results.Add(EffectType.Pressing);
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
                        results.Add(EffectType.Cool);
                        exState = ExState.Pressed;
                        soundType = (int)ButtonType;
                        state = State.Cool;
                        Hidden = true;
                        Array.Clear(colorMarkInfo.Pos1, 0, colorMarkInfo.Pos1.Length);
                        Array.Clear(colorMarkInfo.Pos2, 0, colorMarkInfo.Pos2.Length);
                        colorMarkInfo.VerticesCount = 0;
                    }
                }
                else if (exState == ExState.Pressed)
                {
                    results.Add(EffectType.Pressing);
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
            colorMarkInfo.DrawTrace = (state == State.Moving || state == State.Appearing) && exState == ExState.Waiting;
            mark.Hidden = axis.Hidden = !(state < State.Cool);
            colorMarkInfo.ColorMark.Hidden = ColorHidden || !(state < State.Cool);
            colorMarkInfo.Hidden = ColorHidden;
            return ret;
        }
    }
}