using System;
using System.Collections.Generic;
using System.Text;

using SlimDX;
using SlimDX.Direct3D9;
using PPDFramework;
using PPDFramework.PPDStructure.EVDData;
using PPDFramework.PPDStructure.PPDData;

namespace PPD
{
    class ExMark : Mark
    {
        public enum ExState
        {
            waitingpress = 0,
            pressok = 1,
            released = 2
        }
        ExState exstate = ExState.waitingpress;
        float endtime;
        float lasttime;
        float length;
        PictureObject outcircle;
        PictureObject outjiku;
        PictureObject hold;
        Vector2[] circlepoints;
        float alpha = 0.7f;
        public ExMark(Device device, PPDFramework.Resource.ResourceManager resourceManager, CMarkImagePaths filenames, ExMarkData emd, ButtonType type, float[] eval, float gap, Vector2[] circlepoints, DisplayState dState, bool AC)
            : base(device, resourceManager, filenames, emd, type, eval, gap, dState, AC)
        {
            this.endtime = emd.EndTime;
            this.circlepoints = circlepoints;
            length = endtime - time;
            Initialize();
        }
        public ExMark(Device device, PPDFramework.Resource.ResourceManager resourceManager, CMarkImagePaths filenames, ExMarkData emd, float[] eval, float gap, Vector2[] circlepoints, DisplayState dState, bool AC)
            : base(device, resourceManager, filenames, emd, eval, gap, dState, AC)
        {
            this.endtime = emd.EndTime;
            this.circlepoints = circlepoints;
            length = endtime - time;
            Initialize();
        }

        protected override void Initialize()
        {
            base.Initialize();
            string fn;
            float a, b;
            filenames.GetLongNoteCircleInfo(out fn, out a, out b);
            outcircle = new PictureObject(fn, x, y, true, resourceManager, device);
            outjiku = new PictureObject(filenames.GetCircleAxisImagePath(), x, y, true, resourceManager, device);
            outcircle.Scale = new Vector2(0, 0);
            outjiku.Scale = new Vector2(0, 0);
            filenames.GetHoldInfo(out fn, out  a, out b);
            hold = new PictureObject(fn, x + a, y + b, true, resourceManager, device);
            hold.Scale = new Vector2(0, 0);
        }

        public bool ExUpdate(float ctime, float bp, ref bool[] button, ref bool[] released, ref MarkManager.EffectType check, bool auto, ref int soundtype, MarkManager.EvaluateEffectHandler effectcallback)
        {
            bool ret = false;
            lasttime = ctime;
            //base.Update(ctime, bp, ref button, ref check, auto, ref soundtype);
            if (!auto)
            {
                if (exstate == ExState.waitingpress)
                {
                    if (button[(int)ButtonType])
                    {
                        int a = evaluate(this.time, ctime - adjustgaptime);
                        if (a > 1)
                        {
                            button[(int)ButtonType] = false;
                        }
                        switch (a)
                        {
                            // if more than safe,ok
                            case 5:
                                check = MarkManager.EffectType.Cool;
                                exstate = ExState.pressok;
                                break;
                            case 4:
                                check = MarkManager.EffectType.Fine;
                                exstate = ExState.pressok;
                                break;
                            case 3:
                                check = MarkManager.EffectType.Safe;
                                state = State.safe;
                                exstate = ExState.released;
                                ret = true;
                                break;
                            case 2:
                                check = MarkManager.EffectType.Sad;
                                state = State.sad;
                                exstate = ExState.released;
                                ret = true;
                                break;
                            case 1:
                                check = MarkManager.EffectType.Worst;
                                state = State.worst;
                                exstate = ExState.released;
                                ret = true;
                                break;
                            case 0:
                                break;
                        }
                        if (a != 0)
                        {
                            hidden = false;
                            pos1 = new Vector2[40];
                            pos2 = new Vector2[40];
                            iVerticesCount = 0;
                        }
                        if (AC && (check == MarkManager.EffectType.Cool || check == MarkManager.EffectType.Fine))
                        {
                            state = State.cool;
                            effectcallback.Invoke(MarkManager.EffectType.Cool, Position);
                            hidden = true;
                        }
                    }
                }
                else if (exstate == ExState.pressok)
                {
                    if (AC)
                    {
                        if (released[(int)ButtonType])
                        {
                            check = MarkManager.EffectType.PressReleased;
                            ret = true;
                        }
                        else
                        {
                            check = MarkManager.EffectType.Pressing;
                        }
                    }
                    else
                    {
                        if (released[(int)ButtonType])
                        {
                            released[(int)ButtonType] = false;
                            int a = evaluate(this.endtime, ctime - adjustgaptime);
                            switch (a)
                            {
                                // final evaluate
                                case 5:
                                    check = MarkManager.EffectType.Cool;
                                    state = State.cool;
                                    effectcallback.Invoke(MarkManager.EffectType.Cool, Position);
                                    exstate = ExState.released;
                                    ret = true;
                                    break;
                                case 4:
                                    check = MarkManager.EffectType.Fine;
                                    state = State.fine;
                                    effectcallback.Invoke(MarkManager.EffectType.Fine, Position);
                                    exstate = ExState.released;
                                    ret = true;
                                    break;
                                case 3:
                                    check = MarkManager.EffectType.Safe;
                                    state = State.safe;
                                    effectcallback.Invoke(MarkManager.EffectType.Safe, Position);
                                    exstate = ExState.released;
                                    ret = true;
                                    break;
                                case 2:
                                    check = MarkManager.EffectType.Sad;
                                    state = State.sad;
                                    effectcallback.Invoke(MarkManager.EffectType.Sad, Position);
                                    exstate = ExState.released;
                                    ret = true;
                                    break;
                                case 1:
                                    check = MarkManager.EffectType.Worst;
                                    state = State.worst;
                                    exstate = ExState.released;
                                    ret = true;
                                    break;
                                case 0:
                                    check = MarkManager.EffectType.Worst;
                                    state = State.worst;
                                    exstate = ExState.released;
                                    ret = true;
                                    break;
                            }
                            if (state >= State.cool && state <= State.sad)
                            {
                                //離したとき用の音
                                soundtype = (int)ButtonType;
                            }
                        }
                    }
                }
                if (exstate == ExState.waitingpress)
                {
                    if (ctime - this.time >= eval[3])
                    {
                        check = MarkManager.EffectType.Worst;
                        ret = true;
                    }
                }
                else if (exstate == ExState.pressok)
                {
                    if (!AC)
                    {
                        if (ctime - this.endtime >= eval[3])
                        {
                            check = MarkManager.EffectType.Worst;
                            ret = true;
                        }
                    }
                }
            }
            else
            {
                //when auto
                if (exstate == ExState.waitingpress)
                {
                    if (ctime - this.time >= -autowidth)
                    {
                        check = MarkManager.EffectType.Cool;
                        exstate = ExState.pressok;
                        soundtype = (int)ButtonType;
                        if (AC)
                        {
                            state = State.cool;
                            effectcallback.Invoke(MarkManager.EffectType.Cool, Position);
                            hidden = true;
                        }
                        else
                        {
                            hidden = false;
                        }
                        pos1 = new Vector2[40];
                        pos2 = new Vector2[40];
                        iVerticesCount = 0;
                    }
                }
                else if (exstate == ExState.pressok)
                {
                    if (AC)
                    {
                        check = MarkManager.EffectType.Pressing;
                    }
                    else
                    {
                        if (ctime - this.endtime >= -autowidth)
                        {
                            check = MarkManager.EffectType.Cool;
                            exstate = ExState.released;
                            soundtype = (int)ButtonType + 10;
                            state = State.cool;
                            effectcallback.Invoke(MarkManager.EffectType.Cool, Position);
                            hidden = false;
                            ret = true;
                        }
                    }
                }
            }
            if (state < State.cool)
            {
                float scale = (float)bp / defaultbpm;
                if (hidden)
                {
                    if (this.time - ctime <= 2 / scale)
                    {
                        if (dState == DisplayState.Normal || dState == DisplayState.Hidden)
                        {
                            effectcallback.Invoke(MarkManager.EffectType.Appear, Position);
                            hidden = false;
                        }
                    }
                    if (this.time - ctime <= 0.5f / scale)
                    {
                        if (dState == DisplayState.Sudden)
                        {
                            effectcallback.Invoke(MarkManager.EffectType.Appear, Position);
                            hidden = false;
                        }
                    }
                    if (this.time - ctime <= 0.25f / scale)
                    {
                        if (dState == DisplayState.Hidden && exstate == ExState.waitingpress)
                        {
                            effectcallback.Invoke(MarkManager.EffectType.Appear, Position);
                            hidden = true;
                        }
                    }
                    if (this.endtime - ctime <= 0.25f / scale)
                    {
                        if (dState == DisplayState.Hidden)
                        {
                            effectcallback.Invoke(MarkManager.EffectType.Appear, Position);
                            hidden = true;
                        }
                    }
                }
                if (state == State.appearing && !hidden)
                {
                    float sx = mark.Scale.X + 0.05f;
                    mark.Scale = new Vector2(sx, sx);
                    outcircle.Scale = new Vector2(sx, sx);
                    outjiku.Scale = new Vector2(sx, sx);
                    hold.Scale = new Vector2(sx, sx);
                    if (mark.Scale.X > 1.2f)
                    {
                        mark.Scale = new Vector2(1, 1);
                        outcircle.Scale = new Vector2(1, 1);
                        outjiku.Scale = new Vector2(1, 1);
                        hold.Scale = new Vector2(1, 1);
                        state = State.moving;
                    }
                }
                CaliculateColorPosition(ctime, bp);
                jiku.Rotation = -1 * (float)Math.PI * (this.time - ctime) * scale;
                if (jiku.Rotation >= 0)
                {
                    jiku.Rotation = 0;
                }
                outjiku.Rotation = -2 * (float)Math.PI * (this.time - ctime) / length;
            }
            mark.Update();
            markc.Update();
            jiku.Update();
            outcircle.Update();
            outjiku.Update();
            hold.Update();
            return ret;
        }

        public override void CaliculateColorPosition(float ctime, float bpm)
        {
            float scale = (float)bpm / defaultbpm;
            float timediff = this.time - ctime;
            Vector2 vec = Position + OnChangeColorPosition(scale, timediff);
            Vector2 vec1 = new Vector2(vec.X + 10, vec.Y + 8 * (float)Math.Cos((this.time - ctime) * scale * Math.PI));
            Vector2 vec2 = new Vector2(vec.X + 10, vec.Y - 8 * (float)Math.Cos((this.time - ctime) * scale * Math.PI));
            vec1 = Vector2.TransformCoordinate(vec1, m);
            vec2 = Vector2.TransformCoordinate(vec2, m);
            if (this.time <= ctime)
            {
                vec = new Vector2(this.x, this.y);
                vec1 = new Vector2(vec.X + 10, vec.Y + 8);
                vec2 = new Vector2(vec.X + 10, vec.Y - 8);
                vec1 = Vector2.TransformCoordinate(vec1, m);
                vec2 = Vector2.TransformCoordinate(vec2, m);
            }
            if (iVerticesCount == 40)
            {
                /*Vector2[] tmp = new Vector2[40];
                Array.Copy(pos, 1, tmp, 0, 39);
                tmp[39] = vec;
                pos = tmp;*/
                Array.ConstrainedCopy(pos1, 1, pos1, 0, 39);
                pos1[39] = vec1;
                Array.ConstrainedCopy(pos2, 1, pos2, 0, 39);
                pos2[39] = vec2;
            }
            else
            {
                pos1[iVerticesCount] = vec1;
                pos2[iVerticesCount] = vec2;
                iVerticesCount++;
            }

            if (this.time >= ctime)
            {
                vec = Vector2.TransformCoordinate(vec, m);
                markc.Position = vec;
            }
            else
            {
                markc.Position = new Vector2(x, y);
                float tempscale = jiku.Scale.X - 0.05f;
                if (tempscale < 0)
                {
                    jiku.Scale = new Vector2(0, 0);
                }
                else
                {
                    jiku.Scale = new Vector2(tempscale, tempscale);
                }
            }
        }

        public override void Draw()
        {
            //base.Draw();
            if (hidden) return;
            if (state < State.cool)
            {
                if (!PPDSetting.Setting.DrawSameColorAtSameTimingDisabled && simultaneoustype != 0)
                {
                    device.SetTexture(1, simpic.Texture);
                    device.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.Modulate);
                    device.SetTextureStageState(1, TextureStage.ColorArg1, TextureArgument.Current);
                    device.SetTextureStageState(1, TextureStage.ColorArg2, TextureArgument.Texture);
                    mark.Draw();
                    if (AC)
                    {
                        hold.Draw();
                    }
                    device.SetTexture(1, null);
                    jiku.Draw();
                }
                else
                {
                    mark.Draw();
                    if (AC)
                    {
                        hold.Draw();
                    }
                    jiku.Draw();
                }
            }
        }
        public override void colorDraw()
        {
            //base.colorDraw();
            if (hidden) return;
            if (state == State.moving)
            {
                drawtrace(pos1);
                drawtrace(pos2);
            }
            if (state < State.cool)
            {
                markc.Draw();
            }
        }
        public void gageDraw()
        {
            if (hidden || AC) return;
            if (exstate == ExState.waitingpress)
            {
                outcircle.Draw();
                outjiku.Draw();
            }
            else if (exstate == ExState.pressok)
            {
                if (lasttime >= this.time)
                {
                    drawgage();
                }
                outcircle.Draw();
                outjiku.Draw();
            }
            else if (exstate == ExState.released)
            {
            }

        }
        private void drawgage()
        {
            int num = circlepoints.Length / 2;
            float ratio = (lasttime - this.time) / length;
            if (ratio >= 1) ratio = 1;
            int displaynum = (int)(num * ratio);
            if (displaynum == 0) return;
            ColoredTexturedVertex[] fwv = new ColoredTexturedVertex[displaynum * 2];
            for (int i = 0; i < displaynum; i++)
            {
                fwv[i * 2].Position = new Vector3(x + circlepoints[i * 2].X, y + circlepoints[i * 2].Y, 0.5f);
                fwv[i * 2].Color = new Color4(alpha, 1, 1, 1).ToArgb();
                fwv[i * 2 + 1].Position = new Vector3(x + circlepoints[i * 2 + 1].X, y + circlepoints[i * 2 + 1].Y, 0.5f);
                fwv[i * 2 + 1].Color = new Color4(alpha, 1, 1, 1).ToArgb();
            }
            Texture t = trace.Texture;
            device.SetTexture(0, t);
            device.VertexFormat = ColoredTexturedVertex.Format; // 頂点フォーマットを設定
            // レンダーステート 深度バッファ無効
            //device.SetRenderState(RenderState.ZEnable, false);
            // レンダーステート カリングなし
            //device.SetRenderState(RenderState.CullMode,Cull.None);
            device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, displaynum * 2 - 2, fwv);
            //device.SetRenderState(RenderState.CullMode, Cull.Clockwise);
        }

        public ExState ExMarkState
        {
            get
            {
                return exstate;
            }
        }
    }
}
