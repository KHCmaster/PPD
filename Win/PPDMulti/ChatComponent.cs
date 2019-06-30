using PPDFramework;
using PPDFrameworkCore;
using PPDMulti.Model;
using PPDMultiCommon.Model;
using PPDShareComponent;
using SharpDX;
using System;

namespace PPDMulti
{
    class ChatComponent : FocusableGameComponent
    {
        SpriteObject messageSprite;
        PictureObject back;
        PictureObject scrollBar;

        int scrollIndex;
        const int scrollBarHeight = 361;

        PPDFramework.Resource.ResourceManager resourceManager;
        IGameHost gameHost;

        public ChatComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, IGameHost gameHost) : base(device)
        {
            this.resourceManager = resourceManager;
            this.gameHost = gameHost;

            SpriteObject scrollSprite;

            this.AddChild((messageSprite = new SpriteObject(device)
            {
                Clip = new ClipInfo(gameHost)
                {
                    PositionX = 450,
                    PositionY = 0,
                    Width = 340,
                    Height = 374
                }
            }));
            this.AddChild(scrollSprite = new SpriteObject(device)
            {
                Position = new Vector2(352, 5)
            });
            scrollSprite.AddChild((scrollBar = new PictureObject(device, resourceManager, Utility.Path.Combine("chat_scrollbar.png"))));
            this.AddChild((back = new PictureObject(device, resourceManager, Utility.Path.Combine("chat_back.png"))));
        }

        public void ScrollUp()
        {
            scrollIndex--;
            if (scrollIndex < 0)
            {
                scrollIndex = 0;
            }
        }

        public void ScrollDown()
        {
            scrollIndex++;
            if (scrollIndex >= messageSprite.ChildrenCount - 9)
            {
                scrollIndex = messageSprite.ChildrenCount - 9 < 0 ? 0 : messageSprite.ChildrenCount - 9;
            }
        }

        public void AddMessage(string text, User user)
        {
            AddMessage(text, user, false);
        }


        public void AddMessage(string text, User user, bool isPrivate)
        {
            var message = new Message
            {
                Text = text,
                User = user,
                IsPrivate = isPrivate
            };

            messageSprite.AddChild(new MessageComponent(device, resourceManager, message) { Position = new SharpDX.Vector2(20, messageSprite.ChildrenCount * 41) });

            if (scrollIndex + 1 == messageSprite.ChildrenCount - 9)
            {
                ScrollDown();
            }
        }

        public void AddSystemMessage(string text)
        {
            var message = new Message
            {
                Text = String.Format("[{0}]\n{1}", Utility.Language["SystemMessage"], text),
                User = User.SystemUser
            };

            messageSprite.AddChild(new SystemMessageComponent(device, resourceManager, message) { Position = new SharpDX.Vector2(20, messageSprite.ChildrenCount * 41) });

            if (scrollIndex + 1 == messageSprite.ChildrenCount - 9)
            {
                ScrollDown();
            }
        }

        protected override void UpdateImpl()
        {
            scrollBar.Scale = new Vector2(1, messageSprite.ChildrenCount - 9 <= 0 ? 1 : (float)9 / messageSprite.ChildrenCount);
            scrollBar.Scale = new Vector2(1, scrollBar.Scale.Y <= 0.001f ? 0.001f : scrollBar.Scale.Y);
            scrollBar.Position = new Vector2(0, AnimationUtility.GetAnimationValue(scrollBar.Position.Y, messageSprite.ChildrenCount == 0 ? 0 : scrollBarHeight / (float)messageSprite.ChildrenCount * scrollIndex));
            messageSprite.Position = new Vector2(0, AnimationUtility.GetAnimationValue(messageSprite.Position.Y, -41 * scrollIndex));
        }

        class SystemMessageComponent : BindableGameComponent
        {
            PPDFramework.Resource.ResourceManager resourceManager;

            TextureString messageString;
            PictureObject back;

            public SystemMessageComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, Message message) : base(device)
            {
                this.resourceManager = resourceManager;

                Color4 color = PPDColors.White;

                this.AddChild((messageString = new TextureString(device, "", 10, 300, 40, true, PPDColors.Black)
                {
                    Position = new Vector2(10, 1)
                }));
                this.AddChild((back = new PictureObject(device, resourceManager, Utility.Path.Combine("system_back.png"))));

                AddBinding(new Binding(message, "User.Color", this, "Color"));
                AddBinding(new Binding(message, "Text", this, "Text"));

                Alpha = 0;
            }

            public string Text
            {
                get
                {
                    return messageString.Text;
                }
                set
                {
                    messageString.Text = value;
                }
            }

            protected override void UpdateImpl()
            {
                Alpha = AnimationUtility.IncreaseAlpha(Alpha);
            }
        }

        class MessageComponent : BindableGameComponent
        {
            PPDFramework.Resource.ResourceManager resourceManager;

            TextureString userName;
            PictureObject userIcon;
            TextureString messageString;
            PictureObject back;
            Message message;

            private string currentUserIconPath = Utility.Path.Combine("noimage_icon.png");

            public MessageComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, Message message) : base(device)
            {
                this.resourceManager = resourceManager;
                this.message = message;

                Color4 color = PPDColors.White;

                if (message.User.IsSelf)
                {
                    this.AddChild((userIcon = new PictureObject(device, resourceManager, PathObject.Absolute(currentUserIconPath), true)
                    {
                        Position = new Vector2(16, 20)
                    }));
                    this.AddChild((userName = new TextureString(device, "", 10, PPDColors.Black)
                    {
                        Border = new Border { Color = PPDColors.White, Thickness = 1 }
                    }));
                    this.AddChild((messageString = new TextureString(device, "", 10, 260, 40, true, PPDColors.Black)
                    {
                        Position = new Vector2(50, 1)
                    }));
                    this.AddChild((back = new PictureObject(device, resourceManager, Utility.Path.Combine("message_back.png"))
                    {
                        Position = new Vector2(32, 0),
                        EdgeColors = new[] { color, color, color, color }
                    }));
                }
                else
                {
                    this.AddChild((userIcon = new PictureObject(device, resourceManager, Utility.Path.Combine("noimage_icon.png"), true)
                    {
                        Position = new Vector2(312, 20)
                    }));
                    this.AddChild((userName = new TextureString(device, "", 10, PPDColors.Black)
                    {
                        Border = new Border { Color = PPDColors.White, Thickness = 1 }
                    }));
                    this.AddChild((messageString = new TextureString(device, "", 10, 260, 40, true, PPDColors.Black)
                    {
                        Position = new Vector2(10, 1)
                    }));
                    this.AddChild((back = new PictureObject(device, resourceManager, Utility.Path.Combine("message_back.png"))
                    {
                        Scale = new Vector2(-1, 1),
                        EdgeColors = new[] { color, color, color, color }
                    }));
                    back.ScaleCenter = new Vector2(back.Width / 2, back.Height / 2);
                }

                AddBinding(new Binding(message, "User.Name", this, "UserName"));
                AddBinding(new Binding(message, "User.ImagePath", this, "UserImagePath"));
                AddBinding(new Binding(message, "Text", this, "Text"));
                AddBinding(new Binding(message, "User.Color", this, "Color"));

                Alpha = 0;

                ChangeUserIconScale();
            }

            public string UserName
            {
                get
                {
                    return userName.Text;
                }
                set
                {
                    if (userName.Text != value)
                    {
                        userName.Text = String.Format("{0}[{1}]", message.IsPrivate ? "P" : "", value);
                        userName.Update();
                        if (message.User.IsSelf)
                        {
                            userName.Position = new Vector2(320 - userName.Width, 25);
                        }
                        else
                        {
                            userName.Position = new Vector2(280 - userName.Width, 25);
                        }
                    }
                }
            }

            public string Text
            {
                get
                {
                    return messageString.Text;
                }
                set
                {
                    messageString.Text = value;
                }
            }

            public Color4 Color
            {
                set
                {
                    if (back.EdgeColors[0] != value)
                    {
                        back.EdgeColors = new[] { value, value, value, value };
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
                        if (message.User.IsSelf)
                        {
                            userIcon = new PictureObject(device, resourceManager, PathObject.Absolute(currentUserIconPath), true)
                            {
                                Position = new Vector2(16, 20)
                            };
                        }
                        else
                        {
                            userIcon = new PictureObject(device, resourceManager, PathObject.Absolute(currentUserIconPath), true)
                            {
                                Position = new Vector2(312, 20)
                            };
                        }
                        this.InsertChild(userIcon, 0);
                        ChangeUserIconScale();
                    }
                }
            }

            private void ChangeUserIconScale()
            {
                userIcon.Scale = new SharpDX.Vector2(32 / userIcon.Width, 32 / userIcon.Height);
            }

            protected override void UpdateImpl()
            {
                Alpha = AnimationUtility.IncreaseAlpha(Alpha);
            }
        }
    }
}
