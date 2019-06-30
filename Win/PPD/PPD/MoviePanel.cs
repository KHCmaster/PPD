using PPDFramework;
using PPDFramework.Resource;
using PPDFramework.Shaders;
using PPDFramework.Texture;
using PPDFrameworkCore;
using PPDShareComponent;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace PPD
{
    class MoviePanel : HomePanelBase
    {
        public enum MovieLoopType
        {
            One,
            Sequential,
            Random
        }

        PPDFramework.Resource.ResourceManager resourceManager;
        ISound sound;
        MyGame myGame;

        bool initialized;

        SongInformation currentParent;

        SpriteObject movieSprite;
        SpriteObject thumbSprite;
        SpriteObject controllerSprite;

        MovieController movieController;

        SongInformation currentPlaying;

        LineRectangleComponent selection;

        List<ThumbList> removeList = new List<ThumbList>();

        int updateCount;
        bool playing;

        public MoviePanel(PPDDevice device, MyGame myGame, PPDFramework.Resource.ResourceManager resourceManager, ISound sound) : base(device)
        {
            this.resourceManager = resourceManager;
            this.myGame = myGame;
            this.sound = sound;
        }

        public override void Load()
        {
            OnLoadProgressed(0);
            var temp = new SpriteObject(device);
            OnLoadProgressed(10);
            this.AddChild((controllerSprite = new SpriteObject(device)));
            movieController = new MovieController(device, resourceManager, null);
            controllerSprite.AddChild(movieController);
            OnLoadProgressed(20);
            this.AddChild(temp);
            OnLoadProgressed(30);
            temp.AddChild((selection = new LineRectangleComponent(device, resourceManager, PPDColors.Selection) { RectangleWidth = 180, RectangleHeight = 100, Position = new SharpDX.Vector2(10, 190) }));
            OnLoadProgressed(50);
            temp.AddChild((thumbSprite = new SpriteObject(device)));
            OnLoadProgressed(60);
            temp.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("leftmenu.png")) { Alpha = 0.5f, Scale = new SharpDX.Vector2(1.2f, 1) });
            OnLoadProgressed(80);
            this.AddChild((movieSprite = new SpriteObject(device)));
            OnLoadProgressed(90);

            GotFocused += MoviePanel_GotFocused;
            Inputed += MoviePanel_Inputed;
            OnLoadProgressed(100);
        }

        private ThumbList CurrentThumbList
        {
            get
            {
                return thumbSprite[currentParent.Depth] as ThumbList;
            }
        }

        void MoviePanel_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                CurrentThumbList.CurrentIndex--;
                if (CurrentThumbList.CurrentIndex < 0)
                {
                    CurrentThumbList.CurrentIndex = 0;
                }
                else
                {
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
                updateCount = 0;
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                CurrentThumbList.CurrentIndex++;
                if (CurrentThumbList.CurrentIndex >= currentParent.ChildrenCount)
                {
                    CurrentThumbList.CurrentIndex = currentParent.ChildrenCount - 1;
                }
                else
                {
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
                updateCount = 0;
            }
            else if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                if (currentParent.Children.Length == 0)
                {
                    return;
                }
                if (!PlayMovie())
                {
                    SongInformation current = currentParent.Children[CurrentThumbList.CurrentIndex];
                    sound.Play(PPDSetting.DefaultSounds[1], -1000);
                    AddThumbs(current);
                }
                updateCount = 0;
            }
            else if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                if (currentParent.Parent != null)
                {
                    currentParent = currentParent.Parent;
                    removeList.Add(thumbSprite[thumbSprite.ChildrenCount - 1] as ThumbList);
                    sound.Play(PPDSetting.DefaultSounds[2], -1000);
                }
                updateCount = 0;
            }
            else if (args.InputInfo.IsPressed(ButtonType.Start))
            {
                if (playing)
                {
                    FocusManager.Focus(movieController);
                }
            }
        }

        void moviePlayer_PlayNext(object sender, EventArgs e)
        {
            StopMovie();
            switch (PPDGeneralSetting.Setting.MovieLoopType)
            {
                case MovieLoopType.Sequential:
                    int iter = 0;
                    while (iter < currentParent.ChildrenCount)
                    {
                        CurrentThumbList.CurrentIndex++;
                        if (CurrentThumbList.CurrentIndex >= currentParent.ChildrenCount)
                        {
                            CurrentThumbList.CurrentIndex = 0;
                        }

                        if (PlayMovie())
                        {
                            break;
                        }

                        iter++;
                    }
                    break;
                case MovieLoopType.Random:
                    var songs = new List<int>();
                    iter = 0;
                    foreach (var s in currentParent.Children)
                    {
                        if (s.IsPPDSong)
                        {
                            songs.Add(iter);
                        }
                        iter++;
                    }
                    if (songs.Count > 0)
                    {
                        var index = new Random().Next(songs.Count);
                        CurrentThumbList.CurrentIndex = index;
                        PlayMovie();
                    }
                    break;
            }
        }

        private bool PlayMovie()
        {
            SongInformation current = currentParent.Children[CurrentThumbList.CurrentIndex];
            if (current.IsPPDSong)
            {
                if (currentPlaying != current)
                {
                    StopMovie();
                    var movie = myGame.GetMovie(current);
                    var moviePlayer = new MoviePlayer(device, movie, current, CurrentThumbList[CurrentThumbList.CurrentIndex] as LoadablePictureComponent);
                    moviePlayer.PlayNext += moviePlayer_PlayNext;
                    movieSprite.AddChild(moviePlayer);
                    playing = true;
                    currentPlaying = current;

                    movieController.MoviePlayer = moviePlayer;
                    if (FocusManager.CurrentFocusObject != movieController)
                    {
                        FocusManager.Focus(movieController);
                    }
                }

                return true;
            }

            return false;
        }

        void MoviePanel_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            if (!initialized)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            SongInformation.Updated += SongInformation_Updated;
            currentParent = SongInformation.Root;
            AddThumbs(currentParent);
            initialized = true;
        }

        void SongInformation_Updated(object sender, EventArgs e)
        {
            thumbSprite.ClearChildren();
            currentParent = SongInformation.Root;
            AddThumbs(currentParent);
        }

        private void AddThumbs(SongInformation newParent)
        {
            for (int i = removeList.Count - 1; i >= 0; i--)
            {
                thumbSprite.RemoveChild(removeList[i]);
                removeList.RemoveAt(i);
            }
            int iter = 0;
            var temp = new ThumbList(device)
            {
                Position = new SharpDX.Vector2(200 * newParent.Depth, 0)
            };
            foreach (SongInformation songInfo in newParent.Children)
            {
                var component = new LoadablePictureComponent(device, resourceManager, songInfo)
                {
                    Position = new SharpDX.Vector2(100, 225 + 100 * iter)
                };
                temp.AddChild(component);
                iter++;
            }
            thumbSprite.AddChild(temp);
            currentParent = newParent;
        }

        protected override void UpdateImpl()
        {
            if (initialized)
            {
                thumbSprite.Position = new SharpDX.Vector2(AnimationUtility.GetAnimationValue(thumbSprite.Position.X, -(currentParent.Depth) * 200), 0);
                foreach (GameComponent gc in thumbSprite.Children)
                {
                    var thumbList = gc as ThumbList;
                    thumbList.Position = new SharpDX.Vector2(thumbList.Position.X, AnimationUtility.GetAnimationValue(thumbList.Position.Y, -100 * thumbList.CurrentIndex, 0.2f));
                }

                for (int i = removeList.Count - 1; i >= 0; i--)
                {
                    removeList[i].Alpha = AnimationUtility.DecreaseAlpha(removeList[i].Alpha);
                }

                if (updateCount >= 200 && playing)
                {
                    this[1].Alpha = AnimationUtility.DecreaseAlpha(this[1].Alpha);
                }
                else
                {
                    this[1].Alpha = AnimationUtility.IncreaseAlpha(this[1].Alpha);
                }

                updateCount++;
            }
        }

        public void StopMovie()
        {
            if (playing)
            {
                if (movieSprite.ChildrenCount > 0)
                {
                    movieSprite[0].Dispose();
                    movieSprite.ClearChildren();
                    playing = false;
                }
            }
        }

        class MovieController : FocusableGameComponent
        {
            PPDFramework.Resource.ResourceManager resourceManager;
            MoviePlayer moviePlayer;

            TextureString moviePos;
            FadableButton play;
            FadableButton pause;
            FadableButton stop;
            FadableButton seekForward;
            FadableButton seekBackward;
            FadableButton changeTrimming;
            FadableButton getThumb;
            FadableButton oneLoop;
            FadableButton secLoop;
            FadableButton randomLoop;
            Dictionary<MovieLoopType, FadableButton> loopDict;

            FadableButton[] list;
            int currentIndex;

            public MoviePlayer MoviePlayer
            {
                get { return moviePlayer; }
                set
                {
                    moviePlayer = value;
                }
            }

            public MovieController(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, MoviePlayer moviePlayer) : base(device)
            {
                this.resourceManager = resourceManager;
                this.moviePlayer = moviePlayer;

                this.AddChild((moviePos = new TextureString(device, "", 14, PPDColors.White)
                {
                    Position = new Vector2(0, -9)
                }));
                this.AddChild((changeTrimming = new FadableButton(device, resourceManager,
                    Utility.Path.Combine("moviecontroller", "changetrimming.png"),
                    Utility.Path.Combine("moviecontroller", "changetrimming_select.png"), Utility.Language["ChangeTrimming"])
                { Position = new SharpDX.Vector2(-60, 0) }));
                this.AddChild((seekBackward = new FadableButton(device, resourceManager,
                    Utility.Path.Combine("moviecontroller", "seekbackward.png"),
                    Utility.Path.Combine("moviecontroller", "seekbackward_select.png"), Utility.Language["SeekBackward"])
                { Position = new SharpDX.Vector2(-30, 0) }));
                this.AddChild((play = new FadableButton(device, resourceManager,
                    Utility.Path.Combine("moviecontroller", "play.png"),
                    Utility.Path.Combine("moviecontroller", "play_select.png"), Utility.Language["Play"])));
                this.AddChild((pause = new FadableButton(device, resourceManager,
                    Utility.Path.Combine("moviecontroller", "pause.png"),
                    Utility.Path.Combine("moviecontroller", "pause_select.png"), Utility.Language["Pause"])));
                this.AddChild((stop = new FadableButton(device, resourceManager,
                    Utility.Path.Combine("moviecontroller", "stop.png"),
                    Utility.Path.Combine("moviecontroller", "stop_select.png"), Utility.Language["Stop"])
                { Position = new SharpDX.Vector2(30, 0) }));
                this.AddChild((seekForward = new FadableButton(device, resourceManager,
                    Utility.Path.Combine("moviecontroller", "seekforward.png"),
                    Utility.Path.Combine("moviecontroller", "seekforward_select.png"), Utility.Language["SeekForward"])
                { Position = new SharpDX.Vector2(60, 0) }));
                this.AddChild((getThumb = new FadableButton(device, resourceManager,
                    Utility.Path.Combine("moviecontroller", "getthumb.png"),
                    Utility.Path.Combine("moviecontroller", "getthumb_select.png"), Utility.Language["GetThumb"])
                { Position = new SharpDX.Vector2(90, 0) }));
                this.AddChild((secLoop = new FadableButton(device, resourceManager,
                    Utility.Path.Combine("moviecontroller", "secloop.png"),
                    Utility.Path.Combine("moviecontroller", "secloop_select.png"), Utility.Language["FolderLoop"])
                { Position = new SharpDX.Vector2(120, 0) }));
                this.AddChild((oneLoop = new FadableButton(device, resourceManager,
                    Utility.Path.Combine("moviecontroller", "oneloop.png"),
                    Utility.Path.Combine("moviecontroller", "oneloop_select.png"), Utility.Language["OneLoop"])
                { Position = new SharpDX.Vector2(120, 0) }));
                this.AddChild((randomLoop = new FadableButton(device, resourceManager,
                    Utility.Path.Combine("moviecontroller", "randomloop.png"),
                    Utility.Path.Combine("moviecontroller", "randomloop_select.png"), Utility.Language["RandomLoop"])
                { Position = new SharpDX.Vector2(120, 0) }));
                this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("moviecontroller", "back.png"), true));
                play.Alpha = 0;
                loopDict = new Dictionary<MovieLoopType, FadableButton>
                {
                    {MovieLoopType.One, oneLoop},                    {MovieLoopType.Sequential, secLoop},                    {MovieLoopType.Random, randomLoop}                };
                foreach (var p in loopDict)
                {
                    p.Value.Alpha = p.Key == PPDGeneralSetting.Setting.MovieLoopType ? 1 : 0;
                }

                list = new FadableButton[]
                {
                    changeTrimming,
                    seekBackward,
                    pause,
                    stop,
                    seekForward,
                    getThumb,
                    oneLoop
                };

                this.Position = new SharpDX.Vector2(400, 420);

                Inputed += MovieController_Inputed;
                GotFocused += MovieController_GotFocused;
            }

            void MovieController_GotFocused(IFocusable sender, FocusEventArgs args)
            {
                currentIndex = Array.IndexOf(list, pause);
                foreach (FadableButton button in list)
                {
                    button.Selected = false;
                }
                list[currentIndex].Selected = true;
            }

            void MovieController_Inputed(IFocusable sender, InputEventArgs args)
            {
                if (args.InputInfo.IsPressed(ButtonType.Left))
                {
                    list[currentIndex].Selected = false;
                    currentIndex--;
                    if (currentIndex < 0)
                    {
                        currentIndex = list.Length - 1;
                    }
                    list[currentIndex].Selected = true;
                }
                else if (args.InputInfo.IsPressed(ButtonType.Right))
                {
                    list[currentIndex].Selected = false;
                    currentIndex++;
                    if (currentIndex >= list.Length)
                    {
                        currentIndex = 0;
                    }
                    list[currentIndex].Selected = true;
                }
                else if (args.InputInfo.IsPressed(ButtonType.Circle))
                {
                    switch (currentIndex)
                    {
                        case 0:
                            moviePlayer.ToggleAspectRatio();
                            break;
                        case 1:
                            moviePlayer.SeekBackward();
                            break;
                        case 2:
                            moviePlayer.TogglePlayOrPause();
                            break;
                        case 3:
                            moviePlayer.Stop();
                            break;
                        case 4:
                            moviePlayer.SeekForward();
                            break;
                        case 5:
                            moviePlayer.CreateThumb();
                            break;
                        case 6:
                            if (PPDGeneralSetting.Setting.MovieLoopType >= MovieLoopType.Random)
                            {
                                PPDGeneralSetting.Setting.MovieLoopType = MovieLoopType.One;
                            }
                            else
                            {
                                PPDGeneralSetting.Setting.MovieLoopType++;
                            }
                            break;
                    }
                }
                else if (args.InputInfo.IsPressed(ButtonType.Cross) || args.InputInfo.IsPressed(ButtonType.Start))
                {
                    FocusManager.RemoveFocus();
                }
            }

            protected override void UpdateImpl()
            {
                if (moviePlayer != null)
                {
                    if (moviePlayer.Movie != null && moviePlayer.Movie.Initialized)
                    {
                        moviePos.Text = String.Format("{0}/{1}", GetTime(moviePlayer.Movie.MoviePosition), GetTime(moviePlayer.Movie.Length));
                        moviePos.Position = new SharpDX.Vector2(-moviePos.Width - 75, moviePos.Position.Y);
                    }

                    play.Selected = pause.Selected;
                    play.Alpha = moviePlayer.Movie.Playing ? AnimationUtility.DecreaseAlpha(play.Alpha) : AnimationUtility.IncreaseAlpha(play.Alpha);
                    pause.Alpha = 1 - play.Alpha;

                    randomLoop.Selected = secLoop.Selected = oneLoop.Selected;
                    foreach (var p in loopDict)
                    {
                        p.Value.Alpha = p.Key == PPDGeneralSetting.Setting.MovieLoopType ? AnimationUtility.IncreaseAlpha(p.Value.Alpha) : AnimationUtility.DecreaseAlpha(p.Value.Alpha);
                    }
                }

                this.Alpha = Focused ? AnimationUtility.IncreaseAlpha(this.Alpha) : AnimationUtility.DecreaseAlpha(this.Alpha);
            }

            private string GetTime(double time)
            {
                var minute = (int)(time / 60);
                var second = (int)(time - minute * 60);
                return String.Format("{0:D2}:{1:D2}", minute, second);
            }
        }

        class MoviePlayer : GameComponent
        {
            public event EventHandler PlayNext;

            private IMovie movie;
            private SongInformation songInfo;
            private LoadablePictureComponent picture;
            private bool aspectApplied = true;

            public IMovie Movie
            {
                get;
                private set;
            }

            public SongInformation SongInfo
            {
                get
                {
                    return songInfo;
                }
            }

            public MoviePlayer(PPDDevice device, IMovie movie, SongInformation songInfo, LoadablePictureComponent picture) : base(device)
            {
                this.movie = movie;
                this.songInfo = songInfo;
                this.picture = picture;
                movie.TrimmingData = songInfo.TrimmingData;
                movie.Initialize();
                movie.Volume = songInfo.MovieVolume;
                movie.Seek(songInfo.StartTime);
                movie.Finished += movie_Finished;
                movie.Play();

                Movie = movie;
                this.AddChild(movie as GameComponent);
            }

            void movie_Finished(object sender, EventArgs e)
            {
                switch (PPDGeneralSetting.Setting.MovieLoopType)
                {
                    case MovieLoopType.One:
                        movie.Seek(songInfo.StartTime);
                        movie.Play();
                        break;
                    case MovieLoopType.Sequential:
                    case MovieLoopType.Random:
                        if (PlayNext != null)
                        {
                            PlayNext.Invoke(this, EventArgs.Empty);
                        }
                        break;
                }
            }

            public void SeekBackward()
            {
                double result = movie.MoviePosition - 5;
                movie.Seek(result < 0 ? 0 : result);
            }

            public void SeekForward()
            {
                double result = movie.MoviePosition + 5;
                movie.Seek(result > movie.Length ? movie.Length : result);
            }

            public void TogglePlayOrPause()
            {
                if (movie.Playing)
                {
                    movie.Pause();
                }
                else
                {
                    movie.Play();
                }
            }

            public void Stop()
            {
                movie.Seek(songInfo.StartTime);
                movie.Pause();
            }

            public void ToggleAspectRatio()
            {
                aspectApplied = !aspectApplied;
            }

            public void CreateThumb()
            {
                try
                {
                    TextureBase texture = movie.Texture;
                    if (texture != null)
                    {
                        var bt = new Bitmap(texture.ToStream());
                        Bitmap newbit = null;
                        if (aspectApplied)
                        {
                            if (movie.TrimmingData.Left < 0 || movie.TrimmingData.Top < 0 || movie.TrimmingData.Bottom < 0 || movie.TrimmingData.Right < 0)
                            {
                                newbit = bt.Clone(new System.Drawing.Rectangle(0, 0, bt.Width, bt.Height), bt.PixelFormat);
                                bt.Dispose();
                                bt = new Bitmap(480, 270);
                                var gra = Graphics.FromImage(bt);
                                int width = -(int)(movie.TrimmingData.GetLeftTrimming(movie.MovieWidth) + movie.TrimmingData.GetRightTrimming(movie.MovieWidth)) * bt.Width / 800,
                                    height = -(int)(movie.TrimmingData.GetTopTrimming(movie.MovieHeight) + movie.TrimmingData.GetBottomTrimming(movie.MovieHeight)) * bt.Height / 450;
                                gra.FillRectangle(Brushes.Black, new System.Drawing.Rectangle(0, 0, bt.Width, bt.Height));
                                gra.DrawImage(newbit, new System.Drawing.Rectangle(width / 2, height / 2, 480 - width, 270 - height), new System.Drawing.Rectangle(0, 0, newbit.Width, newbit.Height), GraphicsUnit.Pixel);
                                gra.Dispose();
                            }
                            else
                            {
                                newbit = bt.Clone(new System.Drawing.Rectangle((int)songInfo.TrimmingData.GetLeftTrimming(movie.MovieWidth),
                                     (int)songInfo.TrimmingData.GetTopTrimming(movie.MovieHeight),
                                     (int)(bt.Width - songInfo.TrimmingData.GetLeftTrimming(movie.MovieWidth) - songInfo.TrimmingData.GetRightTrimming(movie.MovieWidth)),
                                     (int)(bt.Height - songInfo.TrimmingData.GetTopTrimming(movie.MovieHeight) - songInfo.TrimmingData.GetTopTrimming(movie.MovieHeight))), bt.PixelFormat);
                                bt.Dispose();
                                bt = new Bitmap(newbit, 480, 270);
                            }
                        }
                        else
                        {
                            newbit = bt.Clone(new System.Drawing.Rectangle(0, 0, bt.Width, bt.Height), bt.PixelFormat);
                            bt.Dispose();
                            bt = new Bitmap(newbit, 480, 270);
                        }
                        newbit.Dispose();
                        if (movie.Rotated)
                        {
                            bt.RotateFlip(RotateFlipType.Rotate180FlipX);
                        }
                        bt.Save(Path.Combine(songInfo.DirectoryPath, "thumb.png"), System.Drawing.Imaging.ImageFormat.Png);

                        if (picture != null)
                        {
                            picture.UpdatePicture();
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Capture Error");
                }
            }

            protected override void DisposeResource()
            {
                if (movie != null)
                {
                    movie.Stop();
                    movie = null;
                }
                base.DisposeResource();
            }

            protected override void UpdateImpl()
            {
                if (movie != null)
                {
                    if (aspectApplied)
                    {
                        movie.TrimmingData = new MovieTrimmingData(
                            AnimationUtility.GetAnimationValue(movie.TrimmingData.Top, songInfo.TrimmingData.Top),
                            AnimationUtility.GetAnimationValue(movie.TrimmingData.Left, songInfo.TrimmingData.Left),
                            AnimationUtility.GetAnimationValue(movie.TrimmingData.Right, songInfo.TrimmingData.Right),
                            AnimationUtility.GetAnimationValue(movie.TrimmingData.Bottom, songInfo.TrimmingData.Bottom)
                        );
                    }
                    else
                    {
                        movie.TrimmingData = new MovieTrimmingData(
                            AnimationUtility.GetAnimationValue(movie.TrimmingData.Top, 0),
                            AnimationUtility.GetAnimationValue(movie.TrimmingData.Left, 0),
                            AnimationUtility.GetAnimationValue(movie.TrimmingData.Right, 0),
                            AnimationUtility.GetAnimationValue(movie.TrimmingData.Bottom, 0)
                        );
                    }
                }
            }
        }

        class ThumbList : GameComponent
        {
            public int CurrentIndex
            {
                get;
                set;
            }

            public ThumbList(PPDDevice device) : base(device)
            {
            }

            protected override bool OnCanUpdateChild(int childIndex)
            {
                return Math.Abs(childIndex - CurrentIndex) <= 4;
            }

            protected override bool OnCanDrawChild(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
            {
                return Math.Abs(childIndex - CurrentIndex) <= 4;
            }
        }

        class LoadablePictureComponent : GameComponent
        {
            static bool isLoading;

            PPDFramework.Resource.ResourceManager resourceManager;
            SongInformation songInfo;

            EffectObject loading;
            PictureObject picture;

            string thumbPath;

            public LoadablePictureComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, SongInformation songInfo) : base(device)
            {
                this.resourceManager = resourceManager;
                this.songInfo = songInfo;

                this.AddChild(new TextureString(device, songInfo.DirectoryName, 12, 150, 30, true, true, PPDColors.White)
                {
                    Position = new Vector2(-75, 35)
                });
                this.AddChild((loading = new EffectObject(device, resourceManager, Utility.Path.Combine("loading.etd"))));
                loading.PlayType = Effect2D.EffectManager.PlayType.Loop;
                loading.Play();
                loading.Scale = new SharpDX.Vector2(0.5f, 0.5f);
                if (songInfo.IsPPDSong)
                {

                    thumbPath = Path.Combine(songInfo.DirectoryPath, "thumb.png");
                    if (File.Exists(thumbPath))
                    {
                        if (!resourceManager.HasResource<ImageResourceBase>(thumbPath))
                        {
                            var thread = ThreadManager.Instance.GetThread(LoadTexture);
                            thread.Start();
                        }
                        else
                        {
                            LoadTexture();
                        }
                    }
                    else
                    {
                        picture = new PictureObject(device, resourceManager, Utility.Path.Combine("noimage.png"), true)
                        {
                            Scale = new SharpDX.Vector2(0.25f, 0.25f),
                            Alpha = 0
                        };
                        this.AddChild(picture);
                    }
                }
                else
                {
                    picture = new PictureObject(device, resourceManager, Utility.Path.Combine("bigfolder.png"), true)
                    {
                        Scale = new SharpDX.Vector2(0.25f, 0.25f),
                        Alpha = 0
                    };
                    this.AddChild(picture);
                }
            }

            public void UpdatePicture()
            {
                if (File.Exists(thumbPath))
                {
                    resourceManager.RemoveResource(thumbPath);
                    RemoveChild(picture);
                    picture = null;
                    var thread = ThreadManager.Instance.GetThread(LoadTexture);
                    thread.Start();
                }
            }

            private void LoadTexture()
            {
                while (true)
                {
                    lock (this)
                    {
                        if (!isLoading)
                        {
                            isLoading = true;
                            break;
                        }
                    }
                    Thread.Sleep(100);
                }
                picture = new PictureObject(device, resourceManager, PathObject.Absolute(thumbPath), true);
                picture.Scale = new SharpDX.Vector2(480 / picture.Width * 0.25f, 270 / picture.Height * 0.25f);
                picture.Alpha = 0;
                this.AddChild(picture);

                lock (this)
                {
                    isLoading = false;
                }
            }

            protected override void UpdateImpl()
            {
                if (picture != null)
                {
                    picture.Alpha = AnimationUtility.IncreaseAlpha(picture.Alpha);
                    loading.Alpha = 1 - picture.Alpha;
                }
            }
        }
    }
}
