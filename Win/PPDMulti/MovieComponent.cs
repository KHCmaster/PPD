using PPDFramework;

namespace PPDMulti
{
    class MovieComponent : GameComponent
    {
        const int fadestep = 2;
        float start;
        float end;
        bool waitingseek;
        int waitingcount;

        public MovieComponent(PPDDevice device, IMovie movie) : base(device)
        {
            Movie = movie;
            this.AddChild(movie as GameComponent);
        }

        public IMovie Movie
        {
            get;
            private set;
        }

        public void SetLoop(float start, float end)
        {
            this.start = start;
            if (end >= Movie.Length) end = (float)Movie.Length;
            this.end = end;
        }

        protected override void UpdateImpl()
        {
            if (waitingseek) waitingcount++;
            if (Movie != null)
            {
                Movie.Update();
                CheckLoop();
            }
        }

        public void CheckLoop()
        {
            double check = Movie.MoviePosition;
            if (this.end - 2 / fadestep <= check)
            {
                if (Movie.FadeState != MovieFadeState.FadeOut)
                {
                    Movie.FadeOut();
                }
                if (this.end <= check)
                {
                    Movie.Stop();
                    Movie.Seek(this.start);
                    Movie.Play();
                    Movie.Pause();
                    this.waitingseek = true;
                    //this.fadein();
                }
            }
            if (waitingseek && waitingcount > 60)
            {
                waitingseek = false;
                Movie.Play();
                Movie.FadeIn();
            }
        }

        protected override void DisposeResource()
        {
            if (Movie != null)
            {
                Movie.Stop();
                Movie = null;
            }
            base.DisposeResource();
        }
    }
}
