using PPDFramework;
using PPDFrameworkCore;
using PPDMulti.Data;
using PPDMulti.Model;
using PPDShareComponent;
using SharpDX;
using System;
using System.Linq;

namespace PPDMulti
{
    class UserSelectComponent : FocusableGameComponent
    {
        PPDFramework.Resource.ResourceManager resourceManager;
        ISound sound;
        PictureObject back;

        SpriteObject userSprite;
        int index = -1;

        public event EventHandler ChangeLeader;
        public event EventHandler KickUser;

        private UserComponent CurrentUser
        {
            get
            {
                if (index < 0 || index >= userSprite.ChildrenCount)
                {
                    return null;
                }
                return (UserComponent)userSprite.GetChildAt(index);
            }
        }

        public User SelectedUser
        {
            get
            {
                if (CurrentUser != null)
                {
                    return CurrentUser.User;
                }
                return null;
            }
        }

        public UserSelectComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound, ChangableList<User> users) : base(device)
        {
            this.resourceManager = resourceManager;
            this.sound = sound;
            this.AddChild(back = new PictureObject(device, resourceManager, Utility.Path.Combine("dialog_back.png")));
            back.AddChild(new TextureString(device, Utility.Language["PlayerManager"], 30, PPDColors.White)
            {
                Position = new Vector2(35, 30)
            });
            back.AddChild(userSprite = new SpriteObject(device) { Position = new Vector2(40, 80) });
            this.AddChild(new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0.75f
            });

            Inputed += ItemSettingComponent_Inputed;
            GotFocused += ItemSettingComponent_GotFocused;

            foreach (var user in users)
            {
                if (user.IsSelf)
                {
                    continue;
                }
                userSprite.AddChild(new UserComponent(device, resourceManager, user));
            }
            AdjustPosition();
            users.ItemChanged += users_ItemChanged;
        }

        void users_ItemChanged(User[] addedItems, User[] removedItems)
        {
            foreach (var user in addedItems)
            {
                if (user.IsSelf)
                {
                    continue;
                }
                userSprite.AddChild(new UserComponent(device, resourceManager, user));
            }
            foreach (var user in removedItems)
            {
                if (user.IsSelf)
                {
                    continue;
                }
                var found = userSprite.Children.FirstOrDefault(c => ((UserComponent)c).User == user);
                if (found != null)
                {
                    userSprite.RemoveChild(found);
                }
            }
            AdjustPosition();
        }

        private void AdjustPosition()
        {
            var iter = 0;
            foreach (var component in userSprite.Children)
            {
                var x = 220 * (iter % 2);
                var y = iter / 2 * 35;
                component.Position = new Vector2(x, y);
                iter++;
            }
            if (index < 0 && userSprite.ChildrenCount > 0)
            {
                index = 0;
                CurrentUser.IsSelected = true;
            }
        }

        void ItemSettingComponent_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            if (!(args.FocusObject is UserMenuDialog))
            {
                back.Position = new SharpDX.Vector2(0, 50);
                Alpha = 0;
            }
        }

        void ItemSettingComponent_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                sound.Play(PPDSetting.DefaultSounds[2], -1000);
                FocusManager.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                if (CurrentUser != null)
                {
                    var dialog = new UserMenuDialog(device, resourceManager);
                    dialog.LostFocused += dialog_LostFocused;
                    dialog.Position = CurrentUser.ScreenPos + new Vector2(20, 40);
                    if (dialog.Position.Y + dialog.Height >= 450)
                    {
                        dialog.Position = CurrentUser.ScreenPos + new Vector2(20, -dialog.Height);
                    }
                    this.InsertChild(dialog, 0);
                    FocusManager.Focus(dialog);
                }
                sound.Play(PPDSetting.DefaultSounds[1], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                ChangeSelectedUserIndex(-2);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                ChangeSelectedUserIndex(2);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Left))
            {
                ChangeSelectedUserIndex(index % 2 == 0 ? 1 : -1);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Right))
            {
                ChangeSelectedUserIndex(index % 2 == 0 ? 1 : -1);
            }
        }

        private void ChangeSelectedUserIndex(int offset)
        {
            if (CurrentUser != null)
            {
                CurrentUser.IsSelected = false;
            }
            index += offset;
            if (index < 0)
            {
                index = userSprite.ChildrenCount - 1;
            }
            if (index >= userSprite.ChildrenCount)
            {
                index = 0;
            }
            if (CurrentUser != null)
            {
                CurrentUser.IsSelected = true;
            }
            sound.Play(PPDSetting.DefaultSounds[0], -1000);
        }

        void dialog_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            var dialog = sender as UserMenuDialog;

            if (dialog.Selected)
            {
                sound.Play(PPDSetting.DefaultSounds[1], -1000);
                UserMenuDialog.Mode mode = dialog.SelectedMode;
                switch (mode)
                {
                    case UserMenuDialog.Mode.ChangeLeader:
                        ChangeLeader?.Invoke(this, EventArgs.Empty);
                        break;
                    case UserMenuDialog.Mode.Kick:
                        KickUser?.Invoke(this, EventArgs.Empty);
                        break;
                }
            }

            this.RemoveChild(dialog);
        }

        protected override void UpdateImpl()
        {
            if (OverFocused)
            {
                back.Position = new SharpDX.Vector2(0, AnimationUtility.GetAnimationValue(back.Position.Y, 0));
                Alpha = AnimationUtility.IncreaseAlpha(Alpha);
            }
            else
            {
                back.Position = new SharpDX.Vector2(0, AnimationUtility.GetAnimationValue(back.Position.Y, 50));
                Alpha = AnimationUtility.DecreaseAlpha(Alpha);
            }
        }

        class UserComponent : BindableGameComponent
        {
            PictureObject userIcon;
            TextureString userNameString;
            RectangleComponent rectangle;
            bool isSelected;

            PPDFramework.Resource.ResourceManager resourceManager;

            private string currentUserIconPath = Utility.Path.Combine("noimage_icon.png");

            public UserComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, User user) : base(device)
            {
                this.resourceManager = resourceManager;

                User = user;

                this.AddChild((userIcon = new PictureObject(device, resourceManager, PathObject.Absolute(currentUserIconPath), true)
                {
                    Position = new Vector2(21, 21)
                }));
                this.AddChild((userNameString = new TextureString(device, "", 18, 160, PPDColors.White)
                {
                    Position = new Vector2(42, 12)
                }));
                this.AddChild(rectangle = new RectangleComponent(device, resourceManager, PPDColors.White)
                {
                    Hidden = true,
                    Position = new Vector2(5, 5),
                    RectangleHeight = 30
                });

                AddBinding(new Binding(user, "Name", this, "UserName"));
                AddBinding(new Binding(user, "ImagePath", this, "UserImagePath"));
                ChangeUserIconScale();
            }

            public User User
            {
                get;
                private set;
            }

            public bool IsSelected
            {
                get { return isSelected; }
                set
                {
                    if (isSelected != value)
                    {
                        isSelected = value;
                        userNameString.Color = isSelected ? PPDColors.Black : PPDColors.White;
                        rectangle.Hidden = !isSelected;
                    }
                }
            }

            public string UserImagePath
            {
                get
                {
                    return currentUserIconPath;
                }
                set
                {
                    if (currentUserIconPath != value)
                    {
                        currentUserIconPath = value;
                        this.RemoveChild(userIcon);
                        userIcon = new PictureObject(device, resourceManager, PathObject.Absolute(currentUserIconPath), true)
                        {
                            Position = new Vector2(21, 21)
                        };
                        this.InsertChild(userIcon, 0);
                        ChangeUserIconScale();
                    }
                }
            }

            public string UserName
            {
                get
                {
                    return userNameString.Text;
                }
                set
                {
                    if (userNameString.Text != value)
                    {
                        userNameString.Text = value;
                        userNameString.Update();
                        rectangle.RectangleWidth = userNameString.Width + 40;
                    }
                }
            }

            private void ChangeUserIconScale()
            {
                userIcon.Scale = new SharpDX.Vector2(20 / userIcon.Width, 20 / userIcon.Height);
            }
        }

        class UserMenuDialog : FocusableGameComponent
        {
            public enum Mode
            {
                ChangeLeader,
                Kick,
            }

            PPDFramework.Resource.ResourceManager resourceManager;
            SpriteObject textSprite;

            RectangleComponent selectRectangle;
            PictureObject back;
            int index;

            public UserMenuDialog(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
            {
                this.resourceManager = resourceManager;

                this.AddChild(new TextureString(device, Utility.Language["Menu"], 14, PPDColors.White)
                {
                    Position = new Vector2(10, 7)
                });
                this.AddChild(textSprite = new SpriteObject(device)
                {
                    Position = new Vector2(5, 35)
                });
                this.AddChild(selectRectangle = new RectangleComponent(device, resourceManager, PPDColors.White)
                {
                    RectangleWidth = 100,
                    RectangleHeight = 22
                });
                this.AddChild(back = new PictureObject(device, resourceManager, Utility.Path.Combine("small_dialog_back.png")));
                this.AddChild(new RectangleComponent(device, resourceManager, PPDColors.Black)
                {
                    RectangleWidth = back.Width,
                    RectangleHeight = back.Height,
                    Alpha = 0.75f
                });

                textSprite.AddChild(new TextureString(device, Utility.Language["ChangeLeader"], 16, 80, PPDColors.White)
                {
                    Position = new Vector2(10, 0)
                });
                textSprite.AddChild(new TextureString(device, Utility.Language["KickPlayer"], 16, 80, PPDColors.White)
                {
                    Position = new Vector2(10, 25)
                });
                index = 0;

                ChangeSelect();

                Inputed += DifficultyDialog_Inputed;
            }

            private TextureString CurrentText
            {
                get
                {
                    return textSprite[index] as TextureString;
                }
            }

            public bool Selected
            {
                get;
                private set;
            }

            public Mode SelectedMode
            {
                get;
                private set;
            }

            private void ChangeSelect()
            {
                CurrentText.Color = PPDColors.Black;
                selectRectangle.Position = textSprite[index].Position + textSprite.Position;
                selectRectangle.RectangleWidth = ((TextureString)textSprite[index]).JustWidth;
                ((TextureString)textSprite[index]).AllowScroll = true;
            }

            void DifficultyDialog_Inputed(IFocusable sender, InputEventArgs args)
            {
                if (args.InputInfo.IsPressed(ButtonType.Cross))
                {
                    FocusManager.RemoveFocus();
                }
                else if (args.InputInfo.IsPressed(ButtonType.Circle))
                {
                    Selected = true;
                    SelectedMode = (UserMenuDialog.Mode)index;
                    FocusManager.RemoveFocus();
                }
                else if (args.InputInfo.IsPressed(ButtonType.Up))
                {
                    CurrentText.Color = PPDColors.White;
                    index--;
                    if (index < 0)
                    {
                        index = textSprite.ChildrenCount - 1;
                    }
                    ChangeSelect();
                }
                else if (args.InputInfo.IsPressed(ButtonType.Down))
                {
                    CurrentText.Color = PPDColors.White;
                    index++;
                    if (index >= textSprite.ChildrenCount)
                    {
                        index = 0;
                    }
                    ChangeSelect();
                }
            }
        }
    }
}