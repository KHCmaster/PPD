using PPDFramework;
using PPDFramework.Mod;
using PPDFramework.Shaders;
using PPDFramework.Web;
using PPDFrameworkCore;
using PPDShareComponent;
using SharpDX;
using System;
using System.Linq;

namespace PPDMulti
{
    class ModPanel : FocusableGameComponent
    {
        const int MaxDisplayCount = 11;
        const int scrollBarStartY = 80;
        const int scrollBarMaxHeight = 330;

        IGameHost gameHost;
        ISound sound;
        PPDFramework.Resource.ResourceManager resourceManager;
        AllowedModList allowedModList;

        RectangleComponent black;
        SpriteObject waitSprite;
        SpriteObject updateSprite;
        PictureObject back;
        ModInfoBase currentModInfo;

        SpriteObject settingMenu;
        SpriteObject updateMenu;
        TextureString author;
        TextureString version;
        TextureString filename;
        TextureString notAvailable;
        RectangleComponent scrollBar;
        SpriteObject modListSprite;

        int currentIndex = -1;
        int scrollIndex;

        private bool initialized;
        private bool initializeFinished;
        private bool updating;
        private bool updateFinished;

        public ModInfoComponentBase CurrentComponent
        {
            get
            {
                return modListSprite[currentIndex] as ModInfoComponentBase;
            }
        }

        public bool Initialized
        {
            get
            {
                return initialized;
            }
        }

        public ModPanel(PPDDevice device, IGameHost gameHost, PPDFramework.Resource.ResourceManager resourceManager, ISound sound, AllowedModList allowedModList) : base(device)
        {
            this.gameHost = gameHost;
            this.sound = sound;
            this.resourceManager = resourceManager;
            this.allowedModList = allowedModList;

            waitSprite = new SpriteObject(device);
            this.AddChild(waitSprite);
            waitSprite.AddChild(new TextureString(device, Utility.Language["InitializingMod"], 20, true, PPDColors.White)
            {
                Position = new Vector2(400, 220)
            });
            waitSprite.AddChild(new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0.65f
            });

            updateSprite = new SpriteObject(device)
            {
                Hidden = true
            };
            this.AddChild(updateSprite);
            updateSprite.AddChild(new TextureString(device, Utility.Language["UpdatingMod"], 20, true, PPDColors.White)
            {
                Position = new Vector2(400, 220)
            });
            updateSprite.AddChild(new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0.65f
            });

            back = new PictureObject(device, resourceManager, Utility.Path.Combine("dialog_back.png"));
            black = new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0.65f
            };
            back.AddChild(new TextureString(device, Utility.Language["Mod"], 30, PPDColors.White)
            {
                Position = new Vector2(35, 30)
            });
            var stackObject = new StackObject(device,
                updateMenu = new StackObject(device,
                    new StackObject(device,
                        new SpaceObject(device, 0, 2),
                        new PictureObject(device, resourceManager, Utility.Path.Combine("square.png")))
                    {
                        IsHorizontal = false
                    },
                        new TextureString(device, String.Format(":{0}", Utility.Language["Update"]), 18, PPDColors.White))
                {
                    IsHorizontal = true
                },
                new SpaceObject(device, 20, 0),
                settingMenu = new StackObject(device,
                    new StackObject(device,
                        new SpaceObject(device, 0, 2),
                        new PictureObject(device, resourceManager, Utility.Path.Combine("delta.png")))
                    {
                        IsHorizontal = false
                    },
                    new TextureString(device, String.Format(":{0}", Utility.Language["Setting"]), 18, PPDColors.White))
                {
                    IsHorizontal = true
                },
                new SpaceObject(device, 20, 0),
                new StackObject(device,
                    new SpaceObject(device, 0, 2),
                    new PictureObject(device, resourceManager, Utility.Path.Combine("checkgreen.png")))
                {
                    Position = new Vector2(0, 50),
                    IsHorizontal = false
                },
                new TextureString(device, String.Format(":{0}", Utility.Language["Available"]), 18, PPDColors.White)
                {
                    Position = new Vector2(35, 50)
                })
            {
                IsHorizontal = true
            };
            back.AddChild(stackObject);
            stackObject.Update();
            stackObject.Position = new Vector2(760 - stackObject.Width, 50);

            var sprite = new SpriteObject(device)
            {
                Position = new Vector2(50, 80)
            };

            modListSprite = new SpriteObject(device);
            sprite.AddChild(modListSprite);
            sprite.AddChild(new TextureString(device, String.Format("{0}:", Utility.Language["Author"]), 20, PPDColors.White)
            {
                Position = new Vector2(425, 0)
            });
            sprite.AddChild(author = new TextureString(device, "", 20, PPDColors.White)
            {
                Position = new Vector2(450, 30)
            });
            sprite.AddChild(new TextureString(device, String.Format("{0}:", Utility.Language["Version"]), 20, PPDColors.White)
            {
                Position = new Vector2(425, 60)
            });
            sprite.AddChild(version = new TextureString(device, "", 20, PPDColors.White)
            {
                Position = new Vector2(450, 90)
            });
            sprite.AddChild(new TextureString(device, String.Format("{0}:", Utility.Language["Filename"]), 20, PPDColors.White)
            {
                Position = new Vector2(425, 120)
            });
            sprite.AddChild(filename = new TextureString(device, "", 20, 240, PPDColors.White)
            {
                Position = new Vector2(450, 150)
            });
            sprite.AddChild(notAvailable = new TextureString(device, Utility.Language["ModWarning"], 20, 280, 280, true, PPDColors.Red)
            {
                Position = new Vector2(425, 180),
                Hidden = true
            });

            this.AddChild(scrollBar = new RectangleComponent(device, resourceManager, PPDColors.White)
            {
                Position = new Vector2(456, 80),
                RectangleHeight = 330,
                RectangleWidth = 5
            });
            this.AddChild(new RectangleComponent(device, resourceManager, PPDColors.White)
            {
                Position = new Vector2(465, 80),
                RectangleHeight = 330,
                RectangleWidth = 1
            });
            this.AddChild(sprite);
            this.AddChild(back);
            this.AddChild(black);

            Inputed += ModPanel_Inputed;
            GotFocused += ModPanel_GotFocused;
        }

        void ModPanel_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            if (!initialized)
            {
                ThreadManager.Instance.GetThread(() =>
                {
                    ModManager.Instance.WaitForLoadFinish();
                    initializeFinished = true;
                }).Start();
            }
        }

        private void LoadMod()
        {
            var modManager = ModManager.Instance;
        }

        private void Initialize()
        {
            initializeFinished = false;
            currentModInfo = ModManager.Instance.Root;
            Reload();
            waitSprite.Hidden = true;
            initialized = true;
        }

        private void Reload(int selectIndex = -1)
        {
            int iter = 0;
            modListSprite.ClearChildren();
            foreach (var modInfo in currentModInfo.Children)
            {
                var pos = new Vector2(0, iter * 30);
                var hidden = iter >= MaxDisplayCount;
                if (modInfo.IsDir)
                {
                    modListSprite.AddChild(new DirModInfoComponent(device, resourceManager, (DirModInfo)modInfo)
                    {
                        Position = new Vector2(0, iter * 30),
                        Hidden = iter >= MaxDisplayCount
                    });
                }
                else
                {
                    modListSprite.AddChild(new ModInfoComponent(device, resourceManager, (ModInfo)modInfo, allowedModList.IsAllowed(((ModInfo)modInfo).FileHashString))
                    {
                        Position = new Vector2(0, iter * 30),
                        Hidden = iter >= MaxDisplayCount
                    });
                }
                iter++;
            }

            if (selectIndex < 0)
            {
                if (currentModInfo.Children.Length != 0)
                {
                    currentIndex = 0;
                }
            }
            else
            {
                currentIndex = selectIndex;
            }

            if (currentModInfo.Children.Length != 0)
            {
                CurrentComponent.Selected = true;
                AdjustScrollBar();
                ChangeModInfo();
            }
        }

        private void AfterUpdate()
        {
            CurrentComponent.UpdateInfo();
            ChangeModInfo();
            updateFinished = false;
            updating = false;
            updateSprite.Hidden = true;
        }

        private void AdjustScrollBar()
        {
            if (currentIndex < scrollIndex)
            {
                scrollIndex = currentIndex;
            }
            else if (scrollIndex + MaxDisplayCount - 1 <= currentIndex)
            {
                scrollIndex += currentIndex - scrollIndex - MaxDisplayCount + 1;
            }

            for (int i = 0; i < modListSprite.ChildrenCount; i++)
            {
                modListSprite[i].Position = new Vector2(0, 30 * (i - scrollIndex));
                modListSprite[i].Hidden = i < scrollIndex || scrollIndex + MaxDisplayCount <= i;
            }

            scrollBar.RectangleHeight = modListSprite.ChildrenCount <= MaxDisplayCount ? scrollBarMaxHeight : scrollBarMaxHeight * MaxDisplayCount / modListSprite.ChildrenCount;
            scrollBar.Position = new Vector2(scrollBar.Position.X, modListSprite.ChildrenCount <= MaxDisplayCount ? scrollBarStartY : scrollBarStartY + scrollBarMaxHeight * scrollIndex / modListSprite.ChildrenCount);
        }

        void ModPanel_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (!initialized || updating)
            {
                return;
            }

            if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                if (currentModInfo == ModManager.Instance.Root)
                {
                    FocusManager.RemoveFocus();
                }
                else
                {
                    var selectIndex = Array.IndexOf(currentModInfo.Parent.Children, currentModInfo);
                    currentModInfo = currentModInfo.Parent;
                    Reload(selectIndex);
                }
                sound.Play(PPDSetting.DefaultSounds[2], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                if (currentIndex >= 0)
                {
                    if (CurrentComponent.ModInfoBase.IsDir)
                    {
                        currentModInfo = CurrentComponent.ModInfoBase;
                        Reload(0);
                        sound.Play(PPDSetting.DefaultSounds[1], -1000);
                    }
                    else
                    {
                        var modComponent = (ModInfoComponent)CurrentComponent;
                        if (modComponent.CanApply)
                        {
                            modComponent.ModInfo.IsApplied = !modComponent.ModInfo.IsApplied;
                            sound.Play(PPDSetting.DefaultSounds[3], -1000);
                        }
                    }
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Triangle))
            {
                if (currentIndex >= 0 && !CurrentComponent.ModInfoBase.IsDir)
                {
                    var modComponent = (ModInfoComponent)CurrentComponent;
                    if (modComponent.ModInfo.Settings.Length > 0)
                    {
                        var modSettingPanel = new ModSettingPanel(device, gameHost, resourceManager, sound, modComponent.ModInfo);
                        modSettingPanel.LostFocused += (s, e) =>
                        {
                            this.RemoveChild(modSettingPanel);
                        };
                        FocusManager.Focus(modSettingPanel);
                        sound.Play(PPDSetting.DefaultSounds[1], -1000);
                        this.InsertChild(modSettingPanel, 0);
                    }
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Square))
            {
                if (currentIndex >= 0 && !CurrentComponent.ModInfoBase.IsDir)
                {
                    var modComponent = (ModInfoComponent)CurrentComponent;
                    if (modComponent.ModInfo.CanUpdate)
                    {
                        updating = true;
                        updateSprite.Hidden = false;
                        modComponent.ModInfo.UpdateFinished += ModInfo_UpdateFinished;
                        ThreadManager.Instance.GetThread(() => modComponent.ModInfo.Update()).Start();
                    }
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                if (currentIndex >= 0)
                {
                    CurrentComponent.Selected = false;
                    currentIndex++;
                    if (currentIndex >= currentModInfo.Children.Length)
                    {
                        currentIndex = 0;
                    }
                    CurrentComponent.Selected = true;
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                    AdjustScrollBar();
                    ChangeModInfo();
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                if (currentIndex >= 0)
                {
                    CurrentComponent.Selected = false;
                    currentIndex--;
                    if (currentIndex < 0)
                    {
                        currentIndex = currentModInfo.Children.Length - 1;
                    }
                    CurrentComponent.Selected = true;
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                    AdjustScrollBar();
                    ChangeModInfo();
                }
            }
        }

        void ModInfo_UpdateFinished()
        {
            ((ModInfoComponent)CurrentComponent).ModInfo.UpdateFinished -= ModInfo_UpdateFinished;
            updateFinished = true;
        }

        private void ChangeModInfo()
        {
            if (CurrentComponent is ModInfoComponent modComponent)
            {
                author.Text = modComponent.ModInfo.AuthorName;
                version.Text = modComponent.ModInfo.Version;
                filename.Text = modComponent.ModInfo.FileName;
                filename.AllowScroll = true;
                if (modComponent.ModInfo.ContainsModifyData)
                {
                    notAvailable.Text = Utility.Language["ModWarning2"];
                }
                else
                {
                    notAvailable.Text = Utility.Language["ModWarning"];
                }
                author.Hidden = version.Hidden = filename.Hidden = false;
                notAvailable.Hidden = modComponent.CanApply;
                settingMenu.Hidden = modComponent.ModInfo.Settings.Length == 0;
                updateMenu.Hidden = !modComponent.ModInfo.CanUpdate;
            }
            else
            {
                author.Hidden = version.Hidden = filename.Hidden = notAvailable.Hidden =
                    settingMenu.Hidden = updateMenu.Hidden = true;
            }
        }

        protected override void UpdateImpl()
        {
            if (initializeFinished)
            {
                Initialize();
            }
            if (updateFinished)
            {
                AfterUpdate();
            }
        }

        protected override bool OnCanUpdate()
        {
            return !Hidden && OverFocused;
        }

        protected override bool OnCanDraw(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return !Hidden && OverFocused;
        }

        public class ModInfoComponentBase : GameComponent
        {
            protected TextureString text;
            private bool selected;

            public bool Selected
            {
                get { return selected; }
                set
                {
                    if (selected != value)
                    {
                        selected = value;
                        text.AllowScroll = selected;
                    }
                }
            }

            public PPDFramework.Mod.ModInfoBase ModInfoBase
            {
                get;
                private set;
            }

            public ModInfoComponentBase(PPDDevice device, PPDFramework.Mod.ModInfoBase modInfo) : base(device)
            {
                ModInfoBase = modInfo;
            }

            public virtual void UpdateInfo()
            {
            }
        }


        public class ModInfoComponent : ModInfoComponentBase
        {
            private bool allowedByServer;

            private PictureObject check;
            private PictureObject checkBox;
            private PictureObject availableRanking;

            public ModInfo ModInfo
            {
                get { return ModInfoBase as ModInfo; }
            }

            public bool CanApply
            {
                get
                {
                    return ModInfo.CanApply();
                }
            }

            public ModInfoComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ModInfo modInfo, bool allowedByServer)
                : base(device, modInfo)
            {
                this.allowedByServer = allowedByServer;
                this.AddChild(check = new PictureObject(device, resourceManager, Utility.Path.Combine("optioncheck.png"))
                {
                    Position = new Vector2(0, -5),
                    Scale = new Vector2(0.5f),
                    Hidden = !modInfo.IsApplied
                });
                this.AddChild(checkBox = new PictureObject(device, resourceManager, Utility.Path.Combine("optioncheckbox.png"))
                {
                    Position = new Vector2(7, 7),
                    Scale = new Vector2(0.5f),
                    Alpha = 0
                });
                this.AddChild(text = new TextureString(device, modInfo.DisplayName, 20, 320, PPDColors.White)
                {
                    Position = new Vector2(30, 0)
                });
                this.AddChild(availableRanking = new PictureObject(device, resourceManager, Utility.Path.Combine("checkgreen.png"))
                {
                    Position = new Vector2(0, 2)
                });
                UpdateInfo();
            }

            public override void UpdateInfo()
            {
                text.Text = ModInfo.DisplayName;
                text.Color = CanApply ? PPDColors.White : PPDColors.Gray;
                text.Update();
                availableRanking.Position = new Vector2(30 + text.JustWidth, availableRanking.Position.Y);
                availableRanking.Hidden = !allowedByServer && ModInfo.ContainsModifyData;
            }

            protected override void UpdateImpl()
            {
                check.Hidden = !ModInfo.IsApplied;
                checkBox.Alpha = Selected ? AnimationUtility.IncreaseAlpha(checkBox.Alpha) : AnimationUtility.DecreaseAlpha(checkBox.Alpha);
            }
        }

        public class DirModInfoComponent : ModInfoComponentBase
        {
            private PictureObject folder;

            public DirModInfo ModInfo
            {
                get { return ModInfoBase as DirModInfo; }
            }

            public DirModInfoComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, DirModInfo modInfo)
                : base(device, modInfo)
            {
                this.AddChild(folder = new PictureObject(device, resourceManager, Utility.Path.Combine("folder.png"))
                {
                    Scale = new Vector2(0.8f),
                    Alpha = 0.5f
                });
                this.AddChild(text = new TextureString(device, modInfo.DisplayName, 20, 320, PPDColors.White)
                {
                    Position = new Vector2(30, 0)
                });
                UpdateInfo();
            }

            public override void UpdateInfo()
            {
                text.Text = ModInfo.DisplayName;
                text.Update();
            }

            protected override void UpdateImpl()
            {
                folder.Alpha = Selected ? AnimationUtility.IncreaseAlpha(folder.Alpha) : AnimationUtility.GetAnimationValue(folder.Alpha, 0.5f);
            }
        }
    }
}
