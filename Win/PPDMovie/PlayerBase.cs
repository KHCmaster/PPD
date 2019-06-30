using DirectShow;
using PPDFramework;
using PPDFramework.Shaders;
using PPDFramework.Texture;
using PPDFramework.Vertex;
using SharpDX;
using System;
using System.Threading;

namespace PPDMovie
{
    public abstract class PlayerBase : GameComponent, IMovie
    {
        protected MovieTrimmingData trimmingdata = new MovieTrimmingData(0, 0, 0, 0);
        public event EventHandler Finished;
        public event EventHandler FadeOutFinished;

        protected const int DefaultFadeStep = 2;
        float fadeStep;

        protected object syncobject = new object();

        protected double length;
        protected bool initialized;

        protected MovieFadeState state = MovieFadeState.None;

        protected int width;
        protected int height;
        protected const int VolumeSilence = -10000;
        protected int innerVolume;
        protected string filename = string.Empty;
        protected float maxu;
        protected float maxv;
        private const int WM_GRAPHNOTIFY = 0x0400 + 13;

        private VertexInfo vertices;

        public string FileName
        {
            get { return filename; }
            set { filename = value; }
        }
        public virtual double PlayRate
        {
            get;
            set;
        }
        public virtual bool IsAudioOnly
        {
            get { return true; }
        }
        public virtual int Volume
        {
            get { return innerVolume; }
            set { innerVolume = value; }
        }

        protected PlayerBase(PPDDevice device, string filename) : base(device)
        {
            PlayRate = 1;
            this.filename = filename;
            MaximumVolume = -1000;
            innerVolume = MaximumVolume;
            MovieDisplayWidth = 800;
            MovieDisplayHeight = 450;
            MovieUtility.GraphNotify += util_GraphNotify;
        }

        public abstract int Initialize();
        public abstract void Seek(double time);
        public abstract void Play();
        public abstract void Stop();
        public abstract void Pause();

        public void FadeIn(float fadeStep)
        {
            if (!initialized)
            {
                return;
            }
            Alpha = 0;
            Volume = VolumeSilence;
            this.fadeStep = fadeStep;
            //DsError.ThrowExceptionForHR(hr);
            state = MovieFadeState.FadeIn;
        }

        public void FadeOut(float fadeStep)
        {
            if (!initialized) return;
            //this.a = 1.0f;
            //this.basicAudio.put_Volume(VolumeFull);
            this.fadeStep = fadeStep;
            state = MovieFadeState.FadeOut;
        }

        public void SetDefaultVisible()
        {
            if (!initialized)
            {
                return;
            }
            Alpha = 1;
            Volume = MaximumVolume;
            //DsError.ThrowExceptionForHR(hr);
            state = MovieFadeState.None;
        }

        protected override void UpdateImpl()
        {
            CheckState();
            CheckFinish();
            if (vertices == null)
            {
                UpdateVertices();
            }
        }

        private void UpdateVertices()
        {
            if (vertices == null)
            {
                vertices = device.GetModule<ShaderCommon>().CreateVertex(4);
            }
            float width = MovieDisplayWidth;
            float height = MovieDisplayHeight;
            float textureWidth = MovieDisplayWidth;
            float textureHeight = MovieDisplayHeight;
            var scale = new Vector2(MovieDisplayWidth / 800, MovieDisplayHeight / 450);
            var left = trimmingdata.GetLeftTrimming(MovieWidth);
            var right = trimmingdata.GetRightTrimming(MovieWidth);
            var top = trimmingdata.GetTopTrimming(MovieHeight);
            var bottom = trimmingdata.GetBottomTrimming(MovieHeight);
            var x1 = (float)(left > 0 ? left : 0) / this.width * maxu;
            var y1 = (float)(top > 0 ? top : 0) / this.height * maxv;
            var x2 = (1.0f - (float)(right > 0 ? right : 0) / this.width) * maxu;
            var y2 = (1.0f - (float)(bottom > 0 ? bottom : 0) / this.height) * maxv;
            if (Rotated)
            {
                y1 = 1 - y1;
                y2 = 1 - y2;
            }
            vertices.Write(new[] {
                new ColoredTexturedVertex(new Vector3((left < 0 ? -left : 0) * scale.X, (top < 0 ? -top : 0) * scale.Y, 0.5f), new Vector2(x1, y1)),
                new ColoredTexturedVertex(new Vector3(width + (right < 0 ? right : 0) * scale.X, (top < 0 ? -top : 0) * scale.Y, 0.5f), new Vector2(x2, y1)),
                new ColoredTexturedVertex(new Vector3((left < 0 ? -left : 0) * scale.X, height + (bottom < 0 ? bottom : 0) * scale.Y, 0.5f), new Vector2(x1, y2)),
                new ColoredTexturedVertex(new Vector3(width + (right < 0 ? right : 0) * scale.X, height + (bottom < 0 ? bottom : 0) * scale.Y, 0.5f), new Vector2(x2,y2))
            });
        }

        protected override bool OnCanUpdate()
        {
            return initialized;
        }

        protected override void DrawImpl(AlphaBlendContext alphaBlendContext)
        {
            TextureBase texture;
            if ((texture = Texture) == null)
            {
                return;
            }

            alphaBlendContext.Texture = texture;
            alphaBlendContext.Vertex = vertices;
            device.GetModule<AlphaBlend>().Draw(device, alphaBlendContext);
        }

        protected void CheckFinish()
        {
            if (this.length <= MoviePosition && Playing)
            {
                Playing = false;
                this.Finished?.Invoke(this, new EventArgs());
            }
        }

        protected void CheckState()
        {
            switch (state)
            {
                case MovieFadeState.FadeIn:
                    bool check1 = false, check2 = false;
                    Alpha += fadeStep / 100f;
                    if (Alpha >= 1.0f)
                    {
                        Alpha = 1.0f;
                        check1 = true;
                    }
                    int vo = Volume;
                    vo += (int)(fadeStep * 100);
                    if (vo <= MaximumVolume)
                    {
                        Volume = vo;
                    }
                    else
                    {
                        Volume = MaximumVolume;
                        check2 = true;
                    }
                    if (check1 && check2)
                    {
                        state = MovieFadeState.None;
                    }
                    break;
                case MovieFadeState.FadeOut:
                    check1 = false; check2 = false;
                    Alpha -= fadeStep / 100f;
                    if (Alpha <= 0.0f)
                    {
                        Alpha = 0.0f;
                        check1 = true;
                    }
                    vo = Volume;
                    vo -= (int)(fadeStep * 100);
                    if (vo >= VolumeSilence)
                    {
                        Volume = vo;
                    }
                    else
                    {
                        Volume = VolumeSilence;
                        check2 = true;
                    }
                    if (check1 && check2)
                    {
                        state = MovieFadeState.None;
                        this.FadeOutFinished?.Invoke(this, new EventArgs());
                    }
                    break;

                case MovieFadeState.None:
                    break;
            }
        }

        void util_GraphNotify(object sender, EventArgs e)
        {
            OnGraphNotify();
        }

        void OnGraphNotify()
        {
            if (MediaEventEx == null || !initialized) return;
            int hr = 0;
            do
            {
                if (MediaEventEx == null || !initialized)
                {
                    break;
                }

                hr = MediaEventEx.GetEvent(out DsEvCode code, out int p1, out int p2, 0);
                if (hr < 0)
                    break;
                hr = MediaEventEx.FreeEventParams(code, p1, p2);
                switch (code)
                {
                    case DsEvCode.Complete:
                        this.Finished?.Invoke(this, new EventArgs());
                        break;
                }
            }
            while (hr == 0);
        }

        public virtual void releaseCOM()
        {
            if (!initialized) return;
            this.initialized = false;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        protected override void DisposeResource()
        {
            if (vertices != null)
            {
                vertices.Dispose();
                vertices = null;
            }
            MovieUtility.GraphNotify -= util_GraphNotify;
            var thread = new Thread(() =>
            {
                Stop();
                Thread.Sleep(1000);
                releaseCOM();
            })
            {
                IsBackground = true
            };
            thread.Start();
        }

        public double Length
        {
            get
            {
                return this.length;
            }
        }

        public abstract double MoviePosition
        {
            get;
        }

        public abstract TextureBase Texture
        {
            get;
        }

        public abstract IMediaEventEx MediaEventEx
        {
            get;
        }

        public int MovieWidth
        {
            get
            {
                return width;
            }
        }

        public int MovieHeight
        {
            get
            {
                return height;
            }
        }

        public float MovieDisplayWidth
        {
            get;
            set;
        }

        public float MovieDisplayHeight
        {
            get;
            set;
        }

        public bool Playing
        {
            get;
            protected set;
        }

        public bool Initialized
        {
            get
            {
                return initialized;
            }
        }

        #region IMovie メンバ


        public float MaxU
        {
            get { return maxu; }
        }

        public float MaxV
        {
            get { return maxv; }
        }

        public MovieTrimmingData TrimmingData
        {
            get { return trimmingdata; }
            set
            {
                trimmingdata = value;
                UpdateVertices();
            }
        }

        public MovieFadeState FadeState
        {
            get { return state; }
        }

        public int MaximumVolume
        {
            get;
            set;
        }

        public abstract bool Rotated
        {
            get;
        }
        #endregion
    }
}
