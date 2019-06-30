using PPDFramework;
using PPDFramework.Mod;
using PPDShareComponent;
using SharpDX;
using System;
using System.Windows.Interop;

namespace PPDSingle
{
    class ModSettingPanel : FocusableGameComponent
    {
        const int ClipY = 80;
        const int ClipHeight = 330;
        const int SpriteY = 80;

        IGameHost gameHost;
        ISound sound;
        PPDFramework.Resource.ResourceManager resourceManager;

        int currentIndex;
        ModInfo modInfo;

        RectangleComponent black;
        PictureObject back;
        SpriteObject settingSprite;
        SpriteObject settingListSprite;
        LineRectangleComponent rectangle;

        private ModSettingComponent CurrentComponent
        {
            get
            {
                return settingListSprite[currentIndex] as ModSettingComponent;
            }
        }

        public ModSettingPanel(PPDDevice device, IGameHost gameHost, PPDFramework.Resource.ResourceManager resourceManager, ISound sound, ModInfo modInfo) : base(device)
        {
            this.gameHost = gameHost;
            this.sound = sound;
            this.resourceManager = resourceManager;
            this.modInfo = modInfo;

            back = new PictureObject(device, resourceManager, Utility.Path.Combine("dialog_back.png"));
            black = new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0.65f
            };
            back.AddChild(new TextureString(device, String.Format("{0}-{1}", Utility.Language["ModSetting"], modInfo.FileName), 30, PPDColors.White)
            {
                Position = new Vector2(35, 30)
            });
            StackObject stackObject;
            back.AddChild(stackObject = new StackObject(device,
                new StackObject(device,
                        new SpaceObject(device, 0, 2),
                        new PictureObject(device, resourceManager, Utility.Path.Combine("circle.png")))
                {
                    IsHorizontal = false
                },
                    new TextureString(device, String.Format(":{0}", Utility.Language["ChangeSetting"]), 18, PPDColors.White))
            {
                IsHorizontal = true
            });
            stackObject.Update();
            stackObject.Position = new Vector2(760 - stackObject.Width, 50);

            settingSprite = new SpriteObject(device)
            {
                Position = new Vector2(50, SpriteY)
            };
            settingSprite.AddChild(settingListSprite = new SpriteObject(device)
            {
                Clip = new ClipInfo(gameHost)
                {
                    PositionX = 40,
                    PositionY = ClipY,
                    Width = 750,
                    Height = ClipHeight
                }
            });

            float height = 0;
            foreach (ModSetting modSetting in modInfo.Settings)
            {
                var component = new ModSettingComponent(device, resourceManager, modSetting,
                    modSetting.GetStringValue(modInfo.ModSettingManager[modSetting.Key]))
                {
                    Position = new Vector2(0, height)
                };
                component.Update();
                settingListSprite.AddChild(component);
                height += component.Height + 10;
            }
            CurrentComponent.IsSelected = true;

            settingSprite.AddChild(rectangle = new LineRectangleComponent(device, resourceManager, PPDColors.Selection)
            {
                RectangleWidth = 700,
                RectangleHeight = 100
            });

            this.AddChild(settingSprite);
            this.AddChild(back);
            this.AddChild(black);

            UpdateBorderPosition(true);

            Inputed += ModSettingPanel_Inputed;
        }

        void ModSettingPanel_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                FocusManager.RemoveFocus();
                sound.Play(PPDSetting.DefaultSounds[2], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                var window = new PPDModSettingUI.ModSettingWindow();
                var helper = new WindowInteropHelper(window)
                {
                    Owner = gameHost.WindowHandle
                };
                ModSetting setting = modInfo.Settings[currentIndex];
                var model = new PPDModSettingUI.ModSettingModel(setting, setting.GetStringValue(modInfo.ModSettingManager[setting.Key]), Utility.Language);
                window.DataContext = model;
                var result = window.ShowDialog();
                if (result == true)
                {
                    if (setting.Validate(model.NewValue, out object val))
                    {
                        modInfo.ModSettingManager[setting.Key] = val;
                        modInfo.SaveSetting();
                        CurrentComponent.CurrentValue = setting.GetStringValue(val);
                    }
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                CurrentComponent.IsSelected = false;
                currentIndex++;
                if (currentIndex >= settingListSprite.ChildrenCount)
                {
                    currentIndex = settingListSprite.ChildrenCount - 1;
                }
                CurrentComponent.IsSelected = true;
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                CurrentComponent.IsSelected = false;
                currentIndex--;
                if (currentIndex < 0)
                {
                    currentIndex = 0;
                }
                CurrentComponent.IsSelected = true;
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
        }

        private void UpdateBorderPosition(bool finishAnimation)
        {
            rectangle.RectangleWidth = 700;
            var temp = CurrentComponent.Position;
            var targetHeight = CurrentComponent.Height + 10;
            if (finishAnimation)
            {
                rectangle.RectangleHeight = targetHeight;
                rectangle.Position = temp;
            }
            else
            {
                rectangle.RectangleHeight = AnimationUtility.GetAnimationValue(rectangle.RectangleHeight, targetHeight);
                rectangle.Position = new Vector2(temp.X, AnimationUtility.GetAnimationValue(rectangle.Position.Y, temp.Y));
            }
        }

        private void UpdateScroll()
        {
            var temp = CurrentComponent.ScreenPos;
            var target = 0f;
            if (temp.Y < ClipY && CurrentComponent.Height < ClipHeight)
            {
                target = ClipY - temp.Y;
            }
            else if (temp.Y + CurrentComponent.Height + 10 > ClipY + ClipHeight)
            {
                target = ClipY + ClipHeight - (temp.Y + CurrentComponent.Height + 10);
            }
            settingSprite.Position = new Vector2(settingSprite.Position.X,
                AnimationUtility.GetAnimationValue(settingSprite.Position.Y, settingSprite.Position.Y + target));
        }

        protected override bool OnCanUpdate()
        {
            return !Hidden && OverFocused;
        }

        protected override void UpdateImpl()
        {
            base.UpdateImpl();
            UpdateBorderPosition(false);
            UpdateScroll();
        }

        protected override bool OnCanDraw(PPDFramework.Shaders.AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return OverFocused;
        }

        class ModSettingComponent : GameComponent
        {
            bool isSelected;
            TextureString nameText;
            TextureString descriptionText;
            TextureString valueText;
            TextureString infoText;

            public string CurrentValue
            {
                set
                {
                    valueText.Text = value;
                }
            }

            public bool IsSelected
            {
                get { return isSelected; }
                set
                {
                    isSelected = value;
                    nameText.AllowScroll = isSelected;
                }
            }

            public ModSettingComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager,
                ModSetting modSetting, string currentValue) : base(device)
            {
                this.AddChild(nameText = new TextureString(device, modSetting.Name, 24, 500, PPDColors.White)
                {
                    AllowScroll = false
                });
                this.AddChild(infoText = new TextureString(device, GetInfoText(modSetting), 12, PPDColors.White)
                {
                    Position = new Vector2(690, 30),
                    Alignment = Alignment.Right
                });
                this.AddChild(descriptionText = new TextureString(device, modSetting.Description, 18, 660, int.MaxValue, true, PPDColors.White)
                {
                    Position = new Vector2(15, 40)
                });
                this.AddChild(valueText = new TextureString(device, currentValue, 24, 400, PPDColors.White)
                {
                    Position = new Vector2(690, 0),
                    Alignment = Alignment.Right
                });
            }

            private String GetInfoText(ModSetting modSetting)
            {
                return String.Format("{0}:{1}", Utility.Language["DefaultValue"], modSetting.GetStringValue(modSetting.Default));
            }
        }
    }
}
