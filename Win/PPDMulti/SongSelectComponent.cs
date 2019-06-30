using PPDFramework;
using PPDFrameworkCore;
using PPDShareComponent;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PPDMulti
{
    class SongSelectComponent : FocusableGameComponent
    {
        enum Mode
        {
            SongInformation,
            LogicInformation,
            CommonSongInfo,
        }

        public event EventHandler SongSelected;

        const int MaxDisplayCount = 17;
        const int MaxScrollBarHeight = 330;
        const int ScrollBarY = 80;


        PPDFramework.Resource.ResourceManager resourceManager;
        ISound sound;

        PictureObject back;
        SpriteObject songInfoSprite;
        RectangleComponent scrollBar;
        TextureString modeString;

        SongInformation currentSong;
        LogicFolderInfomation currentLogic;

        int index;
        int scrollIndex;

        struct SelectStructure
        {
            public int index;
            public int scrollIndex;
        }

        Stack<SelectStructure> songStack;
        Stack<SelectStructure> logicStack;
        Stack<SelectStructure> commonSongStack;

        Mode mode = Mode.SongInformation;

        private LeftMenu leftMenu;
        private MovieManager movieManager;

        private MovieComponent movie;
        private bool isWaitingMovieChange;
        private int waitingMovieChangeCount;

        SongDetailComponent songDetail;

        private SongInformationComponent CurrentComponent
        {
            get
            {
                return songInfoSprite.ChildrenCount == 0 ? null : songInfoSprite[index] as SongInformationComponent;
            }
        }

        public SongInformation SongInformation
        {
            get;
            private set;
        }

        public Difficulty Difficulty
        {
            get;
            private set;
        }

        private Stack<SelectStructure> CurrentStack
        {
            get
            {
                switch (mode)
                {
                    case Mode.CommonSongInfo:
                        return commonSongStack;
                    case Mode.LogicInformation:
                        return logicStack;
                    case Mode.SongInformation:
                        return songStack;
                }
                return null;
            }
        }

        public SongSelectComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound, LeftMenu leftMenu, MovieManager movieManager) : base(device)
        {
            this.resourceManager = resourceManager;
            this.sound = sound;
            this.leftMenu = leftMenu;
            this.movieManager = movieManager;

            songStack = new Stack<SelectStructure>();
            logicStack = new Stack<SelectStructure>();
            commonSongStack = new Stack<SelectStructure>();

            this.AddChild(back = new PictureObject(device, resourceManager, Utility.Path.Combine("dialog_back.png")));
            this.AddChild(new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0.75f
            });

            back.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("bottom_triangle.png"))
            {
                Position = new Vector2(650, 55)
            });
            back.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("lorr.png"))
            {
                Position = new Vector2(500, 56)
            });
            back.AddChild(modeString = new TextureString(device, Utility.Language["MusicScore"], 16, PPDColors.White)
            {
                Position = new Vector2(540, 53)
            });
            back.AddChild(new TextureString(device, Utility.Language["Random"], 16, PPDColors.White)
            {
                Position = new Vector2(675, 53)
            });
            back.AddChild(new TextureString(device, Utility.Language["SelectScore"], 30, PPDColors.White)
            {
                Position = new Vector2(35, 30)
            });
            back.AddChild(songInfoSprite = new SpriteObject(device));
            back.AddChild(new LineRectangleComponent(device, resourceManager, PPDColors.White) { RectangleWidth = 1, RectangleHeight = MaxScrollBarHeight, Position = new Vector2(510, ScrollBarY) });
            back.AddChild(scrollBar = new RectangleComponent(device, resourceManager, PPDColors.White)
            {
                Position = new SharpDX.Vector2(500, ScrollBarY),
                RectangleWidth = 5,
                RectangleHeight = MaxScrollBarHeight
            });
            back.AddChild(songDetail = new SongDetailComponent(device, resourceManager) { Position = new Vector2(520, 220) });

            InitializeSelection();
            GenerateSongInformation();

            GotFocused += SongSelectComponent_GotFocused;

            Inputed += SongSelectComponent_Inputed;
            SongInformation.Updated += SongInformation_Updated;
            movieManager.MovieChanged += movieManager_MovieChanged;
        }

        private void InitializeSelection()
        {
            songStack.Clear();
            logicStack.Clear();
            commonSongStack.Clear();
            currentSong = SongInformation.Root;
            currentLogic = LogicFolderInfomation.Root;
            logicStack.Push(new SelectStructure());
            commonSongStack.Push(new SelectStructure());
            index = 0;
            scrollIndex = 0;
        }

        void SongInformation_Updated(object sender, EventArgs e)
        {
            InitializeSelection();
            GenerateSongInformation();
        }

        private void GenerateSongInformation()
        {
            songInfoSprite.ClearChildren();

            int iter = 0;
            switch (mode)
            {
                case Mode.SongInformation:
                    foreach (SongInformation songInfo in currentSong.Children)
                    {
                        songInfoSprite.AddChild(new SongInformationComponent(device, resourceManager, songInfo, null)
                        {
                            Position = new SharpDX.Vector2(45, 20 * iter + 77),
                            Hidden = iter >= MaxDisplayCount
                        });
                        iter++;
                    }
                    break;
                case Mode.LogicInformation:
                    foreach (LogicFolderInfomation logicInfo in currentLogic.Children)
                    {
                        songInfoSprite.AddChild(new SongInformationComponent(device, resourceManager, null, logicInfo)
                        {
                            Position = new SharpDX.Vector2(45, 20 * iter + 77),
                            Hidden = iter >= MaxDisplayCount
                        });
                        iter++;
                    }
                    break;
                case Mode.CommonSongInfo:
                    if (leftMenu.CommonSongs != null)
                    {
                        var easy = leftMenu.CommonSongs.Where(s => s.Difficulty == PPDFrameworkCore.Difficulty.Easy).ToArray();
                        var normal = leftMenu.CommonSongs.Where(s => s.Difficulty == PPDFrameworkCore.Difficulty.Normal).ToArray();
                        var hard = leftMenu.CommonSongs.Where(s => s.Difficulty == PPDFrameworkCore.Difficulty.Hard).ToArray();
                        var extreme = leftMenu.CommonSongs.Where(s => s.Difficulty == PPDFrameworkCore.Difficulty.Extreme).ToArray();
                        foreach (SongInformation songInfo in SongInformation.All)
                        {
                            if (songInfo.Difficulty.HasFlag(SongInformation.AvailableDifficulty.Easy))
                            {
                                if (easy.FirstOrDefault(s => Utility.IsSameArray(songInfo.EasyHash, s.Hash)) == null)
                                {
                                    continue;
                                }
                            }
                            if (songInfo.Difficulty.HasFlag(SongInformation.AvailableDifficulty.Normal))
                            {
                                if (normal.FirstOrDefault(s => Utility.IsSameArray(songInfo.NormalHash, s.Hash)) == null)
                                {
                                    continue;
                                }
                            }
                            if (songInfo.Difficulty.HasFlag(SongInformation.AvailableDifficulty.Hard))
                            {
                                if (hard.FirstOrDefault(s => Utility.IsSameArray(songInfo.HardHash, s.Hash)) == null)
                                {
                                    continue;
                                }
                            }
                            if (songInfo.Difficulty.HasFlag(SongInformation.AvailableDifficulty.Extreme))
                            {
                                if (extreme.FirstOrDefault(s => Utility.IsSameArray(songInfo.ExtremeHash, s.Hash)) == null)
                                {
                                    continue;
                                }
                            }
                            songInfoSprite.AddChild(new SongInformationComponent(device, resourceManager, songInfo, null)
                            {
                                Position = new SharpDX.Vector2(45, 20 * iter + 77),
                                Hidden = iter >= MaxDisplayCount
                            });
                            iter++;
                        }
                    }
                    break;
            }

            if (index >= songInfoSprite.ChildrenCount)
            {
                index = 0;
                scrollIndex = 0;
            }

            if (CurrentComponent != null)
            {
                CurrentComponent.Selected = true;
            }
            AdjustScrollBar();
            UpdateMovieWaitingCount();
            UpdateSongDetail();
        }

        private void AdjustScrollBar()
        {
            if (index < scrollIndex)
            {
                scrollIndex = index;
            }
            else if (index - scrollIndex > MaxDisplayCount - 1)
            {
                scrollIndex += (index - scrollIndex - MaxDisplayCount + 1);
            }

            for (int i = 0; i < songInfoSprite.ChildrenCount; i++)
            {
                songInfoSprite[i].Position = new Vector2(songInfoSprite[i].Position.X, 20 * (i - scrollIndex) + 77);
                songInfoSprite[i].Hidden = i < scrollIndex || i >= scrollIndex + MaxDisplayCount;
            }

            if (songInfoSprite.ChildrenCount > MaxDisplayCount)
            {
                scrollBar.RectangleHeight = MaxScrollBarHeight * MaxDisplayCount / (float)songInfoSprite.ChildrenCount;
                scrollBar.Position = new SharpDX.Vector2(scrollBar.Position.X, ScrollBarY + (MaxScrollBarHeight - scrollBar.RectangleHeight) * scrollIndex / (songInfoSprite.ChildrenCount - MaxDisplayCount));
            }
            else
            {
                scrollBar.RectangleHeight = MaxScrollBarHeight;
                scrollBar.Position = new SharpDX.Vector2(scrollBar.Position.X, ScrollBarY);
            }
        }

        void SongSelectComponent_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                sound.Play(PPDSetting.DefaultSounds[2], -1000);
                if (CurrentStack.Count == 0)
                {
                    FocusManager.RemoveFocus();
                }
                else
                {
                    switch (mode)
                    {
                        case Mode.LogicInformation:
                            currentLogic = currentLogic.Parent;
                            break;
                        case Mode.SongInformation:
                            currentSong = currentSong.Parent;
                            break;
                    }

                    var selectStructure = CurrentStack.Pop();
                    index = selectStructure.index;
                    scrollIndex = selectStructure.scrollIndex;
                    GenerateSongInformation();
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                if (CurrentComponent != null)
                {
                    sound.Play(PPDSetting.DefaultSounds[1], -1000);
                    if (CurrentComponent.SongInformation != null && CurrentComponent.SongInformation.IsPPDSong)
                    {
                        var dialog = new DifficultyDialog(device, resourceManager, sound, CurrentComponent.SongInformation);
                        dialog.LostFocused += dialog_LostFocused;
                        dialog.Position = CurrentComponent.ScreenPos + new Vector2(20, 20);
                        if (dialog.Position.Y + dialog.Height >= 450)
                        {
                            dialog.Position = CurrentComponent.ScreenPos + new Vector2(20, -dialog.Height);
                        }
                        this.InsertChild(dialog, 0);
                        FocusManager.Focus(dialog);
                    }
                    else
                    {
                        CurrentStack.Push(new SelectStructure
                        {
                            index = index,
                            scrollIndex = scrollIndex
                        });

                        switch (mode)
                        {
                            case Mode.LogicInformation:
                                currentLogic = currentLogic.Children[index];
                                break;
                            case Mode.SongInformation:
                                currentSong = currentSong.Children[index];
                                break;
                        }
                        index = 0;
                        GenerateSongInformation();
                    }
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Triangle))
            {
                if (CurrentComponent == null)
                {
                    return;
                }
                var generated = false;
                switch (mode)
                {
                    case Mode.CommonSongInfo:
                        if (songInfoSprite.ChildrenCount > 0)
                        {
                            CurrentComponent.Selected = false;
                            index = new Random().Next(0, songInfoSprite.ChildrenCount);
                        }
                        break;
                    case Mode.SongInformation:
                        var randomSong = GetRandomSongInfo();
                        if (randomSong == null)
                        {
                            return;
                        }
                        CurrentComponent.Selected = false;

                        if (randomSong.Parent == currentSong)
                        {
                            index = Array.IndexOf(currentSong.Children, randomSong);
                            GenerateSongInformation();
                            generated = true;
                        }
                        else
                        {
                            currentSong = SongInformation.Root;
                            CurrentStack.Clear();
                            var temp = new List<int>();
                            SongInformation songInfo = randomSong.Parent;
                            while (songInfo != SongInformation.Root)
                            {
                                temp.Insert(0, Array.IndexOf(songInfo.Parent.Children, songInfo));
                                songInfo = songInfo.Parent;
                            }

                            foreach (int tempIndex in temp)
                            {
                                CurrentStack.Push(new SelectStructure
                                {
                                    index = tempIndex,
                                    scrollIndex = 0
                                });
                            }
                            currentSong = randomSong.Parent;
                            scrollIndex = 0;
                            index = Array.IndexOf(currentSong.Children, randomSong);
                            GenerateSongInformation();
                            generated = true;
                        }
                        break;
                    case Mode.LogicInformation:
                        var randomLogic = GetRandomLogicInfo();
                        if (randomLogic == null)
                        {
                            return;
                        }
                        CurrentComponent.Selected = false;

                        if (randomLogic.Parent == currentLogic)
                        {
                            index = Array.IndexOf(currentLogic.Children, randomLogic);
                        }
                        else
                        {
                            currentLogic = LogicFolderInfomation.Root;
                            CurrentStack.Clear();
                            var temp = new List<int>();
                            LogicFolderInfomation logicInfo = randomLogic.Parent;
                            while (logicInfo != LogicFolderInfomation.Root)
                            {
                                temp.Insert(0, Array.IndexOf(logicInfo.Parent.Children, logicInfo));
                                logicInfo = logicInfo.Parent;
                            }

                            foreach (int tempIndex in temp)
                            {
                                CurrentStack.Push(new SelectStructure
                                {
                                    index = tempIndex,
                                    scrollIndex = 0
                                });
                            }
                            currentLogic = randomLogic.Parent;
                            scrollIndex = 0;
                            index = Array.IndexOf(currentLogic.Children, randomLogic);
                            GenerateSongInformation();
                            generated = true;
                        }
                        break;
                }
                CurrentComponent.Selected = true;
                AdjustScrollBar();
                UpdateMovieWaitingCount();
                if (!generated)
                {
                    UpdateSongDetail();
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                if (songInfoSprite.ChildrenCount > 0)
                {
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                    CurrentComponent.Selected = false;
                    index--;
                    if (index < 0)
                    {
                        index = songInfoSprite.ChildrenCount - 1;
                    }
                    CurrentComponent.Selected = true;
                    AdjustScrollBar();
                    UpdateMovieWaitingCount();
                    UpdateSongDetail();
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                if (songInfoSprite.ChildrenCount > 0)
                {
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                    CurrentComponent.Selected = false;
                    index++;
                    if (index >= songInfoSprite.ChildrenCount)
                    {
                        index = 0;
                    }
                    CurrentComponent.Selected = true;
                    AdjustScrollBar();
                    UpdateMovieWaitingCount();
                    UpdateSongDetail();
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Left))
            {
                if (songInfoSprite.ChildrenCount > 0)
                {
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                    CurrentComponent.Selected = false;
                    index -= MaxDisplayCount / 2;
                    if (index < 0)
                    {
                        index = songInfoSprite.ChildrenCount - 1;
                    }
                    CurrentComponent.Selected = true;
                    AdjustScrollBar();
                    UpdateMovieWaitingCount();
                    UpdateSongDetail();
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Right))
            {
                if (songInfoSprite.ChildrenCount > 0)
                {
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                    CurrentComponent.Selected = false;
                    index += MaxDisplayCount / 2;
                    if (index >= songInfoSprite.ChildrenCount)
                    {
                        index = 0;
                    }
                    CurrentComponent.Selected = true;
                    AdjustScrollBar();
                    UpdateMovieWaitingCount();
                    UpdateSongDetail();
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.R))
            {
                CurrentStack.Push(new SelectStructure
                {
                    index = index,
                    scrollIndex = scrollIndex
                });
                mode++;
                if (mode > Mode.CommonSongInfo)
                {
                    mode = Mode.SongInformation;
                }

                UpdateModeText();
                var selectStructure = CurrentStack.Pop();
                index = selectStructure.index;
                scrollIndex = selectStructure.scrollIndex;

                GenerateSongInformation();
            }
            else if (args.InputInfo.IsPressed(ButtonType.L))
            {
                CurrentStack.Push(new SelectStructure
                {
                    index = index,
                    scrollIndex = scrollIndex
                });
                mode--;
                if (mode < Mode.SongInformation)
                {
                    mode = Mode.CommonSongInfo;
                }

                UpdateModeText();
                var selectStructure = CurrentStack.Pop();
                index = selectStructure.index;
                scrollIndex = selectStructure.scrollIndex;

                GenerateSongInformation();
            }
        }

        private void UpdateSongDetail()
        {
            if (CurrentComponent == null)
            {
                songDetail.SetInfo(null);
            }
            else
            {
                songDetail.SetInfo(CurrentComponent.SongInformation);
            }
        }

        private void UpdateMovieWaitingCount()
        {
            if (movie != null && !isWaitingMovieChange)
            {
                if (movie.Movie != null)
                {
                    movie.Movie.FadeOut();
                }
            }
            isWaitingMovieChange = true;
            waitingMovieChangeCount = 0;
        }

        void movieManager_MovieChanged(object sender, EventArgs e)
        {
            if (movie != null)
            {
                back.RemoveChild(movie);
                movie = null;
            }
            movie = movieManager.Movie;
            if (movie != null)
            {
                movie.Position = new Vector2(517, 80);
                movie.Scale = new Vector2(0.3f, 0.3f);
                back.AddChild(movie);
            }
        }

        private void UpdateMovie()
        {
            movieManager.Change(CurrentComponent?.SongInformation, true);
        }

        private void UpdateModeText()
        {
            string text = "";
            switch (mode)
            {
                case Mode.CommonSongInfo:
                    text = Utility.Language["CommonScore"];
                    break;
                case Mode.LogicInformation:
                    text = Utility.Language["Link"];
                    break;
                case Mode.SongInformation:
                    text = Utility.Language["MusicScore"];
                    break;
            }
            modeString.Text = text;
        }

        private SongInformation GetRandomSongInfo()
        {
            var list = new List<SongInformation>();
            var folders = new Queue<SongInformation>();
            folders.Enqueue(SongInformation.Root);
            while (folders.Count > 0)
            {
                var folder = folders.Dequeue();
                foreach (SongInformation songInfo in folder.Children)
                {
                    if (songInfo.IsPPDSong)
                    {
                        list.Add(songInfo);
                    }
                    else
                    {
                        folders.Enqueue(songInfo);
                    }
                }
            }

            if (list.Count > 0)
            {
                var select = new Random().Next(0, list.Count);
                return list[select];
            }
            else
            {
                return null;
            }
        }

        private LogicFolderInfomation GetRandomLogicInfo()
        {
            var list = new List<LogicFolderInfomation>();
            var folders = new Queue<LogicFolderInfomation>();
            folders.Enqueue(LogicFolderInfomation.Root);
            while (folders.Count > 0)
            {
                var folder = folders.Dequeue();
                foreach (LogicFolderInfomation logicInfo in folder.Children)
                {
                    if (!logicInfo.IsFolder)
                    {
                        list.Add(logicInfo);
                    }
                    else
                    {
                        folders.Enqueue(logicInfo);
                    }
                }
            }

            if (list.Count > 0)
            {
                var select = new Random().Next(0, list.Count);
                return list[select];
            }
            else
            {
                return null;
            }
        }

        void dialog_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            var dialog = sender as DifficultyDialog;

            if (dialog.Selected)
            {
                sound.Play(PPDSetting.DefaultSounds[1], -1000);
                SongInformation = dialog.SongInformation;
                Difficulty = dialog.Difficulty;
                if (SongSelected != null)
                {
                    SongSelected.Invoke(this, EventArgs.Empty);
                }
            }

            this.RemoveChild(dialog);
        }

        void SongSelectComponent_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            if (!(args.FocusObject is DifficultyDialog))
            {
                back.Position = new SharpDX.Vector2(0, 50);
                Alpha = 0;
            }
        }

        protected override void UpdateImpl()
        {
            if (OverFocused)
            {
                back.Position = new SharpDX.Vector2(0, AnimationUtility.GetAnimationValue(back.Position.Y, 0));
                Alpha = AnimationUtility.IncreaseAlpha(Alpha);
            }
            else
            {
                back.Position = new SharpDX.Vector2(0, AnimationUtility.GetAnimationValue(back.Position.Y, 50));
                Alpha = AnimationUtility.DecreaseAlpha(Alpha);
            }

            if (isWaitingMovieChange)
            {
                waitingMovieChangeCount++;
                if (waitingMovieChangeCount >= 60)
                {
                    isWaitingMovieChange = false;
                    UpdateMovie();
                }
            }
        }

        class SongInformationComponent : GameComponent
        {
            PPDFramework.Resource.ResourceManager resourceManager;
            bool selected;

            TextureString songName;
            RectangleComponent rectangle;

            public SongInformationComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, SongInformation songInformation, LogicFolderInfomation logicFolderInformation) : base(device)
            {
                this.resourceManager = resourceManager;
                SongInformation = songInformation;
                LogicFolderInformation = logicFolderInformation;

                if (SongInformation == null)
                {
                    SongInformation = SongInformation.FindSongInformationByID(logicFolderInformation.ScoreID);
                }

                if (SongInformation != null && SongInformation.IsPPDSong)
                {
                    this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("scoremanager", "score.png"))
                    {
                        Scale = new SharpDX.Vector2(0.8f)
                    });
                }
                else
                {
                    this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("folder.png"))
                    {
                        Position = new Vector2(0, 2),
                        Scale = new SharpDX.Vector2(0.5f)
                    });
                }
                this.AddChild(songName = new TextureString(device, DisplayName, 14, 420, PPDColors.White)
                {
                    Position = new Vector2(15, 0)
                });
                songName.Update();

                this.AddChild(rectangle = new RectangleComponent(device, resourceManager, PPDColors.White)
                {
                    Position = songName.Position,
                    RectangleHeight = 20,
                    RectangleWidth = songName.JustWidth,
                    Hidden = true
                });
            }

            private string DisplayName
            {
                get
                {
                    return LogicFolderInformation == null ? SongInformation.DirectoryName : LogicFolderInformation.Name;
                }
            }

            public bool Selected
            {
                get
                {
                    return selected;
                }
                set
                {
                    if (selected != value)
                    {
                        selected = value;
                        songName.Color = selected ? PPDColors.Black : PPDColors.White;
                        rectangle.Hidden = !selected;
                    }
                }
            }

            public SongInformation SongInformation
            {
                get;
                private set;
            }

            public LogicFolderInfomation LogicFolderInformation
            {
                get;
                private set;
            }
        }

        class SongDetailComponent : FocusableGameComponent
        {
            PPDFramework.Resource.ResourceManager resourceManager;
            TextureString authorText;
            TextureString authorNameText;
            TextureString easyText;
            TextureString normalText;
            TextureString hardText;
            TextureString extremeText;
            TextureString easyDifficultyText;
            TextureString normalDifficultyText;
            TextureString hardDifficultyText;
            TextureString extremeDifficultyText;
            TextureString easyPtText;
            TextureString normalPtText;
            TextureString hardPtText;
            TextureString extremePtText;
            TextureString[] easyTexts;
            TextureString[] normalTexts;
            TextureString[] hardTexts;
            TextureString[] extremeTexts;

            DifficultyMeasureWorker worker;

            public SongDetailComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
            {
                this.resourceManager = resourceManager;

                this.AddChild(new StackObject(device,
                    authorText = new TextureString(device, String.Format("{0}:", Utility.Language["Author"]), 14, PPDColors.White),
                    new SpaceObject(device, 0, 5),
                    new StackObject(device,
                        new SpaceObject(device, 10, 0),
                        authorNameText = new TextureString(device, "", 14, 200, PPDColors.White)
                    )
                    {
                        IsHorizontal = true
                    },
                    new SpaceObject(device, 0, 7),
                    new StackObject(device,
                        easyText = new TextureString(device, "EASY:", 14, PPDColors.White),
                        easyPtText = new TextureString(device, "10Pt", 14, PPDColors.White)
                    )
                    {
                        IsHorizontal = true
                    },
                    new SpaceObject(device, 0, 4),
                    new StackObject(device,
                        new SpaceObject(device, 10, 0),
                        easyDifficultyText = new TextureString(device, "", 14, 200, PPDColors.White)
                    )
                    {
                        IsHorizontal = true
                    },
                    new SpaceObject(device, 0, 7),
                    new StackObject(device,
                        normalText = new TextureString(device, "NORMAL:", 14, PPDColors.White),
                        normalPtText = new TextureString(device, "10Pt", 14, PPDColors.White)
                    )
                    {
                        IsHorizontal = true
                    },
                    new SpaceObject(device, 0, 4),
                    new StackObject(device,
                        new SpaceObject(device, 10, 0),
                        normalDifficultyText = new TextureString(device, "", 14, 200, PPDColors.White)
                    )
                    {
                        IsHorizontal = true
                    },
                    new SpaceObject(device, 0, 7),
                    new StackObject(device,
                        hardText = new TextureString(device, "HARD:", 14, PPDColors.White),
                        hardPtText = new TextureString(device, "10Pt", 14, PPDColors.White)
                    )
                    {
                        IsHorizontal = true
                    },
                    new SpaceObject(device, 0, 4),
                    new StackObject(device,
                        new SpaceObject(device, 10, 0),
                        hardDifficultyText = new TextureString(device, "", 14, 200, PPDColors.White)
                    )
                    {
                        IsHorizontal = true
                    },
                    new SpaceObject(device, 0, 7),
                    new StackObject(device,
                        extremeText = new TextureString(device, "EXTREME:", 14, PPDColors.White),
                        extremePtText = new TextureString(device, "10Pt", 14, PPDColors.White)
                    )
                    {
                        IsHorizontal = true
                    },
                    new SpaceObject(device, 0, 4),
                    new StackObject(device,
                        new SpaceObject(device, 10, 0),
                        extremeDifficultyText = new TextureString(device, "", 14, 200, PPDColors.White)
                    )
                    {
                        IsHorizontal = true
                    }
                ));
                easyTexts = new TextureString[] { easyText, easyPtText, easyDifficultyText };
                normalTexts = new TextureString[] { normalText, normalPtText, normalDifficultyText };
                hardTexts = new TextureString[] { hardText, hardPtText, hardDifficultyText };
                extremeTexts = new TextureString[] { extremeText, extremePtText, extremeDifficultyText };
            }

            private void UpdateColor(TextureString[] texts, Color4 color)
            {
                foreach (var text in texts)
                {
                    text.Color = color;
                }
            }

            public void SetInfo(SongInformation songInfo)
            {
                if (worker != null)
                {
                    worker.Stop();
                    worker = null;
                }

                if (songInfo == null)
                {
                    authorNameText.Text = easyDifficultyText.Text = normalDifficultyText.Text = hardDifficultyText.Text = extremeDifficultyText.Text =
                    easyPtText.Text = normalPtText.Text = hardPtText.Text = extremePtText.Text = "";
                    authorText.Color = authorNameText.Color = PPDColors.Gray;
                    UpdateColor(easyTexts, PPDColors.Gray);
                    UpdateColor(normalTexts, PPDColors.Gray);
                    UpdateColor(hardTexts, PPDColors.Gray);
                    UpdateColor(extremeTexts, PPDColors.Gray);
                    return;
                }

                authorNameText.Text = songInfo.IsPPDSong ? songInfo.AuthorName : "";
                authorText.Color = authorNameText.Color = songInfo.IsPPDSong ? PPDColors.White : PPDColors.Gray;

                easyDifficultyText.Text = songInfo.GetDifficultyString(Difficulty.Easy);
                normalDifficultyText.Text = songInfo.GetDifficultyString(Difficulty.Normal);
                hardDifficultyText.Text = songInfo.GetDifficultyString(Difficulty.Hard);
                extremeDifficultyText.Text = songInfo.GetDifficultyString(Difficulty.Extreme);
                easyPtText.Text = songInfo.Difficulty.HasFlag(SongInformation.AvailableDifficulty.Easy) ?
                    Utility.Language["Loading"] : "";
                normalPtText.Text = songInfo.Difficulty.HasFlag(SongInformation.AvailableDifficulty.Normal) ?
                    Utility.Language["Loading"] : "";
                hardPtText.Text = songInfo.Difficulty.HasFlag(SongInformation.AvailableDifficulty.Hard) ?
                    Utility.Language["Loading"] : "";
                extremePtText.Text = songInfo.Difficulty.HasFlag(SongInformation.AvailableDifficulty.Extreme) ?
                    Utility.Language["Loading"] : "";
                UpdateColor(easyTexts, songInfo.Difficulty.HasFlag(SongInformation.AvailableDifficulty.Easy) ? PPDColors.White : PPDColors.Gray);
                UpdateColor(normalTexts, songInfo.Difficulty.HasFlag(SongInformation.AvailableDifficulty.Normal) ? PPDColors.White : PPDColors.Gray);
                UpdateColor(hardTexts, songInfo.Difficulty.HasFlag(SongInformation.AvailableDifficulty.Hard) ? PPDColors.White : PPDColors.Gray);
                UpdateColor(extremeTexts, songInfo.Difficulty.HasFlag(SongInformation.AvailableDifficulty.Extreme) ? PPDColors.White : PPDColors.Gray);

                if (songInfo.IsPPDSong)
                {
                    worker = new DifficultyMeasureWorker(songInfo);
                    worker.Start();
                }
            }

            protected override void UpdateImpl()
            {
                if (worker != null && !worker.IsWorking)
                {
                    easyPtText.Text = worker.SongInformation.Difficulty.HasFlag(SongInformation.AvailableDifficulty.Easy) ?
                        String.Format("{0:F2}({1:F2})pt", worker.EasyResult.Average, worker.EasyResult.Peak) : "";
                    normalPtText.Text = worker.SongInformation.Difficulty.HasFlag(SongInformation.AvailableDifficulty.Normal) ?
                            String.Format("{0:F2}({1:F2})pt", worker.NormalResult.Average, worker.NormalResult.Peak) : "";
                    hardPtText.Text = worker.SongInformation.Difficulty.HasFlag(SongInformation.AvailableDifficulty.Hard) ?
                            String.Format("{0:F2}({1:F2})pt", worker.HardResult.Average, worker.HardResult.Peak) : "";
                    extremePtText.Text = worker.SongInformation.Difficulty.HasFlag(SongInformation.AvailableDifficulty.Extreme) ?
                         String.Format("{0:F2}({1:F2})pt", worker.ExtremeResult.Average, worker.ExtremeResult.Peak) : "";
                    worker = null;
                }
            }

            class DifficultyMeasureWorker
            {
                private Thread thread;

                public SongInformation SongInformation
                {
                    get;
                    private set;
                }

                public bool IsWorking
                {
                    get;
                    private set;
                }

                public ScoreDifficultyMeasureResult EasyResult
                {
                    get;
                    private set;
                }

                public ScoreDifficultyMeasureResult NormalResult
                {
                    get;
                    private set;
                }

                public ScoreDifficultyMeasureResult HardResult
                {
                    get;
                    private set;
                }

                public ScoreDifficultyMeasureResult ExtremeResult
                {
                    get;
                    private set;
                }

                public DifficultyMeasureWorker(SongInformation songInfo)
                {
                    SongInformation = songInfo;
                }

                public void Start()
                {
                    IsWorking = true;
                    thread = ThreadManager.Instance.GetThread(() =>
                    {
                        if (SongInformation.Difficulty.HasFlag(SongInformation.AvailableDifficulty.Easy))
                        {
                            EasyResult = SongInformation.CalculateDifficulty(Difficulty.Easy);
                        }
                        if (SongInformation.Difficulty.HasFlag(SongInformation.AvailableDifficulty.Normal))
                        {
                            NormalResult = SongInformation.CalculateDifficulty(Difficulty.Normal);
                        }
                        if (SongInformation.Difficulty.HasFlag(SongInformation.AvailableDifficulty.Hard))
                        {
                            HardResult = SongInformation.CalculateDifficulty(Difficulty.Hard);
                        }
                        if (SongInformation.Difficulty.HasFlag(SongInformation.AvailableDifficulty.Extreme))
                        {
                            ExtremeResult = SongInformation.CalculateDifficulty(Difficulty.Extreme);
                        }
                        IsWorking = false;
                    });
                    thread.Start();
                }

                public void Stop()
                {
                    if (thread != null && thread.IsAlive)
                    {
                        thread.Abort();
                        thread = null;
                    }
                }
            }
        }

        class DifficultyDialog : FocusableGameComponent
        {
            PPDFramework.Resource.ResourceManager resourceManager;
            ISound sound;
            SongInformation songInfo;
            SpriteObject textSprite;

            RectangleComponent selectRectangle;

            PictureObject back;

            int index;

            public DifficultyDialog(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound, SongInformation songInfo) : base(device)
            {
                this.resourceManager = resourceManager;
                this.sound = sound;
                this.songInfo = songInfo;

                this.AddChild(new TextureString(device, Utility.Language["SelectDifficulty"], 14, PPDColors.White)
                {
                    Position = new Vector2(10, 7)
                });
                this.AddChild(textSprite = new SpriteObject(device)
                {
                    Position = new Vector2(5, 35)
                });
                this.AddChild(selectRectangle = new RectangleComponent(device, resourceManager, PPDColors.White)
                {
                    RectangleWidth = 100,
                    RectangleHeight = 22
                });
                this.AddChild(back = new PictureObject(device, resourceManager, Utility.Path.Combine("small_dialog_back.png")));
                this.AddChild(new RectangleComponent(device, resourceManager, PPDColors.Black)
                {
                    RectangleWidth = back.Width,
                    RectangleHeight = back.Height,
                    Alpha = 0.75f
                });

                textSprite.AddChild(new TextureString(device, "EASY", 16, GetTextColor(SongInformation.AvailableDifficulty.Easy))
                {
                    Position = new Vector2(10, 0)
                });
                textSprite.AddChild(new TextureString(device, "NORMAL", 16, GetTextColor(SongInformation.AvailableDifficulty.Normal))
                {
                    Position = new Vector2(10, 25)
                });
                textSprite.AddChild(new TextureString(device, "HARD", 16, GetTextColor(SongInformation.AvailableDifficulty.Hard))
                {
                    Position = new Vector2(10, 50)
                });
                textSprite.AddChild(new TextureString(device, "EXTREME", 16, GetTextColor(SongInformation.AvailableDifficulty.Extreme))
                {
                    Position = new Vector2(10, 75)
                });

                if ((songInfo.Difficulty & SongInformation.AvailableDifficulty.Easy) == SongInformation.AvailableDifficulty.Easy)
                {
                    index = 0;
                }
                else if ((songInfo.Difficulty & SongInformation.AvailableDifficulty.Normal) == SongInformation.AvailableDifficulty.Normal)
                {
                    index = 1;
                }
                else if ((songInfo.Difficulty & SongInformation.AvailableDifficulty.Hard) == SongInformation.AvailableDifficulty.Hard)
                {
                    index = 2;
                }
                else if ((songInfo.Difficulty & SongInformation.AvailableDifficulty.Extreme) == SongInformation.AvailableDifficulty.Extreme)
                {
                    index = 3;
                }

                ChangeSelect();

                Inputed += DifficultyDialog_Inputed;
            }

            private TextureString CurrentText
            {
                get
                {
                    return textSprite[index] as TextureString;
                }
            }

            public SongInformation SongInformation
            {
                get
                {
                    return songInfo;
                }
            }

            public bool Selected
            {
                get;
                private set;
            }

            public Difficulty Difficulty
            {
                get;
                private set;
            }

            private void ChangeSelect()
            {
                CurrentText.Color = PPDColors.Black;
                selectRectangle.Position = textSprite[index].Position + textSprite.Position;
                selectRectangle.RectangleWidth = textSprite[index].Width;
            }

            private Color4 GetTextColor(PPDFramework.SongInformation.AvailableDifficulty difficulty)
            {
                return (songInfo.Difficulty & difficulty) == difficulty ? PPDColors.White : PPDColors.Gray;
            }

            private bool DifficultyAvailable()
            {
                switch (index)
                {
                    case 0:
                        return (songInfo.Difficulty & SongInformation.AvailableDifficulty.Easy) == SongInformation.AvailableDifficulty.Easy;
                    case 1:
                        return (songInfo.Difficulty & SongInformation.AvailableDifficulty.Normal) == SongInformation.AvailableDifficulty.Normal;
                    case 2:
                        return (songInfo.Difficulty & SongInformation.AvailableDifficulty.Hard) == SongInformation.AvailableDifficulty.Hard;
                    case 3:
                        return (songInfo.Difficulty & SongInformation.AvailableDifficulty.Extreme) == SongInformation.AvailableDifficulty.Extreme;
                }
                return false;
            }

            void DifficultyDialog_Inputed(IFocusable sender, InputEventArgs args)
            {
                if (args.InputInfo.IsPressed(ButtonType.Cross))
                {
                    FocusManager.RemoveFocus();
                }
                else if (args.InputInfo.IsPressed(ButtonType.Circle))
                {
                    Selected = true;
                    Difficulty = (Difficulty)index;
                    FocusManager.RemoveFocus();
                }
                else if (args.InputInfo.IsPressed(ButtonType.Up))
                {
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                    CurrentText.Color = PPDColors.White;
                    index--;
                    int iter = 0;
                    while (iter < 4)
                    {
                        if (index < 0)
                        {
                            index = 3;
                        }
                        if (DifficultyAvailable())
                        {
                            break;
                        }
                        index--;
                        iter++;
                    }
                    ChangeSelect();
                }
                else if (args.InputInfo.IsPressed(ButtonType.Down))
                {
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                    CurrentText.Color = PPDColors.White;
                    index++;
                    int iter = 0;
                    while (iter < 4)
                    {
                        if (index > 3)
                        {
                            index = 0;
                        }
                        if (DifficultyAvailable())
                        {
                            break;
                        }
                        index++;
                        iter++;
                    }
                    ChangeSelect();
                }
            }
        }

        protected override void DisposeResource()
        {
            SongInformation.Updated -= SongInformation_Updated;
            base.DisposeResource();
        }
    }
}
