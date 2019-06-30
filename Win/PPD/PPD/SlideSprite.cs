using PPDFramework;
using PPDShareComponent;
using System;
using System.Collections.Generic;

namespace PPD
{
    class SlideSprite : FocusableGameComponent
    {
        public event EventHandler Selected;

        PPDFramework.Resource.ResourceManager resourceManager;
        ISound sound;

        SpriteObject textSprite;
        SpriteObject userSprite;
        PictureObject select;
        bool modified;
        List<bool> menuEnabled = new List<bool>();

        int currentIndex = -1;
        int selectCount;

        public SlideSprite(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound) : base(device)
        {
            this.resourceManager = resourceManager;
            this.sound = sound;

            this.AddChild((textSprite = new SpriteObject(device)));
            this.AddChild((userSprite = new SpriteObject(device)));
            this.AddChild((select = new PictureObject(device, resourceManager, Utility.Path.Combine("right.png"), true)
            {
                Position = new SharpDX.Vector2(30, 0),
                Scale = new SharpDX.Vector2(0.5f, 0.5f)
            }));

            Inputed += SlideSprite_Inputed;
        }

        public int CurrentSelection
        {
            get
            {
                return currentIndex;
            }
            set
            {
                currentIndex = value;
                modified = true;
            }
        }

        public bool FirstSlide
        {
            get;
            set;
        }

        void SlideSprite_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                if (selectCount > 0)
                {
                    currentIndex--;
                    if (currentIndex < 0)
                    {
                        currentIndex = textSprite.ChildrenCount - 1;
                    }
                    while (!menuEnabled[currentIndex])
                    {
                        currentIndex--;
                        if (currentIndex < 0)
                        {
                            currentIndex = textSprite.ChildrenCount - 1;
                        }
                    }
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                if (selectCount > 0)
                {
                    currentIndex++;
                    if (currentIndex >= textSprite.ChildrenCount)
                    {
                        currentIndex = 0;
                    }
                    while (!menuEnabled[currentIndex])
                    {
                        currentIndex++;
                        if (currentIndex >= textSprite.ChildrenCount)
                        {
                            currentIndex = 0;
                        }
                    }
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                if (selectCount > 0)
                {
                    if (Selected != null)
                    {
                        Selected.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                if (FocusManager != null && !FirstSlide)
                {
                    FocusManager.RemoveFocus();
                }
            }
        }

        public void AddSelection(string text)
        {
            var textureString = new TextureString(device, text, 18, PPDColors.White)
            {
                Position = new SharpDX.Vector2(400, 0)
            };
            textureString.MouseEnter += textureString_MouseEnter;
            textureString.MouseLeftClick += textureString_MouseLeftClick;
            textSprite.AddChild(textureString);
            modified = true;
            menuEnabled.Add(true);
            selectCount++;
            currentIndex = 0;
        }

        void textureString_MouseLeftClick(GameComponent sender, MouseEvent mouseEvent)
        {
            currentIndex = textSprite.IndexOf(sender);
            if (Selected != null)
            {
                Selected.Invoke(this, EventArgs.Empty);
            }
        }

        void textureString_MouseEnter(GameComponent sender, MouseEvent mouseEvent)
        {
            currentIndex = textSprite.IndexOf(sender);
        }

        public void AddUserComponent(GameComponent gc)
        {
            userSprite.AddChild(gc);
        }

        public void ChangeMenuEnabled(int index, bool enabled)
        {
            if (index >= 0 && index < menuEnabled.Count)
            {
                if (menuEnabled[index] != enabled)
                {
                    textSprite[index].AcceptMouseOperation = enabled;
                    menuEnabled[index] = enabled;
                    modified = true;
                }
            }
        }

        protected override void UpdateImpl()
        {
            if (modified)
            {
                for (int i = 0; i < textSprite.ChildrenCount; i++)
                {
                    textSprite[i].Position = new SharpDX.Vector2(400 - textSprite.Width / 2, 225 - (25) * textSprite.ChildrenCount / 2 + i * 25);
                    (textSprite[i] as TextureString).Alpha = menuEnabled[i] ? 1 : 0f;
                }

                select.Position = new SharpDX.Vector2(400 - textSprite.Width / 2 - 10, textSprite[currentIndex].Position.Y + 10);
                modified = false;
            }

            if (selectCount > 0)
            {
                select.Position = new SharpDX.Vector2(select.Position.X, AnimationUtility.GetAnimationValue(select.Position.Y, textSprite[currentIndex].Position.Y + 10, 0.2f));
            }
            select.Hidden = selectCount == 0;
        }
    }
}
