using PPDFramework;
using System;

namespace PPDMulti
{
    sealed class MovieManager(PPDDevice device, IGameHost gameHost) : IDisposable
    {
        MovieComponent movie;
        SongInformation currentSongInformation;
        SongInformation lastSongInformation;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "外部から渡されるもので、このクラスは所有しない")]
        readonly PPDDevice device = device;
        readonly IGameHost gameHost = gameHost;

        public MovieComponent Movie
        {
            get
            {
                return movie;
            }
        }

        public event EventHandler MovieChanged;
        public event EventHandler MovieChangeFailed;

        public void Stop()
        {
            lastSongInformation = currentSongInformation;
            currentSongInformation = null;
            if (movie == null)
            {
                return;
            }
            movie.Dispose();
            movie = null;
        }

        /// <summary>
        /// 動画を止めて破棄します（Stop と同じ）。
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        public void Change(bool reload)
        {
            Change(lastSongInformation, reload);
        }

        public void Change(SongInformation songInfo, bool reload)
        {
            if (songInfo != null && (reload || currentSongInformation != songInfo))
            {
                movie?.Dispose();
                movie = null;

                if (!songInfo.IsPPDSong)
                {
                    return;
                }

                if (PPDSetting.Setting.MenuMoviePreviewDisabled)
                {
                    OnMovieChanged();
                    return;
                }

                movie = new MovieComponent(device, gameHost.GetMovie(songInfo));
                try
                {
                    movie.Movie.Initialize();
                    movie.Movie.TrimmingData = songInfo.TrimmingData;
                    movie.Movie.MaximumVolume = songInfo.MovieVolume;
                    movie.SetLoop(songInfo.ThumbStartTime, songInfo.ThumbEndTime);
                    movie.Movie.Seek(songInfo.ThumbStartTime);
                    movie.Movie.Play();
                    movie.Movie.FadeIn();
                    OnMovieChanged();
                }
                catch
                {
                    OnMovieChangeFailed();
                }
            }
            currentSongInformation = songInfo;
        }

        public void FadeOut(float fadeStep)
        {
            movie?.Movie.FadeOut(fadeStep);
        }

        private void OnMovieChanged()
        {
            MovieChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnMovieChangeFailed()
        {
            MovieChangeFailed?.Invoke(this, EventArgs.Empty);
        }
    }
}
