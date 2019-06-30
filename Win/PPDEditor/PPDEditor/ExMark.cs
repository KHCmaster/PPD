using PPDFramework;
using PPDFramework.PPDStructure.EVDData;
using PPDFramework.PPDStructure.PPDData;
using SharpDX;
using System;

namespace PPDEditor
{
    class ExMark : Mark
    {
        const float ExFrameSec = 1 / 24f;
        const float ExFrameInvert = 24;
        const int ExMarkPosDiff = 15;
        const int ExMarkPosDiffStart = 24;
        protected float endtime;
        float lasttime;
        PictureObject outcircle;
        PictureObject outjiku;
        PictureObject[] exMarks;
        PictureObject[] exMarksc;
        Vector2[] circlepoints;
        PictureObject trace;
        PictureObject hold;
        Vector2 holdoffset;
        bool played;
        float slideScale = 1;
        public ExMark(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, PPDEditorSkin skin, ButtonType type, float x, float y, float starttime, float endtime, float angle, Vector2[] circlepoints, uint id)
            : base(device, resourceManager, skin, type, x, y, starttime, angle, id)
        {
            this.endtime = endtime;
            this.circlepoints = circlepoints;
            skin.GetLongNoteCircleInfo(out PathObject filename, out float a, out float b);
            outcircle = new PictureObject(device, resourceManager, filename, true)
            {
                Position = new Vector2(x, y)
            };
            outjiku = new PictureObject(device, resourceManager, skin.GetCircleAxisImagePath(), true)
            {
                Position = new Vector2(x, y)
            };
            trace = new PictureObject(device, resourceManager, skin.GetTraceImagePath(type), false)
            {
                Position = new Vector2(x, y)
            };
            skin.GetHoldInfo(out filename, out a, out b);
            hold = new PictureObject(device, resourceManager, filename, true)
            {
                Position = new Vector2(x + a, y + b)
            };
            holdoffset = new Vector2(a, b);
        }
        public int ExUpdate(float ctime, float bp, DisplayState dstate, NoteType noteType, float slideScale, bool releasesound, int sameTimingMarks)
        {
            NoteType = noteType;
            SlideScale = slideScale;
            UpdateSameTimingMarks(sameTimingMarks);
            int ret = 0;
            if (noteType == NoteType.AC || (noteType == NoteType.ACFT && !IsScratch))
            {
                if (!Hidden && ctime - this.time >= -autowidth)
                {
                    ret = 1;
                }
                if (ctime - this.time >= -autowidth)
                {
                    Hidden = true;
                    return ret;
                }
            }
            else
            {
                if (!Hidden && ctime - this.Time >= -autowidth && lasttime < this.Time && !played)
                {
                    ret = 1;
                    played = true;
                }
            }
            lasttime = ctime;
            if (ctime - this.EndTime >= -autowidth)
            {
                Hidden = true;
                if (played && (releasesound || noteType == NoteType.ACFT) && ctime - this.EndTime >= -autowidth)
                {
                    ret = 1;
                }
                played = false;
                return ret;
            }
            float scale = (float)bp / defaultbpm;
            if (this.Time - ctime <= 2 / scale)
            {
                if (dstate == DisplayState.Normal || dstate == DisplayState.Hidden)
                {
                    Hidden = false;
                }
            }
            else
            {
                Hidden = true;
            }
            if (this.Time - ctime <= 0.5f / scale)
            {
                Hidden &= dstate != DisplayState.Sudden;
            }
            if (this.Time - ctime <= 0.25f / scale)
            {
                if (dstate == DisplayState.Hidden)
                {
                    Hidden = true;
                }
            }
            Hidden &= this.Time - ctime > 0;
            if (this.EndTime - ctime <= 0.25f / scale)
            {
                if (dstate == DisplayState.Hidden)
                {
                    Hidden = true;
                }
            }
            if (ctime <= this.Time)
            {
                markc.Position = CalcurateMarkCPosition(Position, markc, this.time, ctime, scale, m);
            }
            else
            {
                markc.Position = new Vector2(x - markc.Width / 2, y - markc.Height / 2);
            }
            if (exMarksc != null)
            {
                for (int i = 0; i < exMarksc.Length; i++)
                {
                    var markTime = this.time + ExFrameSec / slideScale * (i + 1);
                    exMarksc[i].Position = CalcurateMarkCPosition(exMarks[i].Position,
                        exMarksc[i], markTime, ctime, scale);
                    exMarksc[i].Hidden = exMarks[i].Hidden = ctime >= markTime;
                }
                mark.Hidden = markc.Hidden = ctime >= time;
            }


            jiku.Rotation = -1 * (float)Math.PI * (this.Time - ctime) * scale;
            if (jiku.Rotation >= 0)
            {
                jiku.Rotation = 0;
            }
            outjiku.Rotation = -2 * (float)Math.PI * (this.Time - ctime) / Length;

            return ret;
        }

        private void CreateExMark()
        {
            if (!IsScratch)
            {
                exMarks = exMarksc = null;
                return;
            }

            exMarks = new PictureObject[ExCount];
            exMarksc = new PictureObject[ExCount];
            for (int i = 0; i < exMarks.Length; i++)
            {
                exMarks[i] = new PictureObject(device, resourceManager, skin.GetMarkImagePath(GetButtonType() + 2), true)
                {
                    Position = new Vector2(x, y)
                };
                exMarksc[i] = new PictureObject(device, resourceManager, skin.GetMarkColorImagePath(GetButtonType() + 2))
                {
                    Position = new Vector2(x, y)
                };
            }
        }

        public override void GraphicUpdate()
        {
            mark.Update();
            hold.Update();
            jiku.Update();
            markc.Update();
            outcircle.Update();
            outjiku.Update();
            if (exMarks != null)
            {
                foreach (var exMark in exMarks)
                {
                    exMark.Update();
                }
            }
            if (exMarksc != null)
            {
                foreach (var exMarkc in exMarksc)
                {
                    exMarkc.Update();
                }
            }
        }

        public override void Draw()
        {
            if (Hidden) return;
            mark.Draw();
            if (IsScratch)
            {
                if (exMarks != null)
                {
                    foreach (var exMark in exMarks)
                    {
                        exMark.Draw();
                    }
                }
                if (exMarksc != null)
                {
                    foreach (var exMarkc in exMarksc)
                    {
                        exMarkc.Draw();
                    }
                }
            }
            else if (noteType == NoteType.AC || noteType == NoteType.ACFT)
            {
                hold.Draw();
            }
            if (lasttime < this.Time)
            {
                jiku.Draw();
            }
            markc.Draw();
        }

        public void gageDraw()
        {
            if (Hidden || noteType == NoteType.AC || noteType == NoteType.ACFT) return;
            if (lasttime >= this.time)
            {
                DrawGage();
            }
            outcircle.Draw();
            outjiku.Draw();
        }

        public override void DrawOnlyMark()
        {
            mark.Hidden = false;
            mark.Draw();
            if (IsScratch)
            {
                if (exMarks != null)
                {
                    foreach (var exMark in exMarks)
                    {
                        exMark.Hidden = false;
                        exMark.Draw();
                    }
                }
            }
            else if (noteType == NoteType.AC || noteType == NoteType.ACFT)
            {
                hold.Draw();
            }
            else if (noteType == NoteType.Normal)
            {
                outcircle.Draw();
            }
        }

        private void DrawGage()
        {
            var alpha = 0.7f;
            int num = circlepoints.Length / 2;
            float ratio = (lasttime - this.Time) / Length;
            if (ratio >= 1) ratio = 1;
            var displaynum = (int)(num * ratio);
            if (displaynum == 0) return;
            ColoredTexturedVertex[] fwv = new ColoredTexturedVertex[displaynum * 2];
            for (int i = 0; i < displaynum; i++)
            {
                fwv[i * 2].Position = new Vector3(x + circlepoints[i * 2].X, y + circlepoints[i * 2].Y, 0.5f);
                fwv[i * 2].TextureCoordinates = new Vector2(1.0f, 0.5f);
                fwv[i * 2].Color = new Color4(alpha, 1, 1, 1).ToBgra();
                fwv[i * 2 + 1].Position = new Vector3(x + circlepoints[i * 2 + 1].X, y + circlepoints[i * 2 + 1].Y, 0.5f);
                fwv[i * 2 + 1].TextureCoordinates = new Vector2(1.0f, 0.5f);
                fwv[i * 2 + 1].Color = new Color4(alpha, 1, 1, 1).ToBgra();
            }
            var t = trace.ImageResource.Texture;
        }

        protected override void UpdateMarkImage()
        {
            base.UpdateMarkImage();
            trace = new PictureObject(device, resourceManager, skin.GetTraceImagePath(GetButtonType()), false)
            {
                Position = new Vector2(x, y)
            };
            CreateExMark();
            UpdateMarkPosition();
        }
        protected override void UpdateMarkPosition()
        {
            base.UpdateMarkPosition();
            outcircle.Position = new Vector2(this.x, this.y);
            outjiku.Position = new Vector2(this.x, this.y);
            hold.Position = new Vector2(this.x + holdoffset.X, this.y + holdoffset.Y);
            if (exMarks != null)
            {
                int diff = IsRight ? ExMarkPosDiff : -ExMarkPosDiff;
                int currentDiff = IsRight ? ExMarkPosDiffStart : -ExMarkPosDiffStart;
                foreach (var exMark in exMarks)
                {
                    exMark.Position = new Vector2(this.x + currentDiff, this.y);
                    currentDiff += diff;
                }
            }
        }

        protected override void UpdateMarkTime()
        {
            base.UpdateMarkTime();
            CreateExMark();
            UpdateMarkPosition();
        }

        public override MarkData Convert()
        {
            var ret = new ExMarkData(x, y, angle, time, endtime, type, ID);
            foreach (var parameter in parameters)
            {
                ret.AddParameter(parameter.Key, parameter.Value);
            }
            return ret;
        }

        public float EndTime
        {
            get
            {
                return endtime;
            }
            set
            {
                if (endtime != value)
                {
                    endtime = value;
                    CreateExMark();
                    UpdateMarkPosition();
                }
            }
        }

        public float SlideScale
        {
            get
            {
                return slideScale;
            }
            set
            {
                if (slideScale != value)
                {
                    slideScale = value;
                    CreateExMark();
                    UpdateMarkPosition();
                }
            }
        }

        public float Length
        {
            get
            {
                return endtime - time;
            }
        }

        public int ExCount
        {
            get
            {
                return Math.Max(0, (int)(ExFrameInvert * Length * slideScale));
            }
        }
    }
}
