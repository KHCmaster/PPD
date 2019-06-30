using PPDFramework;
using PPDFramework.Web;
using PPDFrameworkCore;
using PPDShareComponent;
using SharpDX;
using System;
using System.Linq;

namespace PPDSingle
{
    class ReplayListComponent : FocusableGameComponent
    {
        const int ClipY = 80;
        const int ClipHeight = 330;
        const int SpriteY = 90;
        const int ItemHeight = 50;

        IGameHost gameHost;
        PPDFramework.Resource.ResourceManager resourceManager;
        ISound sound;
        ReplayInfo[] replays;
        LineRectangleComponent rectangle;

        SpriteObject mainSprite;
        SpriteObject waitSprite;
        bool loading;
        int selection;

        private ReplayComponent SelectedComponent
        {
            get
            {
                return (ReplayComponent)mainSprite.GetChildAt(selection);
            }
        }

        public ReplayListComponent(PPDDevice device, IGameHost gameHost, PPDFramework.Resource.ResourceManager resourceManager, ISound sound) : base(device)
        {
            this.gameHost = gameHost;
            this.resourceManager = resourceManager;
            this.sound = sound;
            PictureObject back;

            waitSprite = new SpriteObject(device)
            {
                Hidden = true
            };
            this.AddChild(waitSprite);
            waitSprite.AddChild(new TextureString(device, Utility.Language["LoadingReplays"], 20, true, PPDColors.White)
            {
                Position = new Vector2(400, 220)
            });
            waitSprite.AddChild(new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0.65f
            });
            rectangle = new LineRectangleComponent(device, resourceManager, PPDColors.Selection)
            {
                Hidden = true,
                RectangleWidth = 700,
                RectangleHeight = ItemHeight
            };
            this.AddChild(rectangle);
            mainSprite = new SpriteObject(device)
            {
                Position = new Vector2(50, SpriteY),
                Clip = new ClipInfo(gameHost)
                {
                    PositionX = 40,
                    PositionY = ClipY,
                    Width = 750,
                    Height = ClipHeight
                }
            };
            this.AddChild(mainSprite);
            this.AddChild(back = new PictureObject(device, resourceManager, Utility.Path.Combine("dialog_back.png")));
            this.AddChild(new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0.80f
            });
            back.AddChild(new TextureString(device, Utility.Language["ReplayList"], 30, PPDColors.White)
            {
                Position = new Vector2(35, 30)
            });

            Inputed += ReplayListComponent_Inputed;
            GotFocused += ReplayListComponent_GotFocused;
        }

        void ReplayListComponent_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            if (args.FocusObject is LeftMenu)
            {
                Load();
            }
        }

        void ReplayListComponent_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (!waitSprite.Hidden)
            {
                return;
            }

            if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                sound.Play(PPDSetting.DefaultSounds[2], -1000);
                FocusManager.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                if (mainSprite.ChildrenCount > 0)
                {
                    sound.Play(PPDSetting.DefaultSounds[1], -1000);
                    if (SelectedComponent.SongInfo == null)
                    {
                        var obd = new OpenBrowserDialog(device, resourceManager, sound)
                        {
                            ScoreLibraryId = SelectedComponent.WebSongInfo.Hash
                        };
                        obd.LostFocused += obd_LostFocused;
                        this.InsertChild(obd, 0);
                        FocusManager.Focus(obd);
                    }
                    else
                    {
                        var gd = new GeneralDialog(device, resourceManager, sound, Utility.Language["ReplayConfirm"], GeneralDialog.ButtonTypes.OkCancel);
                        gd.LostFocused += gd_LostFocused;
                        this.InsertChild(gd, 0);
                        FocusManager.Focus(gd);
                    }
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                if (mainSprite.ChildrenCount > 0)
                {
                    selection++;
                    if (selection >= mainSprite.ChildrenCount)
                    {
                        selection = 0;
                    }
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                if (mainSprite.ChildrenCount > 0)
                {
                    selection--;
                    if (selection < 0)
                    {
                        selection = mainSprite.ChildrenCount - 1;
                    }
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
            }
        }

        void gd_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            var gd = sender as GeneralDialog;
            if (gd.Result == 0)
            {
                if (FocusManager.BaseScene is Menu menu)
                {
                    menu.Replay(SelectedComponent.SongInfo, SelectedComponent.Difficulty, SelectedComponent.Replay);
                }
            }
            gd.Dispose();
            this.RemoveChild(gd);
        }

        void obd_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            var obd = sender as OpenBrowserDialog;
            this.RemoveChild(obd);
            obd.Dispose();
        }

        private void Load()
        {
            if (loading)
            {
                return;
            }
            loading = true;
            waitSprite.Hidden = false;
            ThreadManager.Instance.GetThread(() =>
            {
                replays = WebManager.Instance.GetReplayInfos();
                WebSongInformationManager.Instance.Update(false);
                loading = false;
            }).Start();
        }

        private void Generate()
        {
            mainSprite.ClearChildren();
            int iter = 0;
            foreach (var replay in replays)
            {
                var songInfo = SongInformation.FindSongInformationByHash(CryptographyUtility.Parsex2String(replay.ScoreHash));
                WebSongInformation webSongInfo = null;
                Difficulty difficulty = Difficulty.Other;
                if (songInfo == null)
                {
                    webSongInfo = WebSongInformationManager.Instance[replay.ScoreHash];
                    if (webSongInfo == null)
                    {
                        continue;
                    }
                    var diff = webSongInfo.Difficulties.FirstOrDefault(w => w.Hash == replay.ScoreHash);
                    if (diff != null)
                    {
                        songInfo = webSongInfo.GetSongInformation();
                        difficulty = diff.Difficulty;
                    }
                }
                else
                {
                    if (songInfo != null)
                    {
                        difficulty = songInfo.GetDifficulty(replay.ScoreHash);
                    }
                }
                var control = new ReplayComponent(device, resourceManager, replay, songInfo, webSongInfo, difficulty)
                {
                    Position = new Vector2(0, iter * ItemHeight)
                };
                mainSprite.AddChild(control);
                iter++;
            }
            rectangle.Hidden = !(mainSprite.ChildrenCount > 0);
            rectangle.Position = mainSprite.Position;
            selection = 0;
        }

        private void UpdateScroll()
        {
            var temp = SelectedComponent.ScreenPos;
            var target = 0f;
            if (temp.Y < ClipY && temp.Y + ItemHeight < ClipHeight)
            {
                target = ClipY - temp.Y;
            }
            else if (temp.Y + ItemHeight + 10 > ClipY + ClipHeight)
            {
                target = ClipY + ClipHeight - (temp.Y + ItemHeight + 10);
            }
            mainSprite.Position = new Vector2(mainSprite.Position.X,
                AnimationUtility.GetAnimationValue(mainSprite.Position.Y, mainSprite.Position.Y + target));
        }

        protected override void UpdateImpl()
        {
            if (!loading)
            {
                if (!waitSprite.Hidden)
                {
                    Generate();
                    waitSprite.Hidden = true;
                }
            }
            if (!rectangle.Hidden)
            {
                UpdateScroll();
                rectangle.Position = AnimationUtility.GetAnimationPosition(rectangle.Position, mainSprite.Position + new Vector2(0, ItemHeight * selection));
            }
        }

        protected override bool OnCanUpdate()
        {
            return !Hidden && OverFocused;
        }

        protected override bool OnCanDraw(PPDFramework.Shaders.AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return OverFocused;
        }

        class ReplayComponent : GameComponent
        {
            private PPDFramework.Resource.ResourceManager resourceManager;

            public ReplayInfo Replay
            {
                get;
                private set;
            }

            public SongInformation SongInfo
            {
                get;
                private set;
            }

            public WebSongInformation WebSongInfo
            {
                get;
                private set;
            }

            public Difficulty Difficulty
            {
                get;
                private set;
            }

            public ReplayComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ReplayInfo replay, SongInformation songInfo, WebSongInformation webSongInfo, Difficulty difficulty) : base(device)
            {
                this.resourceManager = resourceManager;
                Replay = replay;
                SongInfo = songInfo;
                WebSongInfo = webSongInfo;
                Difficulty = difficulty;

                var color = songInfo == null ? PPDColors.Gray : PPDColors.White;
                AddChild(new TextureString(device, String.Format("{0}[{1}]", songInfo == null ? webSongInfo.Title : songInfo.DirectoryName, difficulty), 20, 400, color)
                {
                    AllowScroll = true
                });
                this.AddChild(new TextureString(device, replay.Nickname, 20, 300, color)
                {
                    AllowScroll = true,
                    Position = new Vector2(700, 0),
                    Alignment = Alignment.Right
                });
                AddChild(new TextureString(device, String.Format("{0}:{1} C{2} G{3} SF{4} SD{5} W{6} MC{7}", Utility.Language["Score2"], replay.Score,
                    replay.CoolCount, replay.GoodCount, replay.SafeCount, replay.SadCount, replay.WorstCount, replay.MaxCombo), 16, 700, color)
                {
                    Position = new Vector2(0, 26),
                    AllowScroll = true
                });
            }
        }
    }
}
