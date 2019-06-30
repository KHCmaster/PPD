using PPDFramework;
using PPDShareComponent;
using SharpDX;
using System;
using System.Collections;

namespace PPDSingle
{
    /// <summary>
    /// 譜面選択のUI
    /// </summary>
    class MenuSelectSong : FocusableGameComponent
    {
        enum State
        {
            NotAppeared = 0,
            Moving = 1,
            Appeared = 2,
            Still = 3
        }
        public event EventHandler DisapperFinish;

        const int ItemsDefaultX = 5;
        const int ItemsDefaultY = 70;
        const int ItemCount = 9;
        ArrayList pictureobjects;
        PictureObject mode;
        TextureString modeStringLeft;
        TextureString modeStringCenter;
        TextureString modeStringRight;
        SelectedSongInfo[] infos;
        SpriteObject songNameSprite;
        SongNameComponent[] songNames;

        private int selectedIndex;
        State state;
        Menu menu;
        public MenuSelectSong(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, Menu menu) : base(device)
        {
            this.menu = menu;
            state = State.NotAppeared;
            pictureobjects = new ArrayList();

            mode = new PictureObject(device, resourceManager, Utility.Path.Combine("modeselect.png"))
            {
                Position = new Vector2(-10, 20)
            };
            modeStringLeft = new TextureString(device, Utility.Language["List"], 16, true, PPDColors.Gray)
            {
                Position = new Vector2(50, 25)
            };
            modeStringCenter = new TextureString(device, Utility.Language["Score"], 16, true, PPDColors.White)
            {
                Position = new Vector2(170, 25)
            };
            modeStringRight = new TextureString(device, Utility.Language["Link"], 16, true, PPDColors.Gray)
            {
                Position = new Vector2(290, 25)
            };
            songNameSprite = new SpriteObject(device)
            {
                Position = new Vector2(0, ItemsDefaultY)
            };
            songNames = new SongNameComponent[ItemCount];
            for (var i = 0; i < songNames.Length; i++)
            {
                songNames[i] = new SongNameComponent(device, resourceManager, i == (ItemCount / 2))
                {
                    Position = new Vector2(0, (SongNameComponent.ItemHeight + 5) * i + (i > (ItemCount / 2) ? 10 : 0)),
                    ItemAlpha = (float)1 / (Math.Abs(i - (ItemCount / 2)) + 1)
                };
                songNameSprite.AddChild(songNames[i]);
            }
            this.AddChild(songNameSprite);

            this.AddChild(modeStringLeft);
            this.AddChild(modeStringCenter);
            this.AddChild(modeStringRight);
            this.AddChild(mode);
        }

        /// <summary>
        /// 譜面リスト
        /// </summary>
        public SelectedSongInfo[] SongInformations
        {
            set
            {
                infos = value;
            }
        }

        protected override void UpdateImpl()
        {
            songNameSprite.Position = new Vector2(songNameSprite.Position.X, AnimationUtility.GetAnimationValue(songNameSprite.Position.Y, ItemsDefaultY, 0.2f));
            if (state == State.Appeared)
            {
                var dest = -SongNameComponent.ItemWidth - 100;
                songNameSprite.Position = new Vector2(AnimationUtility.GetAnimationValue(songNameSprite.Position.X, dest, 0.2f), songNameSprite.Position.Y);
                if (songNameSprite.Position.X <= -SongNameComponent.ItemWidth)
                {
                    if (this.DisapperFinish != null)
                    {
                        this.DisapperFinish(this, new EventArgs());
                        this.state = State.Still;
                    }
                }
                this.Alpha -= 0.05f;
                if (this.Alpha <= 0)
                {
                    this.Alpha = 0;
                }
            }
            else if (state == State.NotAppeared)
            {
                songNameSprite.Position = new Vector2(AnimationUtility.GetAnimationValue(songNameSprite.Position.X, 0, 0.2f), songNameSprite.Position.Y);
                if (songNameSprite.Position.X >= 0)
                {
                    this.state = State.Still;
                    this.Alpha = 1;
                }
                this.Alpha += 0.05f;
                if (this.Alpha >= 1)
                {
                    this.Alpha = 1;
                }
            }
        }

        protected override bool OnCanUpdate()
        {
            return !Hidden;
        }

        public void ChangeIndex(int index, SelectSongManager.SongChangeMode changeMode)
        {
            if (infos == null || infos.Length == 0)
            {
                foreach (var name in songNames)
                {
                    name.Hidden = true;
                }
                return;
            }
            var dest = songNameSprite.Position.Y;
            switch (changeMode)
            {
                case SelectSongManager.SongChangeMode.Down:
                    dest -= -SongNameComponent.ItemHeight;
                    break;
                case SelectSongManager.SongChangeMode.Reset:
                    break;
                case SelectSongManager.SongChangeMode.Up:
                    dest += -SongNameComponent.ItemHeight;
                    break;
            }
            if (dest >= ItemsDefaultY + SongNameComponent.ItemHeight)
            {
                dest = ItemsDefaultY + SongNameComponent.ItemHeight;
            }
            if (dest <= ItemsDefaultY - SongNameComponent.ItemHeight)
            {
                dest = ItemsDefaultY - SongNameComponent.ItemHeight;
            }
            songNameSprite.Position = new Vector2(songNameSprite.Position.X, dest);
            selectedIndex = index;
            index = selectedIndex - ItemCount / 2;
            foreach (var name in songNames)
            {
                var actualIndex = index;
                while (actualIndex < 0)
                {
                    actualIndex += infos.Length;
                }
                while (actualIndex >= infos.Length)
                {
                    actualIndex -= infos.Length;
                }
                var info = infos[actualIndex];
                name.Name = info.Text;
                name.ShowFolder = info.IsFolder;
                name.ShowAC = info.SongInfo != null && info.SongInfo.HasAC;
                name.ShowFT = info.SongInfo != null && info.SongInfo.HasACFT;
                name.PerfectAlpha = 0;
                if (info.SongInfo != null && !info.IsFolder)
                {
                    name.PerfectAlpha = info.PerfectRatio;
                }
                name.Hidden = false;
                index++;
            }
        }

        public void Disappear()
        {
            state = State.Appeared;
        }
        public void Start()
        {
            state = State.NotAppeared;
        }
        public void ChangeMode(SelectSongManager.Mode mode)
        {
            switch (mode)
            {
                case SelectSongManager.Mode.SongInfo:
                    modeStringLeft.Text = Utility.Language["List"];
                    modeStringCenter.Text = Utility.Language["Score"];
                    modeStringRight.Text = Utility.Language["Link"];
                    break;
                case SelectSongManager.Mode.LogicFolder:
                    modeStringLeft.Text = Utility.Language["Score"];
                    modeStringCenter.Text = Utility.Language["Link"];
                    modeStringRight.Text = Utility.Language["ActiveScore"];
                    break;
                case SelectSongManager.Mode.ActiveScore:
                    modeStringLeft.Text = Utility.Language["Link"];
                    modeStringCenter.Text = Utility.Language["ActiveScore"];
                    modeStringRight.Text = Utility.Language["Contest"];
                    break;
                case SelectSongManager.Mode.Contest:
                    modeStringLeft.Text = Utility.Language["ActiveScore"];
                    modeStringCenter.Text = Utility.Language["Contest"];
                    modeStringRight.Text = Utility.Language["List"];
                    break;
                case SelectSongManager.Mode.List:
                    modeStringLeft.Text = Utility.Language["Contest"];
                    modeStringCenter.Text = Utility.Language["List"];
                    modeStringRight.Text = Utility.Language["Score"];
                    break;
            }
        }

        class SongNameComponent : GameComponent
        {
            PictureObject back;
            PictureObject folder;
            PictureObject ac;
            PictureObject ft;
            PictureObject perfect;
            EffectObject anim;
            TextureString text;
            public const int ItemHeight = 28;
            public const int ItemWidth = 500;

            public string Name
            {
                get
                {
                    return text.Text;
                }
                set
                {
                    text.Text = value;
                    text.AllowScroll |= anim != null;
                }
            }

            public float ItemAlpha
            {
                get
                {
                    return text.Alpha;
                }
                set
                {
                    text.Alpha = value;
                    if (back != null)
                    {
                        back.Alpha = value;
                    }
                }
            }

            public float PerfectAlpha
            {
                get
                {
                    return perfect.Alpha;
                }
                set
                {
                    perfect.Alpha = value;
                }
            }

            public bool ShowFolder
            {
                get
                {
                    return !folder.Hidden;
                }
                set
                {
                    folder.Hidden = !value;
                }
            }

            public bool ShowAC
            {
                get
                {
                    return !ac.Hidden;
                }
                set
                {
                    ac.Hidden = !value;
                }
            }

            public bool ShowFT
            {
                get
                {
                    return !ft.Hidden;
                }
                set
                {
                    ft.Hidden = !value;
                }
            }

            public SongNameComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, bool isSelected) : base(device)
            {
                AddChild(folder = new PictureObject(device, resourceManager, Utility.Path.Combine("folder.png")));
                AddChild(ac = new PictureObject(device, resourceManager, Utility.Path.Combine("ac.png"))
                {
                    Position = new Vector2(30, -10)
                });
                AddChild(ft = new PictureObject(device, resourceManager, Utility.Path.Combine("ft.png"))
                {
                    Position = new Vector2(70, -10)
                });
                AddChild(perfect = new PictureObject(device, resourceManager, Utility.Path.Combine("perfect.png"))
                {
                    Position = new Vector2(110, -10)
                });
                AddChild(text = new TextureString(device, "", 20, 390, PPDColors.White)
                {
                    Position = new Vector2(40, 5)
                });
                if (isSelected)
                {
                    anim = new EffectObject(device, resourceManager, Utility.Path.Combine("selectback.etd"))
                    {
                        PlayType = Effect2D.EffectManager.PlayType.Loop,
                        Alignment = EffectObject.EffectAlignment.TopLeft
                    };
                    anim.Play();
                    AddChild(anim);
                }
                else
                {
                    back = new PictureObject(device, resourceManager, Utility.Path.Combine("selectback.png"));
                    AddChild(back);
                }
            }
        }
    }
}
