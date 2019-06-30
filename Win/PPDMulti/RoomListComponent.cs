using PPDFramework;
using PPDMultiCommon.Web;
using PPDShareComponent;
using SharpDX;
using System;

namespace PPDMulti
{
    class RoomListComponent : FocusableGameComponent
    {
        public event Action<RoomInfo> RoomSelected;

        const int scrollBarStartY = 80;
        const int scrollBarMaxHeight = 330;
        const int MaxDisplayCount = 12;

        PPDFramework.Resource.ResourceManager resourceManager;
        ISound sound;
        IGameHost gameHost;

        PictureObject back;
        SpriteObject listSprite;

        LineRectangleComponent selectRectangle;
        RectangleComponent scrollBar;
        EffectObject loading;
        int currentIndex;
        int scrollIndex;

        GetRoomListExecutor getRoomListExecutor;
        bool listGot;
        int timerID;

        private RoomContentComponent CurrentComponent
        {
            get
            {
                return (currentIndex < listSprite.ChildrenCount ? listSprite[currentIndex] : null) as RoomContentComponent;
            }
        }

        public RoomListComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound, IGameHost gameHost) : base(device)
        {
            this.resourceManager = resourceManager;
            this.sound = sound;
            this.gameHost = gameHost;

            this.AddChild(back = new PictureObject(device, resourceManager, Utility.Path.Combine("dialog_back.png")));
            back.AddChild(loading = new EffectObject(device, resourceManager, Utility.Path.Combine("loading_icon.etd"))
            {
                Position = new Vector2(700, 60)
            });
            back.AddChild(scrollBar = new RectangleComponent(device, resourceManager, PPDColors.White)
            {
                Position = new Vector2(756, 80),
                RectangleHeight = 330,
                RectangleWidth = 5
            });
            back.AddChild(selectRectangle = new LineRectangleComponent(device, resourceManager, PPDColors.Selection) { RectangleWidth = 714, RectangleHeight = 28 });
            back.AddChild(new TextureString(device, Utility.Language["RoomList"], 30, PPDColors.White)
            {
                Position = new Vector2(35, 30)
            });
            back.AddChild(listSprite = new SpriteObject(device) { Position = new SharpDX.Vector2(38, 77) });

            loading.PlayType = Effect2D.EffectManager.PlayType.Loop;
            loading.Play();
            loading.Scale = new Vector2(0.125f);
            loading.Hidden = true;

            getRoomListExecutor = new GetRoomListExecutor();
            getRoomListExecutor.Finished += getRoomListExecutor_Finished;

            timerID = gameHost.AddTimerCallBack(timerCallBack, 15000, false, true);

            Inputed += RoomListComponent_Inputed;
            LostFocused += RoomListComponent_LostFocused;
        }

        void RoomListComponent_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            if (!OverFocused)
            {
                gameHost.RemoveTimerCallBack(timerID);
            }
        }

        private void timerCallBack(int obj)
        {
            StartGetRoomList();
        }

        private void StartGetRoomList()
        {
            loading.Hidden = false;
            getRoomListExecutor.Start();
        }

        void getRoomListExecutor_Finished(object sender, EventArgs e)
        {
            loading.Hidden = true;
            listGot = true;
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

            for (int i = 0; i < listSprite.ChildrenCount; i++)
            {
                listSprite[i].Position = new Vector2(0, 28 * (i - scrollIndex));
                listSprite[i].Hidden = i < scrollIndex || scrollIndex + MaxDisplayCount <= i;
            }

            scrollBar.RectangleHeight = listSprite.ChildrenCount <= MaxDisplayCount ? scrollBarMaxHeight : scrollBarMaxHeight * MaxDisplayCount / listSprite.ChildrenCount;
            scrollBar.Position = new Vector2(scrollBar.Position.X, listSprite.ChildrenCount <= MaxDisplayCount ? scrollBarStartY : scrollBarStartY + scrollBarMaxHeight * scrollIndex / listSprite.ChildrenCount);
        }

        private void AddRoomInfo(RoomInfo roomInfo)
        {
            listSprite.AddChild(new RoomContentComponent(device, resourceManager, roomInfo)
            {
                Position = new SharpDX.Vector2(0, 28 * listSprite.ChildrenCount)
            });

        }

        void RoomListComponent_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                FocusManager temp = FocusManager;
                temp.RemoveFocus();
                temp.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                if (CurrentComponent != null)
                {
                    sound.Play(PPDSetting.DefaultSounds[1], -1000);
                    if (CurrentComponent.RoomInfo.HasPassword)
                    {
                        var dialog = new PasswordDialog(device, resourceManager, gameHost, CurrentComponent.RoomInfo);
                        dialog.Processed += dialog_Processed;
                        FocusManager.Focus(dialog);
                        this.InsertChild(dialog, 0);
                    }
                    else
                    {
                        GoToRoom();
                    }
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Left))
            {
                if (CurrentComponent != null)
                {
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                    currentIndex -= MaxDisplayCount / 2;
                    if (currentIndex < 0)
                    {
                        currentIndex = listSprite.ChildrenCount - 1;
                    }
                    AdjustScrollBar();
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Right))
            {
                if (CurrentComponent != null)
                {
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                    currentIndex += MaxDisplayCount / 2;
                    if (currentIndex >= listSprite.ChildrenCount)
                    {
                        currentIndex = 0;
                    }
                    AdjustScrollBar();
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                if (CurrentComponent != null)
                {
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                    currentIndex--;
                    if (currentIndex < 0)
                    {
                        currentIndex = listSprite.ChildrenCount - 1;
                    }
                    AdjustScrollBar();
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                if (CurrentComponent != null)
                {
                    sound.Play(PPDSetting.DefaultSounds[0], -1000);
                    currentIndex++;
                    if (currentIndex >= listSprite.ChildrenCount)
                    {
                        currentIndex = 0;
                    }
                    AdjustScrollBar();
                }
            }
        }

        void dialog_Processed(object sender, EventArgs e)
        {
            var dialog = sender as PasswordDialog;
            if (dialog.IsValid)
            {
                GoToRoom();
            }
        }

        private void GoToRoom()
        {
            gameHost.RemoveTimerCallBack(timerID);
            if (RoomSelected != null)
            {
                RoomSelected.Invoke(CurrentComponent.RoomInfo);
            }
        }

        protected override void UpdateImpl()
        {
            if (CurrentComponent != null)
            {
                selectRectangle.Hidden = false;
                selectRectangle.Position = listSprite.Position + CurrentComponent.Position;
            }
            else
            {
                selectRectangle.Hidden = true;
            }

            if (listGot)
            {
                listGot = false;

                if (FocusManager != null && FocusManager.CurrentFocusObject == this)
                {
                    listSprite.ClearChildren();
                    foreach (RoomInfo roomInfo in getRoomListExecutor.RoomList)
                    {
                        AddRoomInfo(roomInfo);
                    }

                    if (currentIndex >= listSprite.ChildrenCount)
                    {
                        currentIndex = listSprite.ChildrenCount == 0 ? 0 : listSprite.ChildrenCount - 1;
                    }
                    AdjustScrollBar();
                }
            }
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
        }

        class RoomContentComponent : GameComponent
        {
            private bool selected;
            TextureString roomName;
            TextureString userName;


            public RoomContentComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, RoomInfo roomInfo) : base(device)
            {
                this.RoomInfo = roomInfo;

                if (roomInfo.HasPassword)
                {
                    this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("lock.png"))
                    {
                        Position = new Vector2(4, 1)
                    });
                }

                this.AddChild(roomName = new TextureString(device, roomInfo.RoomName, 20, 400, PPDColors.White)
                {
                    Position = new Vector2(25, 2)
                });
                this.AddChild(userName = new TextureString(device, roomInfo.UserName, 20, 140, PPDColors.White)
                {
                    Position = new Vector2(445, 2)
                });
                this.AddChild(new TextureString(device, roomInfo.Language.ToUpper(), 20, PPDColors.White)
                {
                    Position = new Vector2(605, 2)
                });
                this.AddChild(new TextureString(device, String.Format("{0}/16", roomInfo.PlayerCount), 20, 80, PPDColors.White)
                {
                    Position = new Vector2(655, 2)
                });
                this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("roomback.png")));

                roomName.AllowScroll = userName.AllowScroll = false;
            }

            public RoomInfo RoomInfo
            {
                get;
                private set;
            }

            public bool Selected
            {
                get { return selected; }
                set
                {
                    selected = value;
                    roomName.AllowScroll = userName.AllowScroll = selected;
                }
            }
        }
    }
}
