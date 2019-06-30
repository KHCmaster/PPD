using PPDFramework;
using PPDFramework.Texture;
using System;

namespace PPDSingle
{
    /// <summary>
    /// メニュー動画
    /// </summary>
    class MenuMovie : GameComponent, IMovie
    {
        const int FadeStep = 2;

        public event EventHandler FadeOutFinished;

        protected virtual void OnFinished(EventArgs e)
        {
            Finished?.Invoke(this, e);
        }

        public event EventHandler Finished;
        double start = -1;
        double end = -1;
        bool waitingSeek;
        int waitingCount;
        IMovie movie;
        MovieFadeState lastState;

        public IMovie Movie
        {
            get { return movie; }
            set
            {
                if (movie != value)
                {
                    if (movie != null)
                    {
                        this.RemoveChild((GameComponent)movie);
                    }
                    movie = value;
                    if (movie != null)
                    {
                        this.AddChild((GameComponent)movie);
                    }
                }
            }
        }

        public MenuMovie(PPDDevice device) : base(device)
        {
        }

        protected override void UpdateImpl()
        {
            if (waitingSeek) waitingCount++;
            if (lastState == MovieFadeState.FadeOut && FadeState == MovieFadeState.None)
            {
                OnFadeOutFinished();
            }
            if (movie == null) return;
            lastState = FadeState;
            if (CheckLoopAvailable) CheckLoop();
        }

        public void SetLoop(double start, double end)
        {
            this.start = start;
            if (end >= movie.Length) end = movie.Length;
            this.end = end;
            Seek(this.start);
            Play();
            Pause();
            Seek(this.start);
            Play();
            Pause();
            Seek(this.start);
            CheckLoopAvailable = true;
        }

        public void CheckLoop()
        {
            double check = movie.MoviePosition;
            if (this.end - 2 / FadeStep <= check)
            {
                if (this.FadeState != MovieFadeState.FadeOut)
                {
                    this.FadeOut();
                }
                if (this.end <= check)
                {
                    this.Stop();
                    movie.Seek(this.start);
                    this.Play();
                    this.Pause();
                    this.waitingSeek = true;
                    OnFadeOutFinished();
                    //this.fadein();
                }
            }
            if (waitingSeek && waitingCount > 60)
            {
                waitingSeek = false;
                this.Play();
                this.FadeIn();
            }
        }

        protected void OnFadeOutFinished()
        {
            FadeOutFinished?.Invoke(this, EventArgs.Empty);
        }

        protected override void DisposeResource()
        {
            if (movie != null)
            {
                movie.Dispose();
                movie = null;
            }
        }

        #region IMovie メンバ

        public void FadeIn(float fadeStep = 2)
        {
            if (movie == null) return;
            movie.FadeIn(fadeStep);
        }

        public void FadeOut(float fadeStep = 2)
        {
            if (movie == null) return;
            movie.FadeOut(fadeStep);
        }

        public string FileName
        {
            get { return movie.FileName; }
            set { movie.FileName = value; }
        }

        public int MovieHeight
        {
            get { return movie.MovieHeight; }
        }

        public int Initialize()
        {
            if (movie == null) return 1;
            return movie.Initialize();
        }

        public bool Initialized
        {
            get
            {
                if (movie == null) return false;
                return movie.Initialized;
            }
        }

        public double Length
        {
            get { return movie.Length; }
        }

        public double MoviePosition
        {
            get { return movie.MoviePosition; }
        }

        public void Pause()
        {
            if (movie == null) return;
            movie.Pause();
        }

        public void Play()
        {
            if (movie == null) return;
            movie.Play();
        }

        public bool Playing
        {
            get { return movie.Playing; }
        }

        public void Seek(double time)
        {
            if (movie == null) return;
            movie.Seek(time);
        }

        public void SetDefaultVisible()
        {
            if (movie == null) return;
            movie.SetDefaultVisible();
        }

        public int Volume
        {
            get { return movie != null ? movie.Volume : 0; }
            set
            {
                if (movie != null)
                {
                    movie.Volume = value;
                }
            }
        }

        public void Stop()
        {
            if (movie == null) return;
            movie.Stop();
        }

        public TextureBase Texture
        {
            get { return movie.Texture; }
        }

        public int MovieWidth
        {
            get { return movie.MovieWidth; }
        }
        public float MaxU
        {
            get { return movie.MaxU; }
        }

        public float MaxV
        {
            get { return movie.MaxV; }
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

        public MovieTrimmingData TrimmingData
        {
            get
            {
                return movie.TrimmingData;
            }
            set
            {
                movie.TrimmingData = value;
            }
        }
        public MovieFadeState FadeState
        {
            get
            {
                if (movie == null) return MovieFadeState.None;
                return movie.FadeState;
            }
        }
        public bool CheckLoopAvailable
        {
            get;
            set;
        }
        public int MaximumVolume
        {
            get
            {
                if (movie == null) return 0;
                else return movie.MaximumVolume;
            }
            set
            {
                if (movie != null) movie.MaximumVolume = value;
            }
        }

        public bool Rotated
        {
            get
            {
                if (movie == null) return false;
                else return movie.Rotated;
            }
        }

        public double PlayRate
        {
            get
            {
                if (movie != null)
                {
                    return movie.PlayRate;
                }
                return 1;
            }
            set
            {
                if (movie != null)
                {
                    movie.PlayRate = value;
                }
            }
        }

        public bool IsAudioOnly
        {
            get
            {
                if (movie != null)
                {
                    return movie.IsAudioOnly;
                }
                return false;
            }
        }

        #endregion
    }
}
