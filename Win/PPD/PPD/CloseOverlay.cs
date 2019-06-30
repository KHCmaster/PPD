using PPDFramework;
using PPDFramework.Scene;
using PPDShareComponent;
using SharpDX;
using System;

namespace PPD
{
    class CloseOverlay : SceneBase
    {
        int currentIndex;
        SpriteObject allSlideSprite;
        SlideSprite firstSprite;

        MyGame myGame;

        FocusManager focusManager;

        public CloseOverlay(PPDDevice device) : base(device)
        {
            Alpha = 1;
        }

        public override bool Load()
        {
            myGame = GameHost as MyGame;

            this.AddChild((allSlideSprite = new SpriteObject(device)));

            firstSprite = new SlideSprite(device, ResourceManager, Sound) { FirstSlide = true };

            firstSprite.AddSelection(Utility.Language["Exit"]);
            firstSprite.AddSelection(Utility.Language["Back"]);
            firstSprite.AddSelection(Utility.Language["KeyAndButtonSetting"]);
            firstSprite.AddSelection(Utility.Language["VolumeSetting"]);
            firstSprite.AddSelection(Utility.Language["GoHome"]);

            allSlideSprite.AddChild(firstSprite);

            focusManager = new FocusManager();
            focusManager.Focus(firstSprite);

            firstSprite.Selected += slideSprite_Selected;
            this.MouseRightClick += CloseOverlay_MouseRightClick;

            this.AddChild(new RectangleComponent(device, ResourceManager, PPDColors.Black)
            {
                Alpha = 0.65f,
                RectangleHeight = 450,
                RectangleWidth = 800
            });
            return true;
        }

        void slideSprite_Selected(object sender, EventArgs e)
        {
            var slideSprite = sender as SlideSprite;
            switch (slideSprite.CurrentSelection)
            {
                case 0:
                    GameHost.Exit();
                    break;
                case 1:
                    GameHost.CancelExit();
                    break;
                case 2:
                    currentIndex++;
                    RemoveNext();

                    var keyButtonSprite = new SlideSprite(device, ResourceManager, Sound) { Position = new SharpDX.Vector2(800 * currentIndex, 0) };
                    keyButtonSprite.Selected += keyButtonSprite_Selected;
                    keyButtonSprite.LostFocused += slideSprite_LostFocused;
                    keyButtonSprite.AddSelection(Utility.Language["ChangeCurrentController"]);
                    keyButtonSprite.AddSelection(Utility.Language["ChangeCurrentSetting"]);
                    allSlideSprite.AddChild(keyButtonSprite);
                    focusManager.Focus(keyButtonSprite);
                    break;
                case 3:
                    currentIndex++;
                    RemoveNext();

                    var changeVolumeSprite = new ChangeVolumeSprite(device, ResourceManager, myGame, Sound) { Position = new SharpDX.Vector2(800 * currentIndex, 0) };
                    changeVolumeSprite.LostFocused += slideSprite_LostFocused;
                    allSlideSprite.AddChild(changeVolumeSprite);
                    focusManager.Focus(changeVolumeSprite);
                    break;
                case 4:
                    GameHost.GoHome();
                    break;
            }
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
            currentIndex++;
            RemoveNext();

            var keyButtonSprite = sender as SlideSprite;
            switch (keyButtonSprite.CurrentSelection)
            {
                case 0:
                    var changeControllerSprite = new SlideSprite(device, ResourceManager, Sound) { Position = new SharpDX.Vector2(800 * currentIndex, 0) };
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
                            Position = new Vector2(400, 220)
                        });
                    }
                    else
                    {
                        changeControllerSprite.CurrentSelection = myGame.Input.CurrentJoyStickIndex;
                    }

                    allSlideSprite.AddChild(changeControllerSprite);
                    focusManager.Focus(changeControllerSprite);
                    break;
                case 1:
                    var changeProfileSprite = new SlideSprite(device, ResourceManager, Sound) { Position = new SharpDX.Vector2(800 * currentIndex, 0) };
                    changeProfileSprite.Selected += changeProfileSprite_Selected;
                    changeProfileSprite.LostFocused += slideSprite_LostFocused;

                    for (int i = 0; i < myGame.KeyConfigManager.Configs.Length; i++)
                    {
                        changeProfileSprite.AddSelection(myGame.KeyConfigManager.Configs[i].Name);
                    }
                    changeProfileSprite.CurrentSelection = myGame.KeyConfigManager.CurrentConfigIndex;

                    allSlideSprite.AddChild(changeProfileSprite);
                    focusManager.Focus(changeProfileSprite);

                    break;
            }
        }

        void changeProfileSprite_Selected(object sender, EventArgs e)
        {
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
            while (focusManager.CurrentFocusObject != firstSprite)
            {
                focusManager.RemoveFocus();
            }
            currentIndex = 0;
        }

        void CloseOverlay_MouseRightClick(GameComponent sender, MouseEvent mouseEvent)
        {
            if (currentIndex == 0)
            {
                GameHost.CancelExit();
            }
            else
            {
                focusManager.RemoveFocus();
            }
        }

        public void Update(bool focus, InputInfoBase inputInfo, MouseInfo mouseInfo)
        {
            inputInfo = new MenuInputInfo(inputInfo);

            if (focus)
            {
                currentIndex = 0;
                allSlideSprite.Position = new SharpDX.Vector2(0, 0);
                firstSprite.ChangeMenuEnabled(4, GameHost.CanGoHome);
                firstSprite.CurrentSelection = 0;
            }

            allSlideSprite.Position = new SharpDX.Vector2(AnimationUtility.GetAnimationValue(allSlideSprite.Position.X, -currentIndex * 800), allSlideSprite.Position.Y);


            focusManager.ProcessInput(inputInfo);
            UpdateMouseInfo(mouseInfo);

            Update();
        }

        public override float Width
        {
            get
            {
                return device.Width;
            }
        }

        public override float Height
        {
            get
            {
                return device.Height;
            }
        }
    }
}
