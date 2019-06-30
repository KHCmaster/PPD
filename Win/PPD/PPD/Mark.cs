using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using PPDFramework;
using PPDFramework.PPDStructure.PPDData;
using PPDFramework.PPDStructure.EVDData;


namespace PPD
{
    class Mark
    {
        public delegate Vector2 ColorPositionDelegate(float scale, float timediff);

        public event ColorPositionDelegate ChangeColorPosition;

        protected enum State
        {
            appearing = 0,
            moving = 1,
            disappering = 2,
            cool = 3,
            fine = 4,
            safe = 5,
            sad = 6,
            worst = 7
        }
        protected int simultaneoustype = 0;
        protected State state = State.appearing;
        protected const float defaultbpm = 130;
        protected const float autowidth = 1 / 120f;
        protected float x;
        protected float y;
        protected float angle;
        protected float time;
        protected DisplayState dState;
        protected Vector2[] pos1;
        protected Vector2[] pos2;
        protected CMarkImagePaths filenames;
        protected int iVerticesCount = 0;
        protected Device device;
        protected Matrix m;
        protected float[] eval;
        protected bool hidden;
        protected PictureObject mark;
        protected PictureObject markc;
        protected PictureObject jiku;
        protected PictureObject trace;
        protected PictureObject simpic;
        protected float adjustgaptime;
        protected PPDFramework.Resource.ResourceManager resourceManager;
        public Mark(Device device, PPDFramework.Resource.ResourceManager resourceManager, CMarkImagePaths filenames, MarkData md, ButtonType type, float[] eval, float gap, DisplayState dState, bool AC)
        {
            this.x = md.X;
            this.y = md.Y;
            this.angle = md.Angle;
            this.time = md.Time;
            this.resourceManager = resourceManager;
            this.device = device;
            this.eval = eval;
            this.filenames = filenames;
            this.adjustgaptime = gap;
            ButtonType = type;
            this.dState = dState;
            this.AC = AC;
            Initialize();
        }
        public Mark(Device device, PPDFramework.Resource.ResourceManager resourceManager, CMarkImagePaths filenames, MarkData md, float[] eval, float gap, DisplayState dState, bool AC)
        {
            this.x = md.X;
            this.y = md.Y;
            this.angle = md.Angle;
            this.time = md.Time;
            this.resourceManager = resourceManager;
            this.device = device;
            this.eval = eval;
            this.filenames = filenames;
            this.adjustgaptime = gap;
            ButtonType = md.ButtonType;
            this.dState = dState;
            this.AC = AC;
            Initialize();
        }
        protected virtual void Initialize()
        {
            mark = new PictureObject(filenames.GetMarkImagePath(ButtonType), x, y, true, resourceManager, device);
            markc = new PictureObject(filenames.GetMarkColorImagePath(ButtonType), x, y, true, resourceManager, device);
            jiku = new PictureObject(filenames.GetClockAxisImagePath(), x, y, true, resourceManager, device);
            trace = new PictureObject(filenames.GetTraceImagePath(ButtonType), x, y, false, resourceManager, device);
            this.m = Matrix.Transformation2D(new Vector2(0, 0), 0, new Vector2(1, 1), new Vector2(this.x, this.y), -angle, new Vector2(0, 0));
            hidden = true;
            mark.Scale = new Vector2(0, 0);
            pos1 = new Vector2[40];
            pos2 = new Vector2[40];
        }

        /// <summary>
        /// 5:cool ... 1:worst 0:nothing
        /// </summary>
        /// <param name="ctime"></param>
        /// <returns></returns>
        public int Evaluate(float ctime)
        {
            return evaluate(this.time, ctime);
        }

        protected int evaluate(float basetime, float ctime)
        {
            //5...cool
            //4...good
            //3...safe
            //2...sad
            //1...worst
            //0...not coming
            float difference = (float)Math.Abs(basetime - ctime);
            if (difference <= eval[3])
            {
                if (difference <= eval[2])
                {
                    if (difference <= eval[1])
                    {
                        if (difference <= eval[0])
                        {
                            return 5;
                        }
                        return 4;
                    }
                    return 3;
                }
                return 2;
            }
            if (basetime - ctime < 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public virtual bool Update(float ctime, float bp, ref bool[] button, ref MarkManager.EffectType check, bool auto, ref int soundtype, MarkManager.EvaluateEffectHandler effectcallback)
        {
            bool ret = false;
            if (!auto)
            {
                if (button[(int)ButtonType] && state < State.cool)
                {
                    int a = evaluate(this.time, ctime - adjustgaptime);
                    if (a > 1)
                    {
                        button[(int)ButtonType] = false;
                    }
                    switch (a)
                    {
                        case 5:
                            state = State.cool;
                            effectcallback.Invoke(MarkManager.EffectType.Cool, Position);
                            check = MarkManager.EffectType.Cool;
                            ret = true;
                            break;
                        case 4:
                            state = State.fine;
                            check = MarkManager.EffectType.Fine;
                            ret = true;
                            effectcallback.Invoke(MarkManager.EffectType.Fine, Position);
                            break;
                        case 3:
                            state = State.safe;
                            effectcallback.Invoke(MarkManager.EffectType.Safe, Position);
                            check = MarkManager.EffectType.Safe;
                            ret = true;
                            break;
                        case 2:
                            state = State.sad;
                            effectcallback.Invoke(MarkManager.EffectType.Sad, Position);
                            check = MarkManager.EffectType.Sad;
                            ret = true;
                            break;
                        case 1:
                            state = State.worst;
                            effectcallback.Invoke(MarkManager.EffectType.Cool, Position);
                            check = MarkManager.EffectType.Worst;
                            ret = true;
                            break;
                        case 0:
                            break;
                    }
                }
            }
            else
            {
                //when auto
                if (ctime - this.time >= -autowidth && state < State.cool)
                {
                    state = State.cool;
                    effectcallback.Invoke(MarkManager.EffectType.Cool, Position);
                    soundtype = (int)ButtonType;
                    check = MarkManager.EffectType.Cool;
                    ret = true;
                    hidden = false;
                }
            }
            if (ctime - this.time >= eval[3] && state < State.cool)
            {
                check = MarkManager.EffectType.Worst;
                ret = true;
            }
            if (state < State.cool)
            {
                if (state == State.moving && ctime - this.time >= 0)
                {
                    state = State.disappering;
                }
                if (state == State.disappering)
                {
                    float sx = mark.Scale.X;
                    mark.Scale = new Vector2((eval[3] + this.time - ctime) / eval[3], (eval[3] + this.time - ctime) / eval[3]);
                    jiku.Scale = new Vector2((eval[3] + this.time - ctime) / eval[3], (eval[3] + this.time - ctime) / eval[3]);
                    markc.Scale = new Vector2((eval[3] + this.time - ctime) / eval[3], (eval[3] + this.time - ctime) / eval[3]);
                    if (mark.Scale.X <= 0)
                    {
                        mark.Scale = new Vector2(0, 0);
                        jiku.Scale = new Vector2(0, 0);
                        markc.Scale = new Vector2(0, 0);
                    }
                }
                float scale = (float)bp / defaultbpm;
                if (hidden)
                {
                    if (this.time - ctime <= 2 / scale)
                    {
                        if (dState == DisplayState.Normal || dState == DisplayState.Hidden)
                        {
                            hidden = false;
                            effectcallback.Invoke(MarkManager.EffectType.Appear, Position);
                        }
                    }
                    if (this.time - ctime <= 0.5f / scale)
                    {
                        if (dState == DisplayState.Sudden)
                        {
                            hidden = false;
                            effectcallback.Invoke(MarkManager.EffectType.Appear, Position);
                            pos1 = new Vector2[40];
                            pos2 = new Vector2[40];
                            iVerticesCount = 0;
                        }
                    }
                    if (this.time - ctime <= 0.25f / scale)
                    {
                        if (dState == DisplayState.Hidden)
                        {
                            hidden = true;
                        }
                    }
                }
                if (state == State.appearing && !hidden)
                {
                    float sx = mark.Scale.X + 0.05f;
                    mark.Scale = new Vector2(sx, sx);
                    //uzu.Scale = new Vector2(sx / 4f, sx / 4f);
                    if (mark.Scale.X > 1.2f)
                    {
                        mark.Scale = new Vector2(1, 1);
                        state = State.moving;
                    }
                }
                CaliculateColorPosition(ctime, bp);
                jiku.Rotation = -1 * (float)Math.PI * (this.time - ctime) * scale;
                if (jiku.Rotation >= 0)
                {
                    jiku.Rotation = 0;
                }
            }
            mark.Update();
            markc.Update();
            jiku.Update();
            return ret;
        }

        public virtual void CaliculateColorPosition(float ctime, float bpm)
        {
            float scale = (float)bpm / defaultbpm;
            float timediff = this.time - ctime;
            Vector2 vec = Position + OnChangeColorPosition(scale, timediff);
            Vector2 vec1 = new Vector2(vec.X + 10, vec.Y + 8 * (float)Math.Cos((this.time - ctime) * scale * Math.PI));
            Vector2 vec2 = new Vector2(vec.X + 10, vec.Y - 8 * (float)Math.Cos((this.time - ctime) * scale * Math.PI));
            vec1 = Vector2.TransformCoordinate(vec1, m);
            vec2 = Vector2.TransformCoordinate(vec2, m);
            if (iVerticesCount == 40)
            {
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
            vec = Vector2.TransformCoordinate(vec, m);
            markc.Position = vec;
        }

        protected Vector2 OnChangeColorPosition(float scale, float timediff)
        {
            if (ChangeColorPosition != null)
            {
                return ChangeColorPosition.Invoke(scale, timediff);
            }
            else
            {
                return new Vector2(300 * timediff * scale, -30 * (float)Math.Sin(timediff * scale * Math.PI));
            }
        }

        public virtual void Draw()
        {
            if (hidden) return;
            if (state == State.appearing)
            {
                //uzu.Draw();
            }
            if (state < State.cool)
            {
                if (!PPDSetting.Setting.DrawSameColorAtSameTimingDisabled && simultaneoustype != 0)
                {
                    device.SetTexture(1, simpic.Texture);
                    device.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.Modulate);
                    device.SetTextureStageState(1, TextureStage.ColorArg1, TextureArgument.Current);
                    device.SetTextureStageState(1, TextureStage.ColorArg2, TextureArgument.Texture);
                    mark.Draw();
                    device.SetTexture(1, null);
                    jiku.Draw();
                }
                else
                {
                    mark.Draw();
                    jiku.Draw();
                }
            }


        }
        public virtual void colorDraw()
        {
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
        protected void drawtrace(Vector2[] pos)
        {
            int i, MAX_PLV = 80; // ループカウンタ
            Vector2 v, vD, vN1, vN2; // 計算用のベクタ

            ColoredTexturedVertex[] fwv = new ColoredTexturedVertex[MAX_PLV]; // 頂点データの配列
            int nFWVCount = 0; // 描画する頂点の数。初期値は0
            int k = 0; // 書き込み対象の配列インデックス
            float width = 2f;
            float alpha = 0f;
            vN1 = new Vector2(0, 0);
            vN2 = new Vector2(0, 0);
            // 頂点の数が2より小さい場合は描画できない。FALSEを返して終了
            if (iVerticesCount < 2)
                return;
            for (i = 0; i < iVerticesCount; i++)
            {
                // 最後の頂点以外の場合
                if (i < (iVerticesCount - 1))
                {
                    // 現在の頂点から次の頂点を指すベクトルをvとする
                    v.X = pos[i + 1].X - pos[i].X;
                    v.Y = pos[i + 1].Y - pos[i].Y;

                    if (v.X == 0 && v.Y == 0) continue;
                    // ベクトルvを正規化したものをベクトルvDとする
                    vD = Vector2.Normalize(v);

                    // vDに幅widthをかける
                    vD *= width;

                    // ベクトルvDを-90度回転させたものをベクトルvN1とする
                    vN1.X = vD.Y;
                    vN1.Y = -vD.X;

                    // ベクトルvDを90度回転させたものをベクトルvN2とする
                    vN2.X = -vD.Y;
                    vN2.Y = vD.X;
                }




                // 位置を設定
                fwv[k].Position = new Vector3(pos[i].X + vN1.X, pos[i].Y + vN1.Y, 0.5f);
                fwv[k].Color = new Color4(alpha, 1, 1, 1).ToArgb();




                ++k; // 配列インデックスをひとつインクリメント

                // 位置を設定
                fwv[k].Position = new Vector3(pos[i].X + vN2.X, pos[i].Y + vN2.Y, 0.5f);
                fwv[k].Color = new Color4(alpha, 1, 1, 1).ToArgb();

                k++;

                nFWVCount += 2;    // 描画する頂点の数が2つ増加
                width = 2f + 0.5f * (i + 1) / 40f;
                alpha = 0.95f * (i + 1) / 40f;
            }
            Texture t = trace.Texture;
            device.SetTexture(0, t);
            device.VertexFormat = ColoredTexturedVertex.Format; // 頂点フォーマットを設定
            // レンダーステート 深度バッファ無効
            //device.SetRenderState(RenderState.ZEnable, false);
            // レンダーステート カリングなし
            //device.SetRenderState(RenderState.CullMode,Cull.None);
            Array.Resize(ref fwv, nFWVCount);
            device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 0, nFWVCount - 2, fwv);
            //device.SetRenderState(RenderState.CullMode, Cull.Clockwise);



        }
        public Vector2 Position
        {
            get
            {
                return new Vector2(this.x, this.y);
            }
        }
        public int SimType
        {
            get
            {
                return simultaneoustype;
            }
            set
            {
                simultaneoustype = value;
                if (simultaneoustype != 0)
                {
                    simpic = new PictureObject(filenames.GetTraceImagePath((ButtonType)(SimType - 1)), 0f, 0f, resourceManager, device);
                }
            }
        }
        public float Time
        {
            get
            {
                return time;
            }
            set
            {
                time = value;
            }
        }

        public ButtonType ButtonType
        {
            get;
            private set;
        }

        public Vector2 ColorPosition
        {
            get
            {
                return markc.Position;
            }
            set
            {
                markc.Position = value;
            }
        }

        public bool Hidden
        {
            get
            {
                return hidden;
            }
        }

        public bool AC
        {
            get;
            private set;
        }
    }
}
