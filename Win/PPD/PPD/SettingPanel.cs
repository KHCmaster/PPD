using PPDFramework;
using PPDFramework.Web;
using PPDShareComponent;
using SharpDX.DirectInput;
using System;

namespace PPD
{
    class SettingPanel : HomePanelBase
    {
        PPDFramework.Resource.ResourceManager resourceManager;
        MyGame myGame;
        ISound sound;

        int currentIndex;
        SpriteObject allSlideSprite;
        SlideSprite firstSprite;

        public SettingPanel(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, MyGame myGame, ISound sound) : base(device)
        {
            this.resourceManager = resourceManager;
            this.myGame = myGame;
            this.sound = sound;
        }

        public override void Load()
        {
            OnLoadProgressed(0);
            this.AddChild((allSlideSprite = new SpriteObject(device)));
            OnLoadProgressed(10);
            firstSprite = new SlideSprite(device, resourceManager, sound) { FirstSlide = true };
            OnLoadProgressed(40);
            firstSprite.AddSelection(Utility.Language["KeyAndButtonSetting"]);
            OnLoadProgressed(60);
            firstSprite.AddSelection(Utility.Language["AccountSetting"]);
            firstSprite.AddSelection(Utility.Language["VolumeSetting"]);
            allSlideSprite.AddChild(firstSprite);
            OnLoadProgressed(90);

            firstSprite.Selected += slideSprite_Selected;
            GotFocused += SettingPanel_GotFocused;
            OnLoadProgressed(100);
        }

        void SettingPanel_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            if (args.FocusObject != firstSprite)
            {
                allSlideSprite.Position = new SharpDX.Vector2(0, 0);
            }
        }

        void slideSprite_Selected(object sender, EventArgs e)
        {
            if (!Focused && !OverFocused)
            {
                return;
            }

            var firstSprite = sender as SlideSprite;
            currentIndex++;
            RemoveNext();
            switch (firstSprite.CurrentSelection)
            {
                case 0:
                    var keyButtonSprite = new SlideSprite(device, resourceManager, sound) { Position = new SharpDX.Vector2(800 * currentIndex, 0) };
                    keyButtonSprite.Selected += keyButtonSprite_Selected;
                    keyButtonSprite.LostFocused += slideSprite_LostFocused;
                    keyButtonSprite.AddSelection(Utility.Language["AssingKeyButton"]);
                    keyButtonSprite.AddSelection(Utility.Language["ChangeCurrentController"]);
                    keyButtonSprite.AddSelection(Utility.Language["ChangeCurrentSetting"]);
                    allSlideSprite.AddChild(keyButtonSprite);
                    FocusManager.Focus(keyButtonSprite);
                    break;
                case 1:
                    var accountSprite = new SlideSprite(device, resourceManager, sound) { Position = new SharpDX.Vector2(800 * currentIndex, 0) };
                    accountSprite.Selected += accountSprite_Selected;
                    accountSprite.LostFocused += slideSprite_LostFocused;
                    accountSprite.AddSelection(WebManager.Instance.IsLogined ? Utility.Language["Logout"] : Utility.Language["Login"]);
                    allSlideSprite.AddChild(accountSprite);
                    FocusManager.Focus(accountSprite);
                    break;
                case 2:
                    var changeVolumeSprite = new ChangeVolumeSprite(device, resourceManager, myGame, sound) { Position = new SharpDX.Vector2(800 * currentIndex, 0) };
                    changeVolumeSprite.LostFocused += slideSprite_LostFocused;
                    allSlideSprite.AddChild(changeVolumeSprite);
                    FocusManager.Focus(changeVolumeSprite);
                    break;
            }
        }

        void accountSprite_Selected(object sender, EventArgs e)
        {
            if (!Focused && !OverFocused)
            {
                return;
            }

            if (WebManager.Instance.IsLogined)
            {
                var accountSprite = sender as SlideSprite;
                WebManager.Instance.Logout();
                PPDGeneralSetting.Setting.Username = "";
                PPDGeneralSetting.Setting.Password = "";
            }
            else
            {
                myGame.ShowLoginForm();
            }
            ReturnToFirst();
        }

        void slideSprite_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            if (sender is FocusableGameComponent slide && !slide.OverFocused)
            {
                currentIndex--;
            }
        }

        void keyButtonSprite_Selected(object sender, EventArgs e)
        {
            if (!Focused && !OverFocused)
            {
                return;
            }

            currentIndex++;
            RemoveNext();

            var keyButtonSprite = sender as SlideSprite;
            switch (keyButtonSprite.CurrentSelection)
            {
                case 0:
                    var changeKeyConfigSprite = new ChangeKeyConfigSprite(device, resourceManager, myGame, myGame.KeyConfigManager.CurrentConfig, sound)
                    {
                        Position = new SharpDX.Vector2(800 * currentIndex, 0)
                    };
                    changeKeyConfigSprite.LostFocused += slideSprite_LostFocused;

                    allSlideSprite.AddChild(changeKeyConfigSprite);
                    FocusManager.Focus(changeKeyConfigSprite);
                    break;
                case 1:
                    var changeControllerSprite = new SlideSprite(device, resourceManager, sound) { Position = new SharpDX.Vector2(800 * currentIndex, 0) };
                    changeControllerSprite.Selected += changeControllerSprite_Selected;
                    changeControllerSprite.LostFocused += slideSprite_LostFocused;
                    myGame.Input.Load();

                    for (int i = 0; i < myGame.Input.JoyStickCount; i++)
                    {
                        changeControllerSprite.AddSelection(myGame.Input.JoyStickNames[i]);
                    }

                    if (myGame.Input.JoyStickCount == 0)
                    {
                        changeControllerSprite.AddChild(new TextureString(device, Utility.Language["NoController"], 18, true, PPDColors.White)
                        {
                            Position = new SharpDX.Vector2(400, 220)
                        });
                    }
                    else
                    {
                        changeControllerSprite.CurrentSelection = myGame.Input.CurrentJoyStickIndex;
                    }

                    allSlideSprite.AddChild(changeControllerSprite);
                    FocusManager.Focus(changeControllerSprite);
                    break;
                case 2:
                    var changeProfileSprite = new SlideSprite(device, resourceManager, sound) { Position = new SharpDX.Vector2(800 * currentIndex, 0) };
                    changeProfileSprite.Selected += changeProfileSprite_Selected;
                    changeProfileSprite.LostFocused += slideSprite_LostFocused;

                    for (int i = 0; i < myGame.KeyConfigManager.Configs.Length; i++)
                    {
                        changeProfileSprite.AddSelection(myGame.KeyConfigManager.Configs[i].Name);
                    }
                    changeProfileSprite.CurrentSelection = myGame.KeyConfigManager.CurrentConfigIndex;

                    allSlideSprite.AddChild(changeProfileSprite);
                    FocusManager.Focus(changeProfileSprite);

                    break;
            }
        }

        void changeProfileSprite_Selected(object sender, EventArgs e)
        {
            if (!Focused && !OverFocused)
            {
                return;
            }

            var changeProfileSprite = sender as SlideSprite;
            if (changeProfileSprite.CurrentSelection >= 0 && changeProfileSprite.CurrentSelection < myGame.KeyConfigManager.Configs.Length)
            {
                myGame.KeyConfigManager.CurrentConfigIndex = changeProfileSprite.CurrentSelection;
                if (myGame.Input.CurrentJoyStickIndex >= 0)
                {
                    PPDGeneralSetting.Setting[String.Format("{0}_config", myGame.Input.JoyStickNames[myGame.Input.CurrentJoyStickIndex])] = myGame.KeyConfigManager.CurrentConfig.Name;
                }
            }

            ReturnToFirst();
        }

        void changeControllerSprite_Selected(object sender, EventArgs e)
        {
            if (!Focused && !OverFocused)
            {
                return;
            }

            var changeControllerSprite = sender as SlideSprite;
            if (changeControllerSprite.CurrentSelection >= 0 && changeControllerSprite.CurrentSelection < myGame.Input.JoyStickCount)
            {
                myGame.Input.CurrentJoyStickIndex = changeControllerSprite.CurrentSelection;
                string configName = PPDGeneralSetting.Setting[String.Format("{0}_config", myGame.Input.JoyStickNames[myGame.Input.CurrentJoyStickIndex])];
                myGame.KeyConfigManager.ChangeSettingFromName(configName);
            }

            ReturnToFirst();
        }

        private void RemoveNext()
        {
            while (allSlideSprite.ChildrenCount > currentIndex)
            {
                allSlideSprite.RemoveChild(allSlideSprite[allSlideSprite.ChildrenCount - 1]);
            }
        }

        private void ReturnToFirst()
        {
            while (FocusManager.CurrentFocusObject != firstSprite)
            {
                FocusManager.RemoveFocus();
            }
            currentIndex = 0;
        }

        protected override void UpdateImpl()
        {
            if (currentIndex == 0 && FocusManager != null && FocusManager.CurrentFocusObject == this)
            {
                FocusManager.Focus(firstSprite);
            }

            if (Focused || OverFocused)
            {
                allSlideSprite.Position = new SharpDX.Vector2(AnimationUtility.GetAnimationValue(allSlideSprite.Position.X, -currentIndex * 800), allSlideSprite.Position.Y);
            }
        }

        class ChangeKeyConfigSprite : FocusableGameComponent
        {
            PPDFramework.Resource.ResourceManager resourceManager;
            MyGame myGame;
            ISound sound;

            TextButton[] textButtons;
            TextureString[] keys;
            TextureString[] buttons;

            int currentIndex;
            TextButton clickedTextButton;
            KeyConfig keyConfig;
            bool gotFocused;

            PictureObject select;

            public ChangeKeyConfigSprite(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, MyGame myGame, KeyConfig keyConfig, ISound sound) : base(device)
            {
                this.resourceManager = resourceManager;
                this.myGame = myGame;
                this.keyConfig = keyConfig;
                this.sound = sound;

                var sprite = new SpriteObject(device);

                sprite.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("keysettinggrid.png"))
                {
                    Position = new SharpDX.Vector2(290, 30)
                });
                sprite.AddChild(new TextButton(device, resourceManager) { Position = new SharpDX.Vector2(320, 390), Text = Utility.Language["Start"] });
                sprite.AddChild(new TextButton(device, resourceManager) { Position = new SharpDX.Vector2(320, 420), Text = Utility.Language["Home"] });
                keys = new TextureString[ButtonUtility.Array.Length];
                buttons = new TextureString[ButtonUtility.Array.Length];
                textButtons = new TextButton[ButtonUtility.Array.Length];
                if (myGame.KeyConfigManager.CurrentConfigIndex < myGame.KeyConfigManager.Configs.Length)
                {
                    keyConfig = myGame.KeyConfigManager.Configs[myGame.KeyConfigManager.CurrentConfigIndex];
                }
                for (int i = 0; i < ButtonUtility.Array.Length; i++)
                {
                    var textButton = new TextButton(device, resourceManager) { Position = new SharpDX.Vector2(530, 90 + i * 30), Text = Utility.Language["Change"], Index = i };
                    textButton.MouseLeftDown += textButton_MouseLeftDown;
                    textButtons[i] = textButton;

                    keys[i] = new TextureString(device, keyConfig.GetKeyMap((ButtonType)i).ToString(), 14, true, PPDColors.White)
                    {
                        Position = new SharpDX.Vector2(388, 82 + i * 30)
                    };
                    buttons[i] = new TextureString(device, keyConfig.GetButtonMap((ButtonType)i).ToString(), 14, true, PPDColors.White)
                    {
                        Position = new SharpDX.Vector2(460, 82 + i * 30)
                    };
                    sprite.AddChild(textButton);
                    sprite.AddChild(keys[i]);
                    sprite.AddChild(buttons[i]);
                }

                this.AddChild(sprite);
                this.AddChild((select = new PictureObject(device, resourceManager, Utility.Path.Combine("right.png"), true)
                {
                    Position = new SharpDX.Vector2(280, 0),
                    Scale = new SharpDX.Vector2(0.5f, 0.5f)
                }));

                Inputed += ChangeKeyConfigSprite_Inputed;
                LostFocused += ChangeKeyConfigSprite_LostFocused;
                GotFocused += ChangeKeyConfigSprite_GotFocused;
            }

            void ChangeKeyConfigSprite_GotFocused(IFocusable sender, FocusEventArgs args)
            {
                gotFocused = true;
            }

            void ChangeKeyConfigSprite_LostFocused(IFocusable sender, FocusEventArgs args)
            {
                DisableAssignMode();
                myGame.KeyConfigManager.Write("keyconfig.ini");
            }

            void ChangeKeyConfigSprite_Inputed(IFocusable sender, InputEventArgs args)
            {
                if (args.InputInfo.IsPressed(ButtonType.Up))
                {
                    currentIndex--;
                    if (currentIndex < 0)
                    {
                        currentIndex = ButtonUtility.Array.Length - 1;
                    }
                }
                else if (args.InputInfo.IsPressed(ButtonType.Down))
                {
                    currentIndex++;
                    if (currentIndex >= ButtonUtility.Array.Length)
                    {
                        currentIndex = 0;
                    }
                }
                else if (args.InputInfo.IsPressed(ButtonType.Circle))
                {
                    if (gotFocused)
                    {
                        gotFocused = false;
                    }
                    else
                    {
                        textButton_MouseLeftDown(textButtons[currentIndex], new MouseEvent(SharpDX.Vector2.Zero, false, MouseEvent.MouseEventType.Left));
                    }
                }
                else if (args.InputInfo.IsPressed(ButtonType.Cross))
                {
                    if (FocusManager != null)
                    {
                        FocusManager.RemoveFocus();
                        sound.Play(PPDSetting.DefaultSounds[2], -1000);
                    }
                }
            }

            void textButton_MouseLeftDown(GameComponent sender, MouseEvent mouseEvent)
            {
                if (clickedTextButton != null)
                {
                    clickedTextButton.Text = Utility.Language["Change"];
                }
                clickedTextButton = sender as TextButton;
                clickedTextButton.Text = Utility.Language["Waiting"];
                currentIndex = clickedTextButton.Index;
            }

            protected override void UpdateImpl()
            {
                if (myGame.Input.AssignMode)
                {
                    bool found = false;
                    if (myGame.Input.GetPressedButton(out int button))
                    {
                        keyConfig.SetButtonMap((ButtonType)clickedTextButton.Index, button);
                        buttons[clickedTextButton.Index].Text = button.ToString();
                        found = true;
                    }
                    if (myGame.Input.GetPressedKey(out Key key))
                    {
                        keyConfig.SetKeyMap((ButtonType)clickedTextButton.Index, key);
                        keys[clickedTextButton.Index].Text = key.ToString();
                        found = true;
                    }

                    if (found)
                    {
                        DisableAssignMode();
                    }
                }

                if (clickedTextButton != null && !myGame.Input.AssignMode)
                {
                    myGame.Input.AssignMode = true;
                }

                select.Position = new SharpDX.Vector2(select.Position.X, AnimationUtility.GetAnimationValue(select.Position.Y, currentIndex * 30 + 90, 0.2f));
            }

            private void DisableAssignMode()
            {
                if (clickedTextButton != null)
                {
                    clickedTextButton.Text = Utility.Language["Change"];
                    clickedTextButton = null;
                }
                myGame.Input.AssignMode = false;
            }
        }
    }
}
