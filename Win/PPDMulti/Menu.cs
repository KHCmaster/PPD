using MessagePack;
using PPDCore;
using PPDFramework;
using PPDFrameworkCore;
using PPDMulti.Data;
using PPDMulti.Model;
using PPDMultiCommon.Data;
using PPDMultiCommon.Model;
using PPDMultiCommon.Tcp;
using PPDMultiCommon.Web;
using PPDMultiServer.Tcp;
using PPDShareComponent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PPDMulti
{
    public class Menu : NetworkSceneBase
    {
        enum LastPlayState
        {
            TryToPlayGame = 0,
            GoToPlay,
        }

        LastPlayState lastPlayState;

        const string notifySoundPath = "sounds\\notify.wav";

        BackGroundDisplay bgd;
        UserListComponent userListComponent;

        User selfUser;
        ChatComponent chatComponent;
        DxTextBox textBox;
        LeftMenu leftMenu;
        GameResultComponent gameResultComponent;

        FocusManager focusManager;
        bool textBoxLostFocus;
        bool shouldFocusAgain;
        bool waitingGoToPlay;
        DateTime goToPlayPrepareTime;
        bool fadeOutCalled;

        PPDServer server;
        Timer timer;

        Client client;
        TcpByteReader byteReader;

        Queue<NetworkData> clientHandledData;

        ChangableList<User> userList;
        AllowedModList allowedModList;

        TextureString stateString;

        string version;
        MovieManager movieManager;

        SongInfoEx currentSelectSong;
        PPDScoreInfo ppdScoreInfo;
        bool removeFocusToChatComponent;
        bool canPlayMovie;

        GameRule currentRule = new GameRule();

        Logger logger;

        public Menu(PPDDevice device) : base(device)
        {
            AddProcessor<AddUserNetworkData>(networkData =>
            {
                AddUser(networkData);
            });
            AddProcessor<DeleteUserNetworkData>(networkData =>
            {
                var user = FindUser(networkData.Id);
                DeleteUser(user);
            });
            AddProcessor<AddMessageNetworkData>(networkData =>
            {
                var user = FindUser(networkData.Id);
                if (user != null)
                {
                    NotifyMessageArrive();
                    chatComponent.AddMessage(networkData.Message, user);
                }
            });
            AddProcessor<AddPrivateMessageNetworkData>(networkData =>
            {
                var user = FindUser(networkData.Id);
                if (user != null)
                {
                    NotifyMessageArrive();
                    chatComponent.AddMessage(networkData.Message, user, true);
                }
            });
            AddProcessor<ChangeUserStateNetworkData>(networkData =>
            {
                var user = FindUser(networkData.Id);
                if (user != null)
                {
                    user.CurrentState = networkData.State;
                }
            });
            AddProcessor<GoToPlayNetworkData>(networkData =>
            {
                lock (this)
                {
                    clientHandledData.Clear();
                }
                GoToPlay((GoToPlayNetworkData)networkData);
            });
            AddProcessor<SendServerInfoNetworkData>(networkData =>
            {
                selfUser.ID = networkData.Id;
                allowedModList.AllowedModIds = networkData.AllowedModIds;
                SendScoreList();
            });
            AddProcessor<ChangeSongNetworkData>(networkData =>
            {
                foreach (User user in userList)
                {
                    user.HasSong = false;
                }

                currentSelectSong = new SongInfoEx { Hash = networkData.Hash, Difficulty = networkData.Difficulty };
                if (currentSelectSong.SongInformation != null)
                {
                    var result = currentSelectSong.SongInformation.CalculateDifficulty(currentSelectSong.Difficulty);
                    chatComponent.AddSystemMessage(String.Format(Utility.Language["ScoreChanged"],
                        currentSelectSong.SongInformation.DirectoryName, networkData.Difficulty,
                        String.Format("{0:F2}({1:F2})", result.Average, result.Peak)));
                }
                else
                {
                    ppdScoreInfo = PPDScoreManager.Manager.GetScoreByHash(networkData.Hash);
                    if (ppdScoreInfo == null)
                    {
                        chatComponent.AddSystemMessage(String.Format(Utility.Language["ScoreChanged"], "Unknown Score", networkData.Difficulty, 0));
                    }
                    else
                    {
                        chatComponent.AddSystemMessage(String.Format(Utility.Language["ScoreChanged"], ppdScoreInfo.Title, networkData.Difficulty, 0));
                        chatComponent.AddSystemMessage(Utility.Language["ThisIsRegisteredScore"]);
                    }
                }
                FindSongData();
            });
            AddProcessor<ChangeGameRuleNetworkData>(networkData =>
            {
                ChangeGameRule(networkData.GameRule);
            });
            AddProcessor<CloseConnectNetworkData>(networkData =>
            {
                chatComponent.AddSystemMessage(String.Format(Utility.Language["Disconnected"], Utility.Language[((CloseConnectNetworkData)networkData).Reason.ToString()]));
                client.Close();
                ClearUser();
            });
            AddProcessor<HasSongNetworkData>(networkData =>
            {
                var user = FindUser(networkData.Id);
                if (user != null)
                {
                    user.HasSong = true;
                }
            });
            AddProcessor<ChangeLeaderNetworkData>(networkData =>
            {
                var last = selfUser.IsLeader;
                foreach (var user in userList)
                {
                    user.IsLeader = networkData.UserId == user.ID;
                }
                if (last != selfUser.IsLeader)
                {
                    if (selfUser.IsLeader)
                    {
                        chatComponent.AddSystemMessage(Utility.Language["BecameLeader"]);
                    }
                    else
                    {
                        chatComponent.AddSystemMessage(Utility.Language["BecameNotLeader"]);
                    }
                }
            });
            AddProcessor<PingUserNetworkData>(networkData =>
            {
                var user = FindUser(networkData.Id);
                if (user != null)
                {
                    user.Ping = networkData.Ping;
                }
            });
            AddProcessor<GoToPlayPrepareNetworkData>(networkData =>
            {
                waitingGoToPlay = true;
                fadeOutCalled = false;
                goToPlayPrepareTime = DateTime.Now;
                chatComponent.AddSystemMessage(Utility.Language["StartsAfterFiveMinutes"]);
            });
            AddProcessor<ForceStartNetworkData>(networkData =>
            {
                chatComponent.AddSystemMessage(Utility.Language["ChangedForceStartMode"]);
            });
            AddProcessor<SendScoreListNetworkData>(networkData =>
            {
                leftMenu.CommonSongs = networkData.SongInfos;
            });
            AddProcessor<KickUserNetworkData>(networkData =>
            {
                var user = FindUser(networkData.UserId);
                KickUser(user);
            });
        }

        public override void SceneStackPoped(Dictionary<string, object> param)
        {
            switch (lastPlayState)
            {
                case LastPlayState.TryToPlayGame:
                    selfUser.CurrentState = UserState.NotReady;
                    client.Write(MessagePackSerializer.Serialize(new ChangeUserStateNetworkData { State = selfUser.CurrentState }));
                    break;
                case LastPlayState.GoToPlay:
                    while (focusManager.CurrentFocusObject != chatComponent)
                    {
                        focusManager.RemoveFocus();
                    }
                    gameResultComponent.ClearResult();

                    if (param != null && param.ContainsKey("Result") && param["Result"] != null)
                    {
                        var results = (param["Result"] as Tuple<int, Result>[])
                            .Select(t => new UserResult { User = FindUser(t.Item1), Result = t.Item2 })
                            .Where(r => r.User != null).ToArray();
                        focusManager.Focus(gameResultComponent);

                        Array.Sort(results, CompareResult);
                        foreach (var result in results)
                        {
                            gameResultComponent.AddResult(result);
                        }

                        logger.AddResult(currentRule, currentSelectSong.SongInformation, results);
                    }
                    else
                    {
                        chatComponent.AddSystemMessage(Utility.Language["HostStopGame"]);
                    }
                    break;
            }

            waitingGoToPlay = false;
            movieManager.Change(true);

            base.SceneStackPoped(param);
        }

        private int CompareResult(UserResult r1, UserResult r2)
        {
            int ret = 0;
            var result1 = r1.Result;
            var result2 = r2.Result;
            switch (currentRule.ResultSortType)
            {
                case ResultSortType.Score:
                    ret = result1.Score - result2.Score;
                    if (ret == 0)
                    {
                        ret = result1.CoolCount + result1.GoodCount - result2.CoolCount - result2.GoodCount;
                    }
                    if (ret == 0)
                    {
                        ret = result1.SafeCount - result2.SafeCount;
                    }
                    if (ret == 0)
                    {
                        ret = result1.SadCount - result2.SadCount;
                    }
                    if (ret == 0)
                    {
                        ret = result1.WorstCount - result2.WorstCount;
                    }
                    if (ret == 0)
                    {
                        ret = result1.MaxCombo - result2.MaxCombo;
                    }
                    break;
                case ResultSortType.Accuracy:
                    ret = result1.CoolCount - result2.CoolCount;
                    if (ret == 0)
                    {
                        ret = result1.GoodCount - result2.GoodCount;
                    }
                    if (ret == 0)
                    {
                        ret = result1.SafeCount - result2.SafeCount;
                    }
                    if (ret == 0)
                    {
                        ret = result1.SadCount - result2.SadCount;
                    }
                    if (ret == 0)
                    {
                        ret = result1.WorstCount - result1.WorstCount;
                    }
                    if (ret == 0)
                    {
                        ret = result1.Score - result2.Score;
                    }
                    if (ret == 0)
                    {
                        ret = result1.MaxCombo - result2.MaxCombo;
                    }
                    break;
            }

            return ret * -1;
        }

        public override bool Load()
        {
            PPDFramework.Web.WebModInfo[] webMods = null;
            Action[] actions = {
                () => { webMods = PPDFramework.Web.WebManager.Instance.GetMods(); },
                PPDScoreManager.Manager.Initialize,
            };
            Parallel.ForEach(actions, (action) => action());
            allowedModList = new AllowedModList
            {
                WebMods = webMods
            };

            logger = new Logger(String.Format("{0}.txt", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")));
            Sound.AddSound(notifySoundPath);

            version = FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(this.GetType()).Location).FileVersion;

            userList = new ChangableList<User>();
            userList.ItemChanged += userList_ItemChanged;

            movieManager = new MovieManager(device, GameHost);
            movieManager.MovieChanged += MovieManager_MovieChanged;
            movieManager.MovieChangeFailed += MovieManager_MovieChangeFailed;

            clientHandledData = new Queue<NetworkData>();

            selfUser = new User
            {
                Name = PPDFramework.Web.WebManager.Instance.CurrentUserName,
                AccountId = PPDFramework.Web.WebManager.Instance.CurrentAccountId,
                CurrentState = UserState.NotReady,
                Color = RandomColorGenerator.GetColor(),
                IsSelf = true,
                IsHost = (bool)this.Param["AsHost"]
            };

            this.AddChild(gameResultComponent = new GameResultComponent(device, GameHost, ResourceManager, Sound));
            this.AddChild(leftMenu = new LeftMenu(device, GameHost, ResourceManager, Sound, movieManager, selfUser, userList, allowedModList));
            leftMenu.SongSelected += leftMenu_SongSelected;
            leftMenu.ShowResult += leftMenu_ShowResult;
            leftMenu.UpdateScoreDB += leftMenu_UpdateScoreDB;
            leftMenu.RuleChanged += leftMenu_RuleChanged;
            leftMenu.TryToPlayGame += leftMenu_TryToPlayGame;
            leftMenu.ChangeLeader += leftMenu_ChangeLeader;
            leftMenu.KickUser += leftMenu_KickUser;
            this.AddChild(textBox = new DxTextBox(device, GameHost, ResourceManager));

            this.AddChild((chatComponent = new ChatComponent(device, ResourceManager, GameHost) { Position = new SharpDX.Vector2(430, 0) }));
            this.AddChild(userListComponent = new UserListComponent(device, ResourceManager)
            {
                Position = new SharpDX.Vector2(10, 10)
            });

            PictureObject bottom;
            this.AddChild(bottom = new PictureObject(device, ResourceManager, Utility.Path.Combine("menu_bottom.png"))
            {
                Position = new SharpDX.Vector2(0, 421)
            });
            bottom.AddChild(new PictureObject(device, ResourceManager, Utility.Path.Combine("bottom_triangle.png"))
            {
                Position = new SharpDX.Vector2(7, 7)
            });
            bottom.AddChild(new TextureString(device, Utility.Language["Menu"], 16, PPDColors.White)
            {
                Position = new SharpDX.Vector2(30, 5)
            });
            bottom.AddChild(new PictureObject(device, ResourceManager, Utility.Path.Combine("bottom_circle.png"))
            {
                Position = new SharpDX.Vector2(157, 7)
            });
            bottom.AddChild(stateString = new TextureString(device, Utility.Language["ChangeReadyState"], 16, PPDColors.White)
            {
                Position = new SharpDX.Vector2(180, 5)
            });

            this.AddChild((bgd = new BackGroundDisplay(device, ResourceManager, "skins\\PPDMulti_BackGround.xml", "Menu")));

            focusManager = new FocusManager();
            focusManager.Focus(chatComponent);

            userList.Add(selfUser);

            chatComponent.Inputed += chatComponent_Inputed;
            textBox.LostFocused += textBox_LostFocused;
            leftMenu.Closed += leftMenu_Closed;

            if (selfUser.IsHost)
            {
                WebManager webManager = null;
                if (this.Param.ContainsKey("WebManager"))
                {
                    webManager = this.Param["WebManager"] as WebManager;
                }
                RoomInfo roomInfo = null;
                if (this.Param.ContainsKey("RoomInfo"))
                {
                    roomInfo = this.Param["RoomInfo"] as RoomInfo;

                }
                server = new PPDServer((int)Param["Port"], webManager, roomInfo, GameHost);
                server.FailedToCreateRoom += server_FailedToCreateRoom;
                client = new Client
                {
                    Address = "127.0.0.1",
                    Port = (int)Param["Port"]
                };
                timer = new Timer(state =>
                {
                    server.Update();
                }, null, 0, 1);
            }
            else
            {
                client = new Client
                {
                    Address = (string)Param["IP"],
                    Port = (int)Param["Port"]
                };
            }

            client.Closed += client_Closed;
            client.Read += client_Read;
            byteReader = new TcpByteReader();
            byteReader.ByteReaded += TcpByteReader_ByteReaded;

            if (server != null)
            {
                server.Start();
            }
            client.Start();

            if (client.HasConnection)
            {
                // send login data
                client.Write(MessagePackSerializer.Serialize(new AddUserNetworkData
                {
                    UserName = selfUser.Name,
                    AccountId = selfUser.AccountId,
                    State = selfUser.CurrentState,
                    Version = version
                }));
            }
            else
            {
                chatComponent.AddSystemMessage(Utility.Language["CannotConnectToHost"]);
            }

            // load
            SongInformation.All.ToArray();
            return true;
        }

        private void MovieManager_MovieChanged(object sender, EventArgs e)
        {
            canPlayMovie = true;
        }

        private void MovieManager_MovieChangeFailed(object sender, EventArgs e)
        {
            GameHost.AddNotify(PPDExceptionContentProvider.Provider.GetContent(PPDExceptionType.CannotOpenMovie));
            selfUser.CurrentState = UserState.NotReady;
            canPlayMovie = false;
            chatComponent.AddSystemMessage(Utility.Language["CannotPlayMovie"]);
        }

        void leftMenu_KickUser(User obj)
        {
            removeFocusToChatComponent = true;
            if (obj == null)
            {
                return;
            }

            if (selfUser.IsLeader)
            {
                client.Write(MessagePackSerializer.Serialize(new KickUserNetworkData { UserId = obj.ID }));
            }
        }

        void leftMenu_ChangeLeader(User obj)
        {
            removeFocusToChatComponent = true;
            if (obj == null)
            {
                return;
            }

            if (selfUser.IsLeader)
            {
                client.Write(MessagePackSerializer.Serialize(new ChangeLeaderNetworkData { UserId = obj.ID }));
            }
        }

        void server_FailedToCreateRoom(object sender, EventArgs e)
        {
            chatComponent.AddSystemMessage(Utility.Language["FailedToCreateRoom"]);
        }

        void leftMenu_RuleChanged(GameRule obj)
        {
            removeFocusToChatComponent = true;
            client.Write(MessagePackSerializer.Serialize(new ChangeGameRuleNetworkData { GameRule = obj }));
        }

        private void ChangeGameRule(GameRule gameRule)
        {
            currentRule = gameRule;
            chatComponent.AddSystemMessage(String.Format(Utility.Language["RuleChanged"],
                (gameRule.ItemAvailable ? Utility.Language["Yes"] : Utility.Language["No"]),
                Utility.Language[gameRule.ResultSortType.ToString()]));
            if (currentRule.ItemAvailable)
            {
                var rules = new List<string>
                {
                    String.Format(Utility.Language["ItemSupplyType"], currentRule.ItemSupplyType == ItemSupplyType.ComboWorstCount ?
                        Utility.Language["DependentOnComboOrWorst"] : Utility.Language["DependentOnRank"]),
                    String.Format("ItemMaxCount:{0}", currentRule.MaxItemCount)
                };
                if (currentRule.ItemSupplyType == ItemSupplyType.ComboWorstCount)
                {
                    rules.Add(String.Format("Combo:{0}", currentRule.ItemSupplyComboCount));
                    rules.Add(String.Format("Worst:{0}", currentRule.ItemSupplyWorstCount));
                }
                chatComponent.AddSystemMessage(String.Join(", ", rules.ToArray()));
            }
        }

        void TcpByteReader_ByteReaded(ReadInfo readInfo, int id)
        {
            if (readInfo.NetworkData is PingNetworkData)
            {
                var pingNetworkData = readInfo.NetworkData as PingNetworkData;
                client.Write(MessagePackSerializer.Serialize(new PingNetworkData { Time = pingNetworkData.Time }));
            }
            lock (this)
            {
                clientHandledData.Enqueue(readInfo.NetworkData);
            }
        }

        void leftMenu_TryToPlayGame(object sender, EventArgs e)
        {
            if (currentSelectSong == null || currentSelectSong.SongInformation == null)
            {
                return;
            }

            if (waitingGoToPlay)
            {
                return;
            }

            PlayGame();
        }

        void leftMenu_UpdateScoreDB(object sender, EventArgs e)
        {
            focusManager.RemoveFocus();
            SongInformation.Update();
            if (currentSelectSong != null && currentSelectSong.SongInformation == null)
            {
                FindSongData();
            }

            chatComponent.AddSystemMessage(Utility.Language["ClientDBUpdated"]);
            SendScoreList();
        }

        void leftMenu_ShowResult(object sender, EventArgs e)
        {
            focusManager.RemoveFocus();
            focusManager.Focus(gameResultComponent);
        }

        void leftMenu_SongSelected(object sender, EventArgs e)
        {
            removeFocusToChatComponent = true;
            client.Write(MessagePackSerializer.Serialize(new ChangeSongNetworkData
            {
                Hash = leftMenu.SongInformation.GetScoreHash(leftMenu.Difficulty),
                Difficulty = leftMenu.Difficulty
            }));
        }

        void leftMenu_Closed(object sender, EventArgs e)
        {
            SceneManager.PrepareNextScene(this, new IPSelectScene(device), null, null);
        }

        void userList_ItemChanged(User[] addedItems, User[] removedItems)
        {
            foreach (User user in addedItems)
            {
                userListComponent.AddUser(user);
                ThreadManager.Instance.GetThread(() =>
                {
                    try
                    {
                        user.UpdateImagePath();
                    }
                    catch
                    {
                    }
                }).Start();
            }

            foreach (User user in removedItems)
            {
                userListComponent.RemoveUser(user);
            }
        }

        void client_Read(byte[] data)
        {
            byteReader.Read(data, 0);
        }

        void client_Closed()
        {
            var networkData = new CloseConnectNetworkData { Reason = CloseConnectReason.HostCloseTcp };

            lock (this)
            {
                clientHandledData.Enqueue(networkData);
            }
        }

        User FindUser(int id)
        {
            var user = userList.Find((tempUser) => tempUser.ID == id);
            return user;
        }

        void textBox_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            if (GameHost.TextBoxEnterClosed && textBox.Text != "")
            {
                AddMessage(textBox.Text);
                textBox.Text = "";
                shouldFocusAgain = true;
            }
            textBoxLostFocus = true;
        }

        private void ProcessCommand(string text)
        {
            var splits = text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            var command = splits[0].Substring(1);
            string[] args = new string[splits.Length - 1];
            Array.Copy(splits, 1, args, 0, args.Length);
            switch (command)
            {
                case "open":
                    if (args.Length > 0 && args[0] == "score")
                    {
                        if (ppdScoreInfo == null && currentSelectSong != null)
                        {
                            ppdScoreInfo = PPDScoreManager.Manager.GetScoreByHash(currentSelectSong.Hash);
                        }

                        if (ppdScoreInfo != null)
                        {
                            try
                            {
                                ThreadManager.Instance.GetThread(() =>
                                {
                                    System.Diagnostics.Process.Start(ppdScoreInfo.CreateUrl());
                                }).Start();
                            }
                            catch
                            {
                            }
                        }
                    }
                    break;
                case "force":
                    if (args.Length > 0 && args[0] == "start")
                    {
                        client.Write(MessagePackSerializer.Serialize(new ForceStartNetworkData()));
                    }
                    break;
                case "kick":
                    if (args.Length > 0)
                    {
                        var user = userList.Find(u => u.AccountId == args[0]);
                        if (user != null && selfUser.IsLeader)
                        {
                            client.Write(MessagePackSerializer.Serialize(new KickUserNetworkData { UserId = user.ID }));
                        }
                    }
                    else
                    {
                        chatComponent.AddSystemMessage(Utility.Language["KickCommandUsage"]);
                    }
                    break;
                case "users":
                    var users = String.Join(", ", userList.Select(s => String.Format("@{0}", s.AccountId)).ToArray());
                    chatComponent.AddSystemMessage(users);
                    break;
                case "user":
                    if (args.Length > 0)
                    {
                        var user = userList.Find(u => u.Name == args[0]);
                        if (user != null)
                        {
                            chatComponent.AddSystemMessage(String.Format("{0} (ID: {1})", user.Name, user.AccountId));
                        }
                    }
                    else
                    {
                        chatComponent.AddSystemMessage(Utility.Language["UserCommandUsage"]);
                    }
                    break;
                default:
                    if (command.Length > 0)
                    {
                        chatComponent.AddSystemMessage(Utility.Language["NotExistCommand"]);
                    }

                    var commands = new List<string>
                    {
                        ":open score"
                    };
                    if (selfUser.IsLeader)
                    {
                        commands.Add(":force start");
                    }
                    commands.Add(":kick");
                    commands.Add(":users");
                    commands.Add(":user");
                    chatComponent.AddSystemMessage(String.Format(Utility.Language["AvailableCommands"],
                       String.Join(", ", commands.ToArray())));
                    break;
            }
        }

        private void ProcessPrivateMessage(string text)
        {
            var split = text.Split(new string[] { " " }, StringSplitOptions.None);
            var target = split[0].Substring(1);
            var message = string.Join(" ", split, 1, split.Length - 1);
            var user = userList.Find(u => u.AccountId == target);
            if (user != null)
            {
                client.Write(MessagePackSerializer.Serialize(new AddPrivateMessageNetworkData { Message = message, UserId = user.ID }));
                chatComponent.AddMessage(message, user, true);
            }
            else
            {
                chatComponent.AddSystemMessage(Utility.Language["NotExistUser"]);
                chatComponent.AddSystemMessage(Utility.Language["TryUserCommand"]);
            }
        }

        private void AddMessage(string text)
        {
            if (text.StartsWith(":", StringComparison.Ordinal))
            {
                ProcessCommand(text);
            }
            else if (text.StartsWith("@", StringComparison.Ordinal))
            {
                ProcessPrivateMessage(text);
            }
            else
            {
                chatComponent.AddMessage(textBox.Text, selfUser);
                client.Write(MessagePackSerializer.Serialize(new AddMessageNetworkData { Message = text }));
            }
        }

        void chatComponent_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Start))
            {
                if (!textBoxLostFocus)
                {
                    textBox.TextColor = PPDColors.Black;
                    textBox.TextBoxHeight = 12;
                    textBox.MaxWidth = textBox.TextBoxWidth = 340;
                    textBox.DrawMode = DxTextBox.DrawingMode.DrawCaret | DxTextBox.DrawingMode.DrawSelection;
                    textBox.Position = new SharpDX.Vector2(445, 390);
                    textBox.Text = "";
                    focusManager.Focus(textBox);
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                if (canPlayMovie)
                {
                    Sound.Play(PPDSetting.DefaultSounds[3], -1000);
                    selfUser.CurrentState = selfUser.CurrentState != UserState.NotReady ? UserState.NotReady : UserState.Ready;
                    client.Write(MessagePackSerializer.Serialize(new ChangeUserStateNetworkData { State = selfUser.CurrentState }));
                    if (selfUser.IsLeader && selfUser.CurrentState == UserState.Ready && currentSelectSong != null)
                    {
                        foreach (User user in userList)
                        {
                            if (!user.HasSong)
                            {
                                chatComponent.AddSystemMessage(Utility.Language["SuggestForcePlay"]);
                                break;
                            }
                        }
                    }
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Triangle))
            {
                focusManager.Focus(leftMenu);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                focusManager.Focus(gameResultComponent);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                chatComponent.ScrollUp();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                chatComponent.ScrollDown();
            }
        }

        public override void Update(InputInfoBase inputInfo, MouseInfo mouseInfo)
        {
            inputInfo = new MenuInputInfo(inputInfo);
            if (removeFocusToChatComponent)
            {
                removeFocusToChatComponent = false;
                while (focusManager.CurrentFocusObject != chatComponent)
                {
                    focusManager.RemoveFocus();
                }
            }

            ProcessNetworkData();
            focusManager.ProcessInput(inputInfo);
            if (movieManager.Movie != null)
            {
                movieManager.Movie.Update();
            }
            if (waitingGoToPlay && !fadeOutCalled && DateTime.Now - goToPlayPrepareTime >= TimeSpan.FromSeconds(4.8))
            {
                movieManager.FadeOut(0.5f);
            }
            base.Update();

            textBoxLostFocus = false;
            if (shouldFocusAgain)
            {
                shouldFocusAgain = false;
                focusManager.Focus(textBox);
            }
        }

        private void PlayGame()
        {
            selfUser.CurrentState = UserState.Playing;
            client.Write(MessagePackSerializer.Serialize(new ChangeUserStateNetworkData { State = selfUser.CurrentState }));
            lastPlayState = LastPlayState.TryToPlayGame;
            var gameutility = new PPDGameUtility
            {
                SongInformation = currentSelectSong.SongInformation,
                Difficulty = currentSelectSong.Difficulty,
                DifficultString = currentSelectSong.SongInformation.GetDifficultyString(currentSelectSong.Difficulty),
                Profile = ProfileManager.Instance.Profiles[1],
                AutoMode = AutoMode.None,
                SpeedScale = 1,
                MuteSE = leftMenu.MuteSE,
                Connect = leftMenu.Connect
            };
            var dic = new Dictionary<string, object>
            {
                { "PPDGameUtility", gameutility },
                { "GameInterface", new GameInterface(device) },
                { "GameResult", null },
                { "PauseMenu", new PPDShareComponent.PauseMenu(device, Utility.Path) },
                { "MarkImagePath", new MarkImagePaths() }
            };
            SceneManager.PrepareNextScene(this, new PPDCore.MainGame(device), dic, dic, true);

            movieManager.Stop();
        }

        private void GoToPlay(GoToPlayNetworkData goToPlayNetworkData)
        {
            lastPlayState = LastPlayState.GoToPlay;
            var playUserList = new List<User>();
            foreach (User user in userList)
            {
                if (goToPlayNetworkData.PlayerIds.Contains(user.ID))
                {
                    playUserList.Add(user);
                    user.CurrentState = UserState.NotReady;
                }
            }
            SceneManager.PrepareNextScene(this, new MainGame(device), new Dictionary<string, object>
            {
                {"ByteReader", byteReader},
                { "Client",client},
                { "SongInformation",currentSelectSong.SongInformation},
                { "Difficulty",currentSelectSong.Difficulty},
                { "Users",playUserList.ToArray()},
                { "AllowedModList", allowedModList },
                { "Self",selfUser},
                { "MuteSE",leftMenu.MuteSE},
                { "Connect",leftMenu.Connect},
                { "GameRule",currentRule}
            }, null, true);
            movieManager.Stop();
        }

        private void SendScoreList()
        {
            client.Write(MessagePackSerializer.Serialize(new SendScoreListNetworkData
            {
                SongInfos = SongInformation.All.SelectMany(s =>
                {
                    var ret = new List<SongInfo>();
                    if (s.Difficulty.HasFlag(PPDFramework.SongInformation.AvailableDifficulty.Easy))
                    {
                        ret.Add(new SongInfo { Hash = s.EasyHash, Difficulty = Difficulty.Easy });
                    }
                    if (s.Difficulty.HasFlag(PPDFramework.SongInformation.AvailableDifficulty.Normal))
                    {
                        ret.Add(new SongInfo { Hash = s.NormalHash, Difficulty = Difficulty.Normal });
                    }
                    if (s.Difficulty.HasFlag(PPDFramework.SongInformation.AvailableDifficulty.Hard))
                    {
                        ret.Add(new SongInfo { Hash = s.HardHash, Difficulty = Difficulty.Hard });
                    }
                    if (s.Difficulty.HasFlag(PPDFramework.SongInformation.AvailableDifficulty.Extreme))
                    {
                        ret.Add(new SongInfo { Hash = s.ExtremeHash, Difficulty = Difficulty.Extreme });
                    }
                    return ret;
                }).ToArray()
            }));
        }

        private void ProcessNetworkData()
        {
            while (clientHandledData.Count > 0)
            {
                NetworkData networkData = null;
                lock (this)
                {
                    networkData = clientHandledData.Dequeue();
                }
                Process(networkData);
            }
        }

        private void NotifyMessageArrive()
        {
            if (!GameHost.IsWindowActive)
            {
                Sound.Play(notifySoundPath, -1000);
            }
        }

        private void FindSongData()
        {
            if (currentSelectSong.SongInformation == null)
            {
                chatComponent.AddSystemMessage(String.Format(Utility.Language["NoSong"]));
            }
            if (currentSelectSong.SongInformation != null)
            {
                selfUser.HasSong = true;
                client.Write(MessagePackSerializer.Serialize(new HasSongNetworkData()));
            }
            movieManager.Change(currentSelectSong.SongInformation, false);
        }

        private User AddUser(AddUserNetworkData addUserNetworkData)
        {
            var user = new User
            {
                AccountId = addUserNetworkData.AccountId,
                Name = addUserNetworkData.UserName,
                CurrentState = addUserNetworkData.State,
                Color = RandomColorGenerator.GetColor(),
                IsSelf = false,
                ID = addUserNetworkData.Id
            };

            userList.Add(user);
            chatComponent.AddSystemMessage(String.Format(Utility.Language["UserEnterRoom"], user.Name));

            return user;
        }

        private void DeleteUser(User user)
        {
            if (user != null)
            {
                userList.Remove(user);
                chatComponent.AddSystemMessage(String.Format(Utility.Language["UserLeaveRoom"], user.Name));
            }
        }

        private void ClearUser()
        {
            while (userList.Count > 1)
            {
                userList.RemoveAt(1);
            }
        }

        private void KickUser(User user)
        {
            if (user != null)
            {
                userList.Remove(user);
                chatComponent.AddSystemMessage(String.Format(Utility.Language["UserKicked"], user.Name));
            }
        }

        protected override void DisposeResource()
        {
            Sound.DeleteSound(notifySoundPath);
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
            if (server != null)
            {
                server.Close();
                server = null;
            }
            client.Close();
            client.Closed -= client_Closed;
            client.Read -= client_Read;
            movieManager.Stop();
            base.DisposeResource();
        }
    }
}
