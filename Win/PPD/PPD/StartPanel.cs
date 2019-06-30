using PPDFramework;
using PPDShareComponent;
using SharpDX.DirectInput;
using System;

namespace PPD
{
    class StartPanel : FocusableGameComponent
    {
        public event EventHandler SettingFinished;
        PPDFramework.Resource.ResourceManager resourceManager;

        SpriteObject contentSprite;
        FadableButton right;
        FadableButton left;

        MyGame myGame;

        TextureString controllerNameText;
        FadableButton reload;
        FadableButton up;
        FadableButton down;

        FocusManager focusManager;

        TextButton clickedTextButton;

        KeyConfig keyConfig = new KeyConfig();

        TextureString[] keys;
        TextureString[] buttons;

        int currentPageIndex;

        public StartPanel(PPDDevice device, MyGame myGame, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            this.myGame = myGame;
            this.resourceManager = resourceManager;

            contentSprite = new SpriteObject(device);
            this.AddChild(contentSprite);

            var sprite = new SpriteObject(device);
            sprite.AddChild(new TextureString(device, Utility.Language["WelcomeToPPD"], 20, true, PPDColors.White)
            {
                Position = new SharpDX.Vector2(400, 200)
            });
            sprite.AddChild(new TextureString(device, Utility.Language["StartInitialSetting"], 16, true, PPDColors.White)
            {
                Position = new SharpDX.Vector2(400, 240)
            });
            Add(sprite);

            sprite = new SpriteObject(device);
            sprite.AddChild(new TextureString(device, Utility.Language["StartKeyButtonSetting"], 20, true, PPDColors.White)
            {
                Position = new SharpDX.Vector2(400, 200)
            });
            sprite.AddChild(new TextureString(device, Utility.Language["InsertKeyboardAndController"], 16, true, PPDColors.White)
            {
                Position = new SharpDX.Vector2(400, 240)
            });
            Add(sprite);

            sprite = new SpriteObject(device);
            sprite.AddChild((controllerNameText = new TextureString(device, "", 14, true, PPDColors.White)
            {
                Position = new SharpDX.Vector2(400, 10)
            }));
            reload = new FadableButton(device, resourceManager, Utility.Path.Combine("reload.png"), Utility.Path.Combine("reload_select.png"), "");
            reload.MouseLeftClick += reload_MouseLeftClick;
            up = new FadableButton(device, resourceManager, Utility.Path.Combine("up.png"), Utility.Path.Combine("up_select.png"), "");
            up.MouseLeftClick += up_MouseLeftClick;
            down = new FadableButton(device, resourceManager, Utility.Path.Combine("down.png"), Utility.Path.Combine("down_select.png"), "");
            down.MouseLeftClick += down_MouseLeftClick;
            sprite.AddChild(up);
            sprite.AddChild(down);
            sprite.AddChild(reload);
            up.Position = down.Position = reload.Position = new SharpDX.Vector2(400, 225);
            sprite.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("keysettinggrid.png"))
            {
                Position = new SharpDX.Vector2(290, 30)
            });
            sprite.AddChild(new TextButton(device, resourceManager) { Position = new SharpDX.Vector2(320, 390), Text = Utility.Language["Start"] });
            sprite.AddChild(new TextButton(device, resourceManager) { Position = new SharpDX.Vector2(320, 420), Text = Utility.Language["Home"] });
            keys = new TextureString[ButtonUtility.Array.Length];
            buttons = new TextureString[ButtonUtility.Array.Length];
            if (myGame.KeyConfigManager.Configs.Length > 0)
            {
                keyConfig = myGame.KeyConfigManager[0];
            }
            for (int i = 0; i < ButtonUtility.Array.Length; i++)
            {
                var textButton = new TextButton(device, resourceManager) { Position = new SharpDX.Vector2(530, 90 + i * 30), Text = Utility.Language["Change"], Index = i };
                textButton.MouseLeftDown += textButton_MouseLeftDown;

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
            Add(sprite);

            sprite = new SpriteObject(device);
            sprite.AddChild(new TextureString(device, Utility.Language["SettingCompleted"], 20, true, PPDColors.White)
            {
                Position = new SharpDX.Vector2(400, 200)
            });
            sprite.AddChild(new TextureString(device, Utility.Language["UserKeyboardOrController"], 16, true, PPDColors.White)
            {
                Position = new SharpDX.Vector2(400, 240)
            });
            Add(sprite);

            right = new FadableButton(device, resourceManager, Utility.Path.Combine("right.png"), Utility.Path.Combine("right_select.png"), "Next") { Position = new SharpDX.Vector2(770, 225) };
            this.AddChild(right);
            left = new FadableButton(device, resourceManager, Utility.Path.Combine("left.png"), Utility.Path.Combine("left_select.png"), "Back") { Position = new SharpDX.Vector2(30, 225) };
            this.AddChild(left);

            left.MouseLeftClick += left_MouseLeftClick;
            right.MouseLeftClick += right_MouseLeftClick;

            focusManager = new FocusManager();
            focusManager.Focus(this);

            left.Alpha = 0;
            this.Alpha = 0;
        }

        void textButton_MouseLeftDown(GameComponent sender, MouseEvent mouseEvent)
        {
            myGame.Input.AssignMode = true;
            if (clickedTextButton != null)
            {
                clickedTextButton.Text = Utility.Language["Change"];
            }
            clickedTextButton = sender as TextButton;
            clickedTextButton.Text = Utility.Language["Waiting"];
        }

        void down_MouseLeftClick(GameComponent sender, MouseEvent mouseEvent)
        {
            if (myGame != null)
            {
                if (myGame.Input.JoyStickCount > 0)
                {
                    int newIndex = myGame.Input.CurrentJoyStickIndex + 1;
                    if (newIndex >= myGame.Input.JoyStickCount)
                    {
                        newIndex = 0;
                    }
                    myGame.Input.CurrentJoyStickIndex = newIndex;
                    UpdateContollerDisplay();
                }
            }
        }

        void up_MouseLeftClick(GameComponent sender, MouseEvent mouseEvent)
        {
            if (myGame != null)
            {
                if (myGame.Input.JoyStickCount > 0)
                {
                    int newIndex = myGame.Input.CurrentJoyStickIndex - 1;
                    if (newIndex < 0)
                    {
                        newIndex = myGame.Input.JoyStickCount - 1;
                    }
                    myGame.Input.CurrentJoyStickIndex = newIndex;
                    UpdateContollerDisplay();
                }
            }
        }

        void reload_MouseLeftClick(GameComponent sender, MouseEvent mouseEvent)
        {
            if (myGame != null)
            {
                ReloadInput();
            }
        }

        void right_MouseLeftClick(GameComponent sender, MouseEvent mouseEvent)
        {
            if (currentPageIndex < contentSprite.ChildrenCount - 1)
            {
                OnCurrentPageIndexChange(currentPageIndex + 1);
            }
            else
            {
                if (SettingFinished != null)
                {
                    SettingFinished.Invoke(this, EventArgs.Empty);
                }
                AcceptMouseOperation = false;
            }
        }

        void left_MouseLeftClick(GameComponent sender, MouseEvent mouseEvent)
        {
            if (currentPageIndex > 0)
            {
                OnCurrentPageIndexChange(currentPageIndex - 1);
            }
        }

        private void OnCurrentPageIndexChange(int newIndex)
        {
            if (currentPageIndex < newIndex)
            {
                switch (newIndex)
                {
                    case 2:
                        ReloadInput();
                        break;
                }
            }

            currentPageIndex = newIndex;
            foreach (GameComponent gc in contentSprite.Children)
            {
                gc.AcceptMouseOperation = false;
            }
            contentSprite[currentPageIndex].AcceptMouseOperation = true;
        }

        private void ReloadInput()
        {
            if (myGame != null)
            {
                myGame.Input.Load();
                UpdateContollerDisplay();
            }
        }

        private void UpdateContollerDisplay()
        {
            if (myGame != null)
            {
                if (myGame.Input.JoyStickCount > 0)
                {
                    controllerNameText.Text = String.Format(Utility.Language["CurrentController"], myGame.Input.JoyStickNames[myGame.Input.CurrentJoyStickIndex]);
                }
                else
                {
                    controllerNameText.Text = Utility.Language["NoController"];
                }
                up.Position = new SharpDX.Vector2(controllerNameText.Position.X + controllerNameText.Width / 2 + 10, controllerNameText.Position.Y);
                down.Position = new SharpDX.Vector2(controllerNameText.Position.X + controllerNameText.Width / 2 + 10, controllerNameText.Position.Y + 15);
                reload.Position = new SharpDX.Vector2(controllerNameText.Position.X + controllerNameText.Width / 2 + 30, controllerNameText.Position.Y + controllerNameText.Height / 2);
            }
        }

        private void Add(GameComponent gc)
        {
            gc.Position = new SharpDX.Vector2(contentSprite.ChildrenCount * 800, 0);
            contentSprite.AddChild(gc);
        }

        protected override void UpdateImpl()
        {
            this.Alpha = AnimationUtility.IncreaseAlpha(this.Alpha);
            contentSprite.Position = new SharpDX.Vector2(AnimationUtility.GetAnimationValue(contentSprite.Position.X, -currentPageIndex * 800), 0);

            left.Alpha = currentPageIndex != 0 ? AnimationUtility.IncreaseAlpha(left.Alpha) : AnimationUtility.DecreaseAlpha(left.Alpha);

            if (clickedTextButton != null)
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
                    clickedTextButton.Text = Utility.Language["Change"];
                    clickedTextButton = null;
                    myGame.Input.AssignMode = false;
                }
            }
        }
    }

    class TextButton : GameComponent
    {
        TextureString textString;
        public TextButton(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            this.AddChild((textString = new TextureString(device, "", 12, true, PPDColors.White)));
            this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("keysettingbutton.png"), true));
            textString.Position = new SharpDX.Vector2(0, -textString.Height / 2);
        }

        public string Text
        {
            get
            {
                return textString.Text;
            }
            set
            {
                textString.Text = value;
            }
        }

        public int Index
        {
            get;
            set;
        }

        public override bool HitTest(SharpDX.Vector2 vec)
        {
            return base.HitTest(vec + new SharpDX.Vector2(this[1].Width / 2, this[1].Height / 2));
        }
    }
}
