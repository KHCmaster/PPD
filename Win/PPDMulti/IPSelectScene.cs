using PPDFramework;
using PPDFramework.Scene;
using PPDMultiCommon.Web;
using PPDShareComponent;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace PPDMulti
{
    public class IPSelectScene : SceneBase
    {
        bool asHost;
        string ip;
        int port;
        string roomName;
        string password;

        enum NextPanel
        {
            None,
            Port,
            PortToConfrim,
            Password,
            Confirm
        }

        NextPanel generatePanel = NextPanel.None;

        int currentIndex;
        SpriteObject allSlideSprite;
        SlideSprite firstSprite;

        FocusManager focusManager;

        DxTextBox currentTextBox;
        DxTextBox shouldFocusTextBox;
        bool oneFramePassedAfterRemoveFocus;

        RoomInfo lastCreatedRoom;

        public IPSelectScene(PPDDevice device) : base(device)
        {
        }

        public override bool Load()
        {
            this.AddChild((allSlideSprite = new SpriteObject(device)));

            LoadLastCreatedRoom();

            firstSprite = new SlideSprite(device, ResourceManager, Sound) { FirstSlide = true };

            firstSprite.AddSelection(Utility.Language["CreateRoom"]);
            firstSprite.AddSelection(Utility.Language["EnterRoom"]);
            firstSprite.AddSelection(Utility.Language["WaitPlayerAsHost"]);
            firstSprite.AddSelection(Utility.Language["ConnectToHost"]);

            allSlideSprite.AddChild(firstSprite);

            focusManager = new FocusManager();
            focusManager.Focus(firstSprite);

            this.AddChild(new PictureObject(device, ResourceManager, Utility.Path.Combine("background.png")));

            firstSprite.Selected += slideSprite_Selected;

            return true;
        }

        private void LoadLastCreatedRoom()
        {
            try
            {
                var document = XDocument.Parse(SkinSetting.Setting.LastCreatedRoom);
                var roomElem = document.Element("Root").Element("Room");
                lastCreatedRoom = new RoomInfo("", roomElem.Attribute("Name").Value,
                    roomElem.Attribute("Password").Value, int.Parse(roomElem.Attribute("Port").Value), "", "", 0);
            }
            catch
            {
            }
        }

        private void SaveLastCreatedRoom()
        {
            try
            {
                var document = new XDocument(new XElement("Root",
                        new XElement("Room",
                            new XAttribute("Name", lastCreatedRoom.RoomName),
                            new XAttribute("Password", lastCreatedRoom.PasswordHash),
                            new XAttribute("Port", lastCreatedRoom.Port)
                        )));
                SkinSetting.Setting.LastCreatedRoom = document.ToString();
            }
            catch
            {
            }
        }

        private void CreateRoomNameSprite()
        {
            var roomSprite = new SlideSprite(device, ResourceManager, Sound) { Position = new SharpDX.Vector2(800 * currentIndex, 0) };
            roomSprite.AddUserComponent(new TextureString(device, Utility.Language["InputRoomName"], 18, true, PPDColors.White)
            {
                Position = new SharpDX.Vector2(400, 200)
            });
            var roomNameTextBox = new DxTextBox(device, GameHost, ResourceManager)
            {
                DrawOnlyFocus = false
            };
            roomNameTextBox.LostFocused += roomNameTextBox_LostFocused;
            roomNameTextBox.Position = new SharpDX.Vector2(300, 250);
            roomNameTextBox.TextBoxHeight = 20;
            roomNameTextBox.TextBoxWidth = roomNameTextBox.MaxWidth = 200;
            roomNameTextBox.Text = "";
            currentTextBox = roomNameTextBox;
            roomSprite.AddUserComponent(roomNameTextBox);
            roomSprite.AddUserComponent(new TextureString(device, "", 18, true, PPDColors.White)
            {
                Position = new SharpDX.Vector2(400, 300)
            });
            roomSprite.Inputed += slideSprite_Inputed;
            roomSprite.LostFocused += slideSprite_LostFocused;
            allSlideSprite.AddChild(roomSprite);
            focusManager.Focus(roomSprite);
            focusManager.Focus(roomNameTextBox);
        }

        void slideSprite_Selected(object sender, EventArgs e)
        {
            currentIndex++;

            var slideSprite = sender as SlideSprite;
            switch (slideSprite.CurrentSelection)
            {
                case 0:
                    asHost = true;
                    if (lastCreatedRoom == null)
                    {
                        CreateRoomNameSprite();
                    }
                    else
                    {
                        var confirmSprite = new SlideSprite(device, ResourceManager, Sound)
                        {
                            Position = new SharpDX.Vector2(800 * currentIndex, 0),
                            AutoBack = false
                        };
                        confirmSprite.AddUserComponent(new TextureString(device, Utility.Language["CreateRoomByLastInfoConfirm"], 18, true, PPDColors.White)
                        {
                            Position = new SharpDX.Vector2(400, 150)
                        });
                        confirmSprite.AddUserComponent(new TextureString(device, String.Format(Utility.Language["RoomName"], lastCreatedRoom.RoomName), 18, PPDColors.White)
                        {
                            Position = new SharpDX.Vector2(300, 200)
                        });
                        confirmSprite.AddUserComponent(new TextureString(device, String.Format(Utility.Language["Password"], lastCreatedRoom.PasswordHash), 18, PPDColors.White)
                        {
                            Position = new SharpDX.Vector2(300, 230)
                        });
                        confirmSprite.AddUserComponent(new TextureString(device, String.Format(Utility.Language["Port"], lastCreatedRoom.Port), 18, PPDColors.White)
                        {
                            Position = new SharpDX.Vector2(300, 260)
                        });
                        confirmSprite.LostFocused += slideSprite_LostFocused;
                        confirmSprite.Inputed += confirmSprite_Inputed2;
                        allSlideSprite.AddChild(confirmSprite);
                        focusManager.Focus(confirmSprite);
                    }
                    break;
                case 1:
                    var roomListSplite = new SlideSprite(device, ResourceManager, Sound) { Position = new SharpDX.Vector2(800 * currentIndex, 0) };
                    var roomList = new RoomListComponent(device, ResourceManager, Sound, GameHost);
                    roomList.RoomSelected += roomList_RoomSelected;
                    roomListSplite.AddUserComponent(roomList);
                    roomListSplite.LostFocused += slideSprite_LostFocused;
                    allSlideSprite.AddChild(roomListSplite);
                    focusManager.Focus(roomListSplite);
                    focusManager.Focus(roomList);
                    break;
                case 2:
                    asHost = true;
                    CreatePortPanel(false);
                    break;
                case 3:
                    asHost = false;
                    var ipSprite = new SlideSprite(device, ResourceManager, Sound) { Position = new SharpDX.Vector2(800 * currentIndex, 0) };
                    ipSprite.AddUserComponent(new TextureString(device, Utility.Language["InputIP"], 18, true, PPDColors.White)
                    {
                        Position = new SharpDX.Vector2(400, 200)
                    });
                    var ipTextBox = new DxTextBox(device, GameHost, ResourceManager)
                    {
                        DrawOnlyFocus = false
                    };
                    ipTextBox.LostFocused += ipTextBox_LostFocused;
                    ipTextBox.Position = new SharpDX.Vector2(300, 250);
                    ipTextBox.TextBoxHeight = 20;
                    ipTextBox.TextBoxWidth = ipTextBox.MaxWidth = 200;
                    ipTextBox.Text = "";
                    currentTextBox = ipTextBox;
                    ipSprite.AddUserComponent(ipTextBox);
                    ipSprite.AddUserComponent(new TextureString(device, "", 18, true, PPDColors.White)
                    {
                        Position = new SharpDX.Vector2(400, 300)
                    });
                    ipSprite.Inputed += slideSprite_Inputed;
                    ipSprite.LostFocused += slideSprite_LostFocused;
                    allSlideSprite.AddChild(ipSprite);
                    focusManager.Focus(ipSprite);
                    focusManager.Focus(ipTextBox);
                    break;
            }
        }

        void roomList_RoomSelected(RoomInfo obj)
        {
            asHost = false;
            ip = obj.IP;
            port = obj.Port;
            GoNextScene(false);
        }

        void slideSprite_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Start))
            {
                if (focusManager.CurrentFocusObject == currentTextBox)
                {
                    focusManager.RemoveFocus();
                }
                else if (oneFramePassedAfterRemoveFocus)
                {
                    focusManager.Focus(currentTextBox);
                }
            }
        }

        private void CreatePortPanel(bool toConfirm)
        {
            var portSprite = new SlideSprite(device, ResourceManager, Sound) { Position = new SharpDX.Vector2(800 * currentIndex, 0) };
            portSprite.AddUserComponent(new TextureString(device, Utility.Language["InputPort"], 18, true, PPDColors.White)
            {
                Position = new SharpDX.Vector2(400, 200)
            });
            var portTextBox = new DxTextBox(device, GameHost, ResourceManager)
            {
                DrawOnlyFocus = false
            };
            if (!toConfirm)
            {
                portTextBox.LostFocused += portTextBox_LostFocused;
            }
            else
            {
                portTextBox.LostFocused += portToConfirmTextBox_LostFocused;
            }
            portTextBox.Position = new SharpDX.Vector2(300, 250);
            portTextBox.TextBoxHeight = 20;
            portTextBox.TextBoxWidth = portTextBox.MaxWidth = 200;
            portTextBox.Text = "";
            currentTextBox = portTextBox;
            portSprite.AddUserComponent(portTextBox);
            portSprite.AddUserComponent(new TextureString(device, "", 18, true, PPDColors.White)
            {
                Position = new SharpDX.Vector2(400, 300)
            });
            portSprite.AddUserComponent(new TextureString(device, asHost ? Utility.Language["MustOpenPort"] : "", 18, true, PPDColors.White)
            {
                Position = new SharpDX.Vector2(400, 220)
            });
            portSprite.LostFocused += slideSprite_LostFocused;
            portSprite.Inputed += slideSprite_Inputed;
            allSlideSprite.AddChild(portSprite);
            focusManager.Focus(portSprite);
            focusManager.Focus(portTextBox);
        }

        private void CreateConfirmPanel()
        {
            var confirmSprite = new SlideSprite(device, ResourceManager, Sound)
            {
                Position = new SharpDX.Vector2(800 * currentIndex, 0)
            };
            confirmSprite.AddUserComponent(new TextureString(device, Utility.Language["ConfirmCreateRoom"], 18, true, PPDColors.White)
            {
                Position = new SharpDX.Vector2(400, 150)
            });
            confirmSprite.AddUserComponent(new TextureString(device, String.Format(Utility.Language["RoomName"], roomName), 18, PPDColors.White)
            {
                Position = new SharpDX.Vector2(300, 200)
            });
            confirmSprite.AddUserComponent(new TextureString(device, String.Format(Utility.Language["Password"], password), 18, PPDColors.White)
            {
                Position = new SharpDX.Vector2(300, 230)
            });
            confirmSprite.AddUserComponent(new TextureString(device, String.Format(Utility.Language["Port"], port), 18, PPDColors.White)
            {
                Position = new SharpDX.Vector2(300, 260)
            });
            confirmSprite.LostFocused += slideSprite_LostFocused;
            confirmSprite.Inputed += confirmSprite_Inputed;
            allSlideSprite.AddChild(confirmSprite);
            focusManager.Focus(confirmSprite);
        }

        void confirmSprite_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                GoNextScene(true);
            }
        }

        void confirmSprite_Inputed2(IFocusable sender, InputEventArgs args)
        {
            roomName = lastCreatedRoom.RoomName;
            password = lastCreatedRoom.PasswordHash;
            port = lastCreatedRoom.Port;
            if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                GoNextScene(true);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                currentIndex++;
                CreateRoomNameSprite();
            }
        }

        private void CreatePasswordPanel()
        {
            var passwordSprite = new SlideSprite(device, ResourceManager, Sound) { Position = new SharpDX.Vector2(800 * currentIndex, 0) };
            passwordSprite.AddUserComponent(new TextureString(device, Utility.Language["InputPassword"], 18, true, PPDColors.White)
            {
                Position = new SharpDX.Vector2(400, 200)
            });
            var passwordTextBox = new DxTextBox(device, GameHost, ResourceManager)
            {
                DrawOnlyFocus = false
            };
            passwordTextBox.LostFocused += passwordTextBox_LostFocused;
            passwordTextBox.Position = new SharpDX.Vector2(300, 250);
            passwordTextBox.TextBoxHeight = 20;
            passwordTextBox.TextBoxWidth = passwordTextBox.MaxWidth = 200;
            passwordTextBox.Text = "";
            currentTextBox = passwordTextBox;
            passwordSprite.AddUserComponent(passwordTextBox);
            passwordSprite.AddUserComponent(new TextureString(device, "", 18, true, PPDColors.White)
            {
                Position = new SharpDX.Vector2(400, 300)
            });
            passwordSprite.LostFocused += slideSprite_LostFocused;
            passwordSprite.Inputed += slideSprite_Inputed;
            allSlideSprite.AddChild(passwordSprite);
            focusManager.Focus(passwordSprite);
            focusManager.Focus(passwordTextBox);
        }

        void passwordTextBox_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            oneFramePassedAfterRemoveFocus = false;
            if (sender is DxTextBox textBox)
            {
                var errorString = textBox.Parent[2] as TextureString;

                textBox.Parent.RemoveChild(textBox);
                password = textBox.Text;
                generatePanel = NextPanel.PortToConfrim;
            }
        }

        void roomNameTextBox_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            oneFramePassedAfterRemoveFocus = false;
            if (sender is DxTextBox textBox)
            {
                var errorString = textBox.Parent[2] as TextureString;
                try
                {
                    if (!CheckRoomName(textBox.Text))
                    {
                        throw new ArgumentException();
                    }

                    textBox.Parent.RemoveChild(textBox);
                    roomName = textBox.Text;
                    generatePanel = NextPanel.Password;
                }
                catch
                {
                    errorString.Text = Utility.Language["InvalidRoomName"];
                    if (textBox.Text != "")
                    {
                        shouldFocusTextBox = textBox;
                    }
                }
            }
        }

        void ipTextBox_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            oneFramePassedAfterRemoveFocus = false;
            if (sender is DxTextBox textBox)
            {
                var errorString = textBox.Parent[2] as TextureString;
                try
                {
                    if (!CheckIPText(textBox.Text))
                    {
                        throw new ArgumentException();
                    }

                    textBox.Parent.RemoveChild(textBox);
                    ip = textBox.Text;
                    generatePanel = NextPanel.Port;
                }
                catch
                {
                    errorString.Text = Utility.Language["InvalidIP"];
                    if (textBox.Text != "")
                    {
                        shouldFocusTextBox = textBox;
                    }
                }
            }
        }

        private bool CheckRoomName(string roomName)
        {
            return roomName.Trim() != "";
        }

        private bool CheckIPText(string ip)
        {
            var splitIp = ip.Split('.');
            if (splitIp.Length != 4)
            {
                return false;
            }

            foreach (string byteString in splitIp)
            {
                try
                {
                    var num = int.Parse(byteString);
                    if (num < 0 || num > 255)
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        void portTextBox_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            oneFramePassedAfterRemoveFocus = false;
            if (sender is DxTextBox textBox)
            {
                var errorString = textBox.Parent[2] as TextureString;
                try
                {
                    port = int.Parse(textBox.Text);
                    if (port < 0 || port > UInt16.MaxValue)
                    {
                        throw new ArgumentException();
                    }
                    GoNextScene(false);
                }
                catch
                {
                    errorString.Text = Utility.Language["InvalidPort"];
                    if (textBox.Text != "")
                    {
                        shouldFocusTextBox = textBox;
                    }
                }
            }
        }

        void portToConfirmTextBox_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            oneFramePassedAfterRemoveFocus = false;
            if (sender is DxTextBox textBox)
            {
                var errorString = textBox.Parent[2] as TextureString;
                try
                {
                    port = int.Parse(textBox.Text);
                    if (port < 0 || port > UInt16.MaxValue)
                    {
                        throw new ArgumentException();
                    }

                    textBox.Parent.RemoveChild(textBox);
                    generatePanel = NextPanel.Confirm;
                }
                catch
                {
                    errorString.Text = Utility.Language["InvalidPort"];
                    if (textBox.Text != "")
                    {
                        shouldFocusTextBox = textBox;
                    }
                }
            }
        }

        void slideSprite_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            if (sender is SlideSprite slide && !slide.OverFocused)
            {
                currentIndex = 0;
                while (allSlideSprite.ChildrenCount > 1)
                {
                    allSlideSprite.RemoveChild(allSlideSprite[allSlideSprite.ChildrenCount - 1]);
                }
                while (focusManager.CurrentFocusObject != firstSprite)
                {
                    focusManager.RemoveFocus();
                }
            }
        }

        private void GoNextScene(bool createRoom)
        {
            if (!createRoom)
            {
                SceneManager.PrepareNextScene(this, new Menu(device), new Dictionary<string, object>
                {
                    {"AsHost",asHost},                    {"IP",ip},                    {"Port",port}                }, null);
            }
            else
            {
                lastCreatedRoom = new RoomInfo("", roomName, password, port, "", "", 0);
                SaveLastCreatedRoom();
                SceneManager.PrepareNextScene(this, new Menu(device), new Dictionary<string, object>
                {
                    {"AsHost",true},                    {"Port",port},                    {"WebManager",new WebManager()},                    {"RoomInfo",new RoomInfo(PPDFramework.Web.WebManager.Instance.CurrentUserName, roomName,password,port)}                }, null);
            }
        }

        public override void Update(InputInfoBase inputInfo, MouseInfo mouseInfo)
        {
            oneFramePassedAfterRemoveFocus = true;
            if (shouldFocusTextBox != null)
            {
                focusManager.Focus(shouldFocusTextBox);
                shouldFocusTextBox = null;
            }

            switch (generatePanel)
            {
                case NextPanel.Port:
                    currentIndex++;
                    CreatePortPanel(false);
                    break;
                case NextPanel.Password:
                    currentIndex++;
                    CreatePasswordPanel();
                    break;
                case NextPanel.Confirm:
                    currentIndex++;
                    CreateConfirmPanel();
                    break;
                case NextPanel.PortToConfrim:
                    currentIndex++;
                    CreatePortPanel(true);
                    break;
            }
            generatePanel = NextPanel.None;

            allSlideSprite.Position = new SharpDX.Vector2(AnimationUtility.GetAnimationValue(allSlideSprite.Position.X, -currentIndex * 800), allSlideSprite.Position.Y);


            focusManager.ProcessInput(inputInfo);
            UpdateMouseInfo(mouseInfo);

            base.Update();
        }

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

            public bool AutoBack
            {
                get;
                set;
            }

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

                AutoBack = true;
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
                else if (args.InputInfo.IsPressed(ButtonType.Cross) && AutoBack)
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
                //textureString.MouseEnter += new MouseEventHandler(textureString_MouseEnter);
                //textureString.MouseLeftClick += new MouseEventHandler(textureString_MouseLeftClick);
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

            public void ClearUserComponent()
            {
                userSprite.ClearChildren();
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
                    textSprite.Update();
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
                else
                {
                    select.Hidden = true;
                }
            }
        }
    }
}
