using PPDFramework;
using PPDFramework.Shaders;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PPDSingle
{
    class ContextMenu : FocusableGameComponent
    {
        public event EventHandler Selected;

        PictureObject back;

        ISound sound;
        SpriteObject<MenuItem> menuSprite;
        RectangleComponent rectangle;
        int currentIndex;

        public ContextMenu(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound) : base(device)
        {
            this.sound = sound;

            back = new PictureObject(device, resourceManager, Utility.Path.Combine("scoremanager", "menuback.png"))
            {
                Position = new Vector2(-10, -10)
            };

            Inputed += ContextMenu_Inputed;

            GotFocused += ContextMenu_GotFocused;
            this.AddChild(menuSprite = new SpriteObject<MenuItem>(device));
            this.AddChild(rectangle = new RectangleComponent(device, resourceManager, PPDColors.White));
            this.AddChild(back);
        }

        void ContextMenu_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            menuSprite[currentIndex].Selected &= currentIndex < 0;
            if (menuSprite.ChildrenCount > 0)
            {
                menuSprite[0].Selected = true;
                currentIndex = 0;
            }
            else
            {
                currentIndex = -1;
            }
            UpdateRectangle();
        }

        void ContextMenu_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                FocusManager.RemoveFocus();
                OnSelected();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                FocusManager.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Triangle))
            {
                FocusManager.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                if (currentIndex >= 0)
                {
                    menuSprite[currentIndex].Selected = false;
                    int iter = currentIndex - 1 < 0 ? menuSprite.ChildrenCount - 1 : currentIndex - 1;
                    while (iter != currentIndex)
                    {
                        if (menuSprite[iter].Enabled)
                        {
                            menuSprite[iter].Selected = true;
                            break;
                        }
                        iter--;
                        if (iter < 0) iter = menuSprite.ChildrenCount - 1;
                    }
                    currentIndex = iter;
                    menuSprite[currentIndex].Selected = true;
                    UpdateRectangle();
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                if (currentIndex >= 0)
                {
                    menuSprite[currentIndex].Selected = false;
                    int iter = currentIndex + 1 >= menuSprite.ChildrenCount ? 0 : currentIndex + 1;
                    while (iter != currentIndex)
                    {
                        if (menuSprite[iter].Enabled)
                        {
                            menuSprite[iter].Selected = true;
                            break;
                        }
                        iter++;
                        if (iter >= menuSprite.ChildrenCount) iter = 0;
                    }
                    currentIndex = iter;
                    menuSprite[currentIndex].Selected = true;
                    UpdateRectangle();
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
            }
        }

        protected void OnSelected()
        {
            if (Selected != null && currentIndex >= 0)
            {
                Selected.Invoke(menuSprite[currentIndex], EventArgs.Empty);
            }
        }

        public IEnumerable<MenuItem> Menus
        {
            get
            {
                return menuSprite.Children.OfType<MenuItem>();
            }
        }

        public void AddMenu(string menu)
        {
            AddMenu(new MenuItem(device, menu));
        }

        public void AddMenu(MenuItem menuItem)
        {
            menuSprite.AddChild(menuItem);
            if (menuSprite.ChildrenCount == 1)
            {
                menuItem.Selected = true;
            }
            else
            {
                menuItem.Selected = false;
            }
            menuItem.Position = new Vector2(0, (menuSprite.ChildrenCount - 1) * 22);
        }

        private void UpdateRectangle()
        {
            if (currentIndex >= 0)
            {
                rectangle.Position = menuSprite[currentIndex].Position;
                rectangle.RectangleWidth = menuSprite[currentIndex].Width;
                rectangle.RectangleHeight = menuSprite[currentIndex].Height + 2;
                rectangle.Hidden = false;
            }
            else
            {
                rectangle.Hidden = true;
            }
        }

        protected override bool OnCanUpdate()
        {
            return OverFocused;
        }

        protected override bool OnCanDraw(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return OverFocused;
        }

        public int CheckPositionX(Vector2 pos)
        {
            return 750 - (int)(pos.X + back.Width);
        }

        public int CheckPositionY(Vector2 pos)
        {
            return 420 - (int)(pos.Y + back.Height);
        }

        public override float Height
        {
            get
            {
                return back.Height;
            }
        }
    }

    class MenuItem : GameComponent
    {
        bool enabled;
        bool selected;
        TextureString text;

        public MenuItem(PPDDevice device, string name) : base(device)
        {
            this.AddChild(text = new TextureString(device, name, 18, PPDColors.White));
            Name = name;
            Enabled = true;
        }

        public string Name
        {
            get;
            set;
        }

        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    ChangeColor();
                }
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
                    ChangeColor();
                }
            }
        }

        private void ChangeColor()
        {
            if (Enabled)
            {
                if (Selected)
                {
                    text.Color = PPDColors.Black;
                }
                else
                {
                    text.Color = PPDColors.White;
                }
            }
            else
            {
                text.Color = PPDColors.Gray;
            }
        }
    }
}
