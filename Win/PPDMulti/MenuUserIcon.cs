using PPDFramework;
using PPDFrameworkCore;
using PPDMulti.Model;
using PPDMultiCommon.Data;
using System;

namespace PPDMulti
{
    class MenuUserIcon : BindableGameComponent
    {
        PPDFramework.Resource.ResourceManager resourceManager;

        PictureObject readyIcon;
        PictureObject notReadyIcon;
        PictureObject playingIcon;
        PictureObject watchIcon;
        PictureObject hasSongIcon;

        PictureObject userIcon;
        TextureString userNameString;
        TextureString accountIdString;
        PictureObject[] pingPictures;
        TextureString pingString;
        PictureObject leaderIcon;

        private string currentUserIconPath = Utility.Path.Combine("noimage_icon.png");

        UserListComponent listComponent;

        public MenuUserIcon(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, UserListComponent listComponent, User user) : base(device)
        {
            this.resourceManager = resourceManager;
            this.listComponent = listComponent;
            User = user;

            this.AddChild((leaderIcon = new PictureObject(device, resourceManager, Utility.Path.Combine("leader.png"))));
            this.AddChild((userIcon = new PictureObject(device, resourceManager, PathObject.Absolute(currentUserIconPath), true)
            {
                Position = new SharpDX.Vector2(21, 21)
            }));
            this.AddChild((userNameString = new TextureString(device, "", 15, 140, PPDColors.White)
            {
                Position = new SharpDX.Vector2(42, 3)
            }));
            this.AddChild((accountIdString = new TextureString(device, "", 15, 140, PPDColors.White)
            {
                Position = new SharpDX.Vector2(42, 3)
            }));
            this.AddChild((readyIcon = new PictureObject(device, resourceManager, Utility.Path.Combine("user_state_ready.png"), true)
            {
                Position = new SharpDX.Vector2(187, 30)
            }));
            this.AddChild((notReadyIcon = new PictureObject(device, resourceManager, Utility.Path.Combine("user_state_notready.png"), true)
            {
                Position = new SharpDX.Vector2(187, 30)
            }));
            this.AddChild((playingIcon = new PictureObject(device, resourceManager, Utility.Path.Combine("user_state_playing.png"), true)
            {
                Position = new SharpDX.Vector2(187, 30)
            }));
            this.AddChild((watchIcon = new PictureObject(device, resourceManager, Utility.Path.Combine("user_state_watch.png"), true)
            {
                Position = new SharpDX.Vector2(187, 30)
            }));
            this.AddChild(hasSongIcon = new PictureObject(device, resourceManager, Utility.Path.Combine("hassong.png"), true)
            {
                Position = new SharpDX.Vector2(165, 30)
            });
            pingPictures = new PictureObject[5];
            for (int i = 0; i < 5; i++)
            {
                pingPictures[i] = new PictureObject(device, resourceManager, Utility.Path.Combine(String.Format("ping_{0}.png", i)))
                {
                    Position = new SharpDX.Vector2(42, 22)
                };
                this.AddChild(pingPictures[i]);
            }
            this.AddChild(pingString = new TextureString(device, "0", 12, PPDColors.White)
            {
                Position = new SharpDX.Vector2(60, 24)
            });
            this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("user_back.png")));

            AddBinding(new Binding(user, "Name", this, "UserName"));
            AddBinding(new Binding(user, "AccountId", this, "AccountId"));
            AddBinding(new Binding(user, "ImagePath", this, "UserImagePath"));
            AddBinding(new Binding(user, "CurrentState", this, "UserState"));
            AddBinding(new Binding(user, "HasSong", this, "HasSong"));
            AddBinding(new Binding(user, "Ping", this, "Ping"));
            AddBinding(new Binding(user, "IsLeader", this, "IsLeader"));

            ChangeUserIconScale();
        }

        public User User
        {
            get;
            private set;
        }

        public bool IsLeader
        {
            get { return !leaderIcon.Hidden; }
            set
            {
                if (!leaderIcon.Hidden != value)
                {
                    leaderIcon.Hidden = !value;
                }
            }
        }

        public bool HasSong
        {
            get
            {
                return !hasSongIcon.Hidden;
            }
            set
            {
                if (!hasSongIcon.Hidden != value)
                {
                    hasSongIcon.Hidden = !value;
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
                        Position = new SharpDX.Vector2(21, 21)
                    };
                    this.InsertChild(userIcon, 1);
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
                userNameString.Text = value;
            }
        }

        public string AccountId
        {
            get
            {
                return accountIdString.Text;
            }
            set
            {
                var newValue = String.Format("@{0}", value);
                accountIdString.Text = newValue;
            }
        }

        public UserState UserState
        {
            set
            {
                readyIcon.Hidden = notReadyIcon.Hidden = watchIcon.Hidden = playingIcon.Hidden = true;
                switch (value)
                {
                    case UserState.NotReady:
                        notReadyIcon.Hidden = false;
                        break;
                    case UserState.Ready:
                        readyIcon.Hidden = false;
                        break;
                    case UserState.Watch:
                        watchIcon.Hidden = false;
                        break;
                    case UserState.Playing:
                        playingIcon.Hidden = false;
                        break;
                }
            }
        }

        public int Ping
        {
            set
            {
                this.pingString.Text = value.ToString();
                foreach (PictureObject pictureObject in pingPictures)
                {
                    pictureObject.Hidden = true;
                }
                if (value < 100)
                {
                    pingPictures[4].Hidden = false;
                }
                else if (value < 200)
                {
                    pingPictures[3].Hidden = false;
                }
                else if (value < 300)
                {
                    pingPictures[2].Hidden = false;
                }
                else if (value < 400)
                {
                    pingPictures[1].Hidden = false;
                }
                else
                {
                    pingPictures[0].Hidden = false;
                }
            }
        }

        private void ChangeUserIconScale()
        {
            userIcon.Scale = new SharpDX.Vector2(32 / userIcon.Width, 32 / userIcon.Height);
        }

        protected override void UpdateImpl()
        {
            if (userNameString.Hidden != !listComponent.IsNameVisible)
            {
                userNameString.Hidden = !listComponent.IsNameVisible;
                if (!userNameString.Hidden)
                {
                    userNameString.AllowScroll = true;
                }
            }
            if (accountIdString.Hidden != listComponent.IsNameVisible)
            {
                accountIdString.Hidden = listComponent.IsNameVisible;
                if (!accountIdString.Hidden)
                {
                    accountIdString.AllowScroll = true;
                }
            }
            userNameString.Alpha = listComponent.NameAlpha;
            accountIdString.Alpha = listComponent.IdAlpha;
        }
    }
}
