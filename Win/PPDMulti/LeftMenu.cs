using PPDFramework;
using PPDFramework.Web;
using PPDFrameworkCore;
using PPDMulti.Data;
using PPDMulti.Model;
using PPDMultiCommon.Model;
using PPDShareComponent;
using SharpDX;
using System;

namespace PPDMulti
{
    class LeftMenu : FocusableGameComponent
    {
        enum Mode
        {
            SongSelect = 0,
            GameRule,
            UserManage,
            ItemSetting,
            TryToPlayGame,
            MuteSE,
            Connect,
            ShowResult,
            UpdateDB,
            Mod,
            Finish,
        }

        public event EventHandler SongSelected;
        public event EventHandler Closed;
        public event EventHandler ShowResult;
        public event EventHandler UpdateScoreDB;
        public event Action<GameRule> RuleChanged;
        public event EventHandler TryToPlayGame;
        public event Action<User> ChangeLeader;
        public event Action<User> KickUser;

        IGameHost gameHost;
        PPDFramework.Resource.ResourceManager resourceManager;
        ISound sound;
        MovieManager movieManager;
        ChangableList<User> users;
        AllowedModList allowedModList;

        PictureObject back;
        EffectObject select;
        Mode mode = Mode.SongSelect;
        const int selectX = 15;
        const int selectDiffY = 10;

        SongSelectComponent songSelectComponent;
        GameRuleComponent gameRuleComponent;
        ItemSettingComponent itemSettingComponent;
        UserSelectComponent userSelectComponent;
        ModPanel modPanel;
        SpriteObject leftSprite;
        SpriteObject songSelectSprite;
        SpriteObject userSelectSprite;
        SpriteObject itemSettingSprite;
        SpriteObject gameRuleSprite;
        SpriteObject modSprite;

        User user;
        bool[] enables;

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

        public bool MuteSE
        {
            get;
            private set;
        }

        public bool Connect
        {
            get;
            private set;
        }

        public SongInfo[] CommonSongs
        {
            get;
            set;
        }

        public LeftMenu(PPDDevice device, IGameHost gameHost, PPDFramework.Resource.ResourceManager resourceManager,
            ISound sound, MovieManager movieManager, User user, ChangableList<User> users, AllowedModList allowedModList) : base(device)
        {
            this.gameHost = gameHost;
            this.resourceManager = resourceManager;
            this.sound = sound;
            this.movieManager = movieManager;
            this.user = user;
            this.users = users;
            this.allowedModList = allowedModList;

            this.AddChild(songSelectSprite = new SpriteObject(device));
            this.AddChild(gameRuleSprite = new SpriteObject(device));
            this.AddChild(userSelectSprite = new SpriteObject(device));
            this.AddChild(itemSettingSprite = new SpriteObject(device));
            this.AddChild(modSprite = new SpriteObject(device));
            this.AddChild(leftSprite = new SpriteObject(device));
            leftSprite.AddChild(back = new PictureObject(device, resourceManager, Utility.Path.Combine("leftmenu.png")));

            enables = new bool[]{
                false,
                false,
                false,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true
            };

            user.PropertyChanged += (name) =>
            {
                if (name == "IsLeader")
                {
                    enables[0] = enables[1] = enables[2] = user.IsLeader;
                    if (!user.IsLeader)
                    {
                        if (mode >= Mode.SongSelect && mode <= Mode.UserManage)
                        {
                            mode = Mode.ItemSetting;
                        }
                    }
                    (back[0] as TextureString).Color = GetTextColor(0);
                    (back[1] as TextureString).Color = GetTextColor(1);
                    (back[2] as TextureString).Color = GetTextColor(2);
                }
            };

            back.AddChild(new TextureString(device, Utility.Language["ChangeScore"], 16, GetTextColor())
            {
                Position = new Vector2(30, 70)
            });
            back.AddChild(new TextureString(device, Utility.Language["ChangeRule"], 16, GetTextColor())
            {
                Position = new Vector2(30, 100)
            });
            back.AddChild(new TextureString(device, Utility.Language["PlayerManager"], 16, GetTextColor())
            {
                Position = new Vector2(30, 130)
            });
            back.AddChild(new TextureString(device, Utility.Language["ItemUseSetting"], 16, GetTextColor())
            {
                Position = new Vector2(30, 160)
            });
            back.AddChild(new TextureString(device, Utility.Language["TryToPlayGame"], 16, GetTextColor())
            {
                Position = new Vector2(30, 190)
            });
            back.AddChild(new TextureString(device, String.Format("{0}:{1}", Utility.Language["MuteSE"], Utility.Language["OFF"]), 16, GetTextColor())
            {
                Position = new Vector2(30, 220)
            });
            back.AddChild(new TextureString(device, String.Format("{0}:{1}", Utility.Language["Connect"], Utility.Language["OFF"]), 16, GetTextColor())
            {
                Position = new Vector2(30, 250)
            });
            back.AddChild(new TextureString(device, Utility.Language["Result"], 16, GetTextColor())
            {
                Position = new Vector2(30, 280)
            });
            back.AddChild(new TextureString(device, Utility.Language["UpdateDB"], 16, GetTextColor())
            {
                Position = new Vector2(30, 310)
            });
            back.AddChild(new TextureString(device, Utility.Language["Mod"], 16, GetTextColor())
            {
                Position = new Vector2(30, 340)
            });
            back.AddChild(new TextureString(device, user.IsHost ? Utility.Language["FinishHost"] : Utility.Language["LeaveRoom"], 16, GetTextColor())
            {
                Position = new Vector2(30, 370)
            });
            Connect = SkinSetting.Setting.Connect;
            (back[(int)Mode.Connect] as TextureString).Text = String.Format("{0}:{1}", Utility.Language["Connect"], Connect ? Utility.Language["ON"] : Utility.Language["OFF"]);

            mode = Mode.ItemSetting;

            select = new EffectObject(device, resourceManager, Utility.Path.Combine("greenflare.etd"))
            {
                Position = new Vector2(15, back[(int)mode].Position.Y + selectDiffY)
            };
            select.PlayType = Effect2D.EffectManager.PlayType.ReverseLoop;
            select.Play();
            select.Alignment = EffectObject.EffectAlignment.Center;
            select.Scale = new Vector2(0.4f, 0.4f);

            back.AddChild(select);

            leftSprite.Position = new Vector2(-back.Width, 0);

            Inputed += LeftMenu_Inputed;
        }

        private Color4 GetTextColor()
        {
            return GetTextColor(back.ChildrenCount);
        }

        private Color4 GetTextColor(int index)
        {
            return enables[index] ? PPDColors.White : PPDColors.Gray;
        }

        void LeftMenu_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                sound.Play(PPDSetting.DefaultSounds[2], -1000);
                FocusManager.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Triangle))
            {
                sound.Play(PPDSetting.DefaultSounds[2], -1000);
                FocusManager.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                switch (mode)
                {
                    case Mode.SongSelect:
                        if (songSelectComponent == null)
                        {
                            songSelectComponent = new SongSelectComponent(device, resourceManager, sound, this, movieManager);
                            songSelectComponent.SongSelected += songSelectComponent_SongSelected;
                            songSelectSprite.InsertChild(songSelectComponent, 0);
                        }
                        sound.Play(PPDSetting.DefaultSounds[1], -1000);
                        FocusManager.Focus(songSelectComponent);
                        break;
                    case Mode.GameRule:
                        if (gameRuleComponent == null)
                        {
                            gameRuleComponent = new GameRuleComponent(device, resourceManager, sound);
                            gameRuleComponent.RuleChanged += gameRuleComponent_RuleChanged;
                            gameRuleSprite.InsertChild(gameRuleComponent, 0);
                        }
                        sound.Play(PPDSetting.DefaultSounds[1], -1000);
                        FocusManager.Focus(gameRuleComponent);
                        break;
                    case Mode.UserManage:
                        if (userSelectComponent == null)
                        {
                            userSelectComponent = new UserSelectComponent(device, resourceManager, sound, users);
                            userSelectComponent.ChangeLeader += userSelectComponent_ChangeLeader;
                            userSelectComponent.KickUser += userSelectComponent_KickUser;
                            userSelectSprite.InsertChild(userSelectComponent, 0);
                        }
                        sound.Play(PPDSetting.DefaultSounds[1], -1000);
                        FocusManager.Focus(userSelectComponent);
                        break;
                    case Mode.ItemSetting:
                        if (itemSettingComponent == null)
                        {
                            itemSettingComponent = new ItemSettingComponent(device, resourceManager, sound);
                            itemSettingSprite.InsertChild(itemSettingComponent, 0);
                        }
                        sound.Play(PPDSetting.DefaultSounds[1], -1000);
                        FocusManager.Focus(itemSettingComponent);
                        break;
                    case Mode.TryToPlayGame:
                        sound.Play(PPDSetting.DefaultSounds[1], -1000);
                        FocusManager.RemoveFocus();
                        OnTryToPlayGame();
                        break;
                    case Mode.MuteSE:
                        sound.Play(PPDSetting.DefaultSounds[3], -1000);
                        MuteSE = !MuteSE;
                        (back[(int)mode] as TextureString).Text = String.Format("{0}:{1}", Utility.Language["MuteSE"], MuteSE ? Utility.Language["ON"] : Utility.Language["OFF"]);
                        break;
                    case Mode.Connect:
                        sound.Play(PPDSetting.DefaultSounds[3], -1000);
                        Connect = !Connect;
                        (back[(int)mode] as TextureString).Text = String.Format("{0}:{1}", Utility.Language["Connect"], Connect ? Utility.Language["ON"] : Utility.Language["OFF"]);
                        SkinSetting.Setting.Connect = Connect;
                        break;
                    case Mode.ShowResult:
                        sound.Play(PPDSetting.DefaultSounds[1], -1000);
                        if (ShowResult != null)
                        {
                            ShowResult.Invoke(this, EventArgs.Empty);
                        }
                        break;
                    case Mode.UpdateDB:
                        sound.Play(PPDSetting.DefaultSounds[1], -1000);
                        if (UpdateScoreDB != null)
                        {
                            UpdateScoreDB.Invoke(this, EventArgs.Empty);
                        }
                        break;
                    case Mode.Mod:
                        if (modPanel == null)
                        {
                            modPanel = new ModPanel(device, gameHost, resourceManager, sound, allowedModList);
                            modSprite.InsertChild(modPanel, 0);
                        }
                        sound.Play(PPDSetting.DefaultSounds[1], -1000);
                        FocusManager.Focus(modPanel);
                        break;
                    case Mode.Finish:
                        sound.Play(PPDSetting.DefaultSounds[1], -1000);
                        var confirmComponent = new ConfirmComponent(device, resourceManager, Utility.Path, Utility.Language["FinishConfirm"], Utility.Language["Yes"], Utility.Language["No"], Utility.Language["OK"], ConfirmComponent.ConfirmButtonType.YesNo);
                        confirmComponent.LostFocused += confirmComponent_LostFocused;
                        this.InsertChild(confirmComponent, 0);
                        FocusManager.Focus(confirmComponent);
                        break;
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                mode--;
                while (true)
                {
                    if (mode < 0)
                    {
                        mode = (Mode)(enables.Length - 1);
                    }

                    if (enables[(int)mode])
                    {
                        break;
                    }
                    mode--;
                }
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                mode++;
                while (true)
                {
                    if ((int)mode >= enables.Length)
                    {
                        mode = Mode.SongSelect;
                    }

                    if (enables[(int)mode])
                    {
                        break;
                    }
                    mode++;
                }
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }

            select.Position = new Vector2(select.Position.X, selectDiffY + back[(int)mode].Position.Y);
        }

        void userSelectComponent_KickUser(object sender, EventArgs e)
        {
            KickUser?.Invoke(userSelectComponent.SelectedUser);
        }

        void userSelectComponent_ChangeLeader(object sender, EventArgs e)
        {
            ChangeLeader?.Invoke(userSelectComponent.SelectedUser);
        }

        void gameRuleComponent_RuleChanged(object sender, EventArgs e)
        {
            if (RuleChanged != null)
            {
                RuleChanged.Invoke(gameRuleComponent.GameRule);
            }
        }

        void songSelectComponent_SongSelected(object sender, EventArgs e)
        {
            if (SongSelected != null)
            {
                SongInformation = songSelectComponent.SongInformation;
                Difficulty = songSelectComponent.Difficulty;
                SongSelected.Invoke(this, EventArgs.Empty);
            }
        }

        void confirmComponent_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            if (sender is ConfirmComponent confirmComponent)
            {
                if (confirmComponent.OK)
                {
                    OnClose();
                }
                this.RemoveChild(confirmComponent);
            }
        }

        private void OnClose()
        {
            if (Closed != null)
            {
                Closed.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnTryToPlayGame()
        {
            TryToPlayGame?.Invoke(this, EventArgs.Empty);
        }

        protected override void UpdateImpl()
        {
            if (OverFocused)
            {
                leftSprite.Position = new Vector2(AnimationUtility.GetAnimationValue(leftSprite.Position.X, 0, 0.3f), 0);
            }
            else
            {
                leftSprite.Position = new Vector2(AnimationUtility.GetAnimationValue(leftSprite.Position.X, -back.Width, 0.3f), 0);
            }
        }
    }
}
