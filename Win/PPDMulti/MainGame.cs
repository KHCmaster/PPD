using MessagePack;
using PPDCore;
using PPDCoreModel.Data;
using PPDExpansionCore;
using PPDFramework;
using PPDFrameworkCore;
using PPDMulti.Data;
using PPDMulti.Model;
using PPDMultiCommon.Data;
using PPDMultiCommon.Model;
using PPDMultiCommon.Tcp;
using PPDShareComponent;
using System;
using System.Collections.Generic;

namespace PPDMulti
{
    class MainGame : NetworkSceneBase
    {
        enum FadeOutAction
        {
            None,
            SendNetworkData,
            WaitFinish
        }

        Client client;
        PPDExpansionCore.Tcp.Client expansionClient;
        TcpByteReader byteReader;

        ChangableList<User> userList;
        ChangableList<UserPlayState> userPlayStateList;
        User selfUser;
        UserPlayState selfPlayState;
        UserScoreListComponent userScoreListComponent;
        ItemManagerComponent itemManagerComponent;
        ItemOverrayComponent itemOverrayComponent;

        MainGameComponent mainGameComponent;
        MainGameConfig config;
        GameRule gameRule;
        SpriteObject filterSprite;
        PauseMenu pauseMenu;
        RectangleComponent black;
        bool fadeOut;
        bool ready;
        FadeOutAction fadeOutAction = FadeOutAction.None;

        Queue<NetworkData> clientHandledData;

        bool paused;
        int lastStartPressCount;
        int waitFinishCount;
        int lastComboCount;
        int lastLife;
        int lastScore;
        SharpDX.Vector2 lastMarkPos;
        Random r = new Random();
        PPDGameUtility gameutility;

        List<GameComponent> shouldDisposeItem = new List<GameComponent>();

        public override string SpriteDir
        {
            get { return Utility.MainGamePath.BaseDir; }
        }

        public MainGame(PPDDevice device) : base(device)
        {
            AddProcessor<CloseNetworkData>(networkData =>
            {
                lock (this)
                {
                    clientHandledData.Clear();
                    SceneManager.PopCurrentScene(null);
                }
            });
            AddProcessor<DeleteUserNetworkData>(networkData =>
            {
                var user = FindUser(networkData.Id);
                if (user != null)
                {
                    userList.Remove(user);
                }
                var userPlayState = FindUserPlayState(networkData.Id);
                if (userPlayState != null)
                {
                    userPlayStateList.Remove(userPlayState);
                }
            });
            AddProcessor<PlayMainGameNetworkData>(networkData =>
            {
                foreach (UserPlayState userPlayState in userPlayStateList)
                {
                    userPlayState.Loaded = true;
                }
                mainGameComponent.Play();
                ready = true;
            });
            AddProcessor<ChangeScoreNetworkData>(networkData =>
            {
                var userPlayState = FindUserPlayState(networkData.Id);
                if (userPlayState != null)
                {
                    userPlayState.Score = networkData.Score;
                }
            });
            AddProcessor<ChangeLifeNetworkData>(networkData =>
            {
                var userPlayState = FindUserPlayState(networkData.Id);
                if (userPlayState != null)
                {
                    userPlayState.Life = networkData.Life;
                }
            });
            AddProcessor<ChangeEvaluateNetworkData>(networkData =>
            {
                var userPlayState = FindUserPlayState(networkData.Id);
                if (userPlayState != null)
                {
                    userPlayState.Evaluate = networkData.Evaluate;
                }
            });
            AddProcessor<GoToMenuNetworkData>(networkData =>
            {
                GoToMenu(networkData.Results);
            });
            AddProcessor<JustGoToMenuNetworkData>(networkData =>
            {
                lock (this)
                {
                    clientHandledData.Clear();
                }
                SceneManager.PopCurrentScene(null);
            });
            AddProcessor<AddEffectToPlayerNetworkData>(networkData =>
            {
                AddEffect(networkData.ItemType, networkData.Id);
            });
            AddProcessor<MainGameLoadedNetworkData>(networkData =>
            {
                var userPlayState = FindUserPlayState(networkData.Id);
                if (userPlayState != null)
                {
                    userPlayState.Loaded = true;
                }
            });
        }

        public override bool Load()
        {
            clientHandledData = new Queue<NetworkData>();

            client = Param["Client"] as Client;
            byteReader = Param["ByteReader"] as TcpByteReader;
            client.Closed += client_Closed;
            byteReader.ByteReaded += TcpByteReader_ByteReaded;

            gameRule = Param["GameRule"] as GameRule;
            userList = new ChangableList<User>(Param["Users"] as User[]);
            var songInformation = Param["SongInformation"] as SongInformation;
            var difficulty = (Difficulty)Param["Difficulty"];
            var allowedModList = (AllowedModList)Param["AllowedModList"];
            selfUser = Param["Self"] as User;

            userPlayStateList = new ChangableList<UserPlayState>();
            userPlayStateList.ItemChanged += userPlayStateList_ItemChanged;
            userScoreListComponent = new UserScoreListComponent(device, ResourceManager)
            {
                Position = new SharpDX.Vector2(680, 45)
            };

            itemManagerComponent = new ItemManagerComponent(device, ResourceManager, gameRule)
            {
                Position = new SharpDX.Vector2(682, 420)
            };

            itemOverrayComponent = new ItemOverrayComponent(device, ResourceManager, itemManagerComponent);
            itemOverrayComponent.ItemSet += itemOverrayComponent_ItemSet;

            selfPlayState = new UserPlayState { User = selfUser };
            foreach (User user in userList)
            {
                var userPlayState = new UserPlayState { User = user };
                if (user == selfUser)
                {
                    userScoreListComponent.AddSelfUser(selfPlayState, itemManagerComponent);
                }
                else
                {
                    userPlayStateList.Add(userPlayState);
                    userScoreListComponent.AddUser(userPlayState);
                }
            }
            userScoreListComponent.AddFinish();

            black = new RectangleComponent(device, ResourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0,
                Hidden = true
            };

            // メインゲーム用のパラメータの準備
            gameutility = new PPDGameUtility
            {
                SongInformation = songInformation,
                Difficulty = difficulty,
                DifficultString = songInformation.GetDifficultyString(difficulty),
                Profile = ProfileManager.Instance.Default,
                AutoMode = AutoMode.None,
                SpeedScale = 1,
                Random = false,
                MuteSE = (bool)Param["MuteSE"],
                Connect = (bool)Param["Connect"],
                IsDebug = true,
                GodMode = true,
                CanApplyModCallback = m => allowedModList.IsAllowed(m.FileHashString) || !m.ContainsModifyData
            };

            GameInterfaceBase cgi = new GameInterface(device)
            {
                Sound = Sound,
                PPDGameUtility = gameutility,
                ResourceManager = ResourceManager
            };
            cgi.Load();

            pauseMenu = null;
            if (selfUser.IsLeader)
            {
                pauseMenu = new PauseMenu(device)
                {
                    Sound = Sound,
                    ResourceManager = ResourceManager
                };
                pauseMenu.Load();
                pauseMenu.Resumed += pauseMenu_Resumed;
                pauseMenu.Returned += pauseMenu_Returned;
            }

            config = new MainGameConfig(itemManagerComponent);
            mainGameComponent = new MainGameComponent(device, GameHost, ResourceManager, Sound, this,
                gameutility, cgi, new MarkImagePaths(), null, pauseMenu, config, songInformation.StartTime, songInformation.StartTime)
            {
                PauseMovieWhenPause = false
            };

            filterSprite = new SpriteObject(device);

            mainGameComponent.Finished += mainGameComponent_Finished;
            mainGameComponent.Drawed += mainGameComponent_Drawed;
            mainGameComponent.ScoreChanged += mainGameComponent_ScoreChanged;
            mainGameComponent.LifeChanged += mainGameComponent_LifeChanged;
            mainGameComponent.EvaluateChanged += mainGameComponent_EvaluateChanged;
            mainGameComponent.ComboChanged += mainGameComponent_ComboChanged;

            mainGameComponent.Initialize(fadeOut, fadeOut, new Dictionary<string, object>
            {
                {"MultiItemComponent", itemManagerComponent}
            });

            this.AddChild(black);

            shouldDisposeItem.AddRange(new GameComponent[]{
                userScoreListComponent,
                itemManagerComponent,
                itemOverrayComponent,
                mainGameComponent,
                filterSprite,
                pauseMenu
            });

            ConnectExpansion();

            client.Write(MessagePackSerializer.Serialize(new MainGameLoadedNetworkData()));

            return true;
        }

        private void ConnectExpansion()
        {
            if (PPDExpansionCore.Tcp.Client.IsListening(PPDSetting.Setting.ExpansionWaitPort) && gameutility.SongInformation != null)
            {
                expansionClient = new PPDExpansionCore.Tcp.Client(PPDSetting.Setting.ExpansionWaitPort);
                expansionClient.Start();
                SendScoreInfo();
                SendPlayerInfo();
            }
        }

        private void SendScoreInfo()
        {
            expansionClient.Send(new ScoreInfo
            {
                StartTime = gameutility.SongInformation.StartTime,
                EndTime = gameutility.SongInformation.EndTime,
                PlayType = PlayType.MultiPlay,
                ScoreHash = gameutility.SongInformation.GetScoreHash(gameutility.Difficulty),
                Difficulty = gameutility.Difficulty,
                ScoreName = gameutility.SongInformation.DirectoryName
            });
        }

        private void SendPlayerInfo()
        {
            foreach (var user in userList)
            {
                expansionClient.Send(new PlayerInfo
                {
                    AcccountId = user.AccountId,
                    IsSelf = user == selfUser,
                    PlayerId = user.ID,
                    UserName = user.Name,
                    R = (byte)(user.Color.Red * 255),
                    G = (byte)(user.Color.Green * 255),
                    B = (byte)(user.Color.Blue * 255)
                });
            }
        }

        void itemOverrayComponent_ItemSet()
        {
            itemManagerComponent.AddItem();
        }

        void mainGameComponent_ComboChanged(int obj1, SharpDX.Vector2 obj2)
        {
            if (!gameRule.ItemAvailable)
            {
                return;
            }

            if (gameRule.ItemSupplyType == ItemSupplyType.ComboWorstCount)
            {
                if (obj1 > 0 && (obj1 / gameRule.ItemSupplyComboCount - lastComboCount / gameRule.ItemSupplyComboCount > 0))
                {
                    int count = obj1 / gameRule.ItemSupplyComboCount - lastComboCount / gameRule.ItemSupplyComboCount;
                    for (int i = 0; i < count; i++)
                    {
                        itemOverrayComponent.AddItem(obj2);
                    }
                }
                lastMarkPos = obj2;
                lastComboCount = obj1;
            }
            else if (gameRule.ItemSupplyType == ItemSupplyType.Rank)
            {
                if (obj1 > 0 && obj1 - lastComboCount > 0)
                {
                    for (int j = 0; j < obj1 - lastComboCount; j++)
                    {
                        bool item = false;
                        for (int i = 0; i < (selfPlayState.Rank - 1) * 5; i++)
                        {
                            if (r.Next(0, 500) < 1)
                            {
                                item = true;
                                break;
                            }
                        }
                        if (item)
                        {
                            itemOverrayComponent.AddItem(obj2);
                        }
                    }
                }
                lastComboCount = obj1;
            }
        }

        void pauseMenu_Resumed(object sender, EventArgs e)
        {
            paused = false;
        }

        void pauseMenu_Returned(object sender, EventArgs e)
        {
            paused = false;
            client.Write(MessagePackSerializer.Serialize(new JustGoToMenuNetworkData()));
        }

        void TcpByteReader_ByteReaded(ReadInfo readInfo, int id)
        {
            lock (this)
            {
                clientHandledData.Enqueue(readInfo.NetworkData);
            }
        }

        void userPlayStateList_ItemChanged(UserPlayState[] addedItems, UserPlayState[] removedItems)
        {
            foreach (UserPlayState userPlayState in removedItems)
            {
                userScoreListComponent.DeleteUser(userPlayState);
            }
        }

        void mainGameComponent_EvaluateChanged(MarkEvaluateType obj1, bool obj2)
        {
            if (obj1 == MarkEvaluateType.Cool || obj1 == MarkEvaluateType.Fine)
            {
                selfPlayState.Evaluate = obj2 ? (MarkEvaluateTypeEx)(obj1 + 5) : (MarkEvaluateTypeEx)obj1;
            }
            else
            {
                selfPlayState.Evaluate = (MarkEvaluateTypeEx)obj1;
            }
            client.Write(MessagePackSerializer.Serialize(new ChangeEvaluateNetworkData { Evaluate = selfPlayState.Evaluate }));
        }

        void mainGameComponent_LifeChanged(int obj)
        {
            selfPlayState.Life = obj;
        }

        void mainGameComponent_ScoreChanged(int obj)
        {
            selfPlayState.Score = obj;
        }

        private User FindUser(int id)
        {
            if (selfUser.ID == id)
            {
                return selfUser;
            }

            var user = userList.Find((tempUser) => tempUser.ID == id);
            return user;
        }

        private UserPlayState FindUserPlayState(int id)
        {
            if (selfPlayState.User.ID == id)
            {
                return selfPlayState;
            }

            var userPlayState = userPlayStateList.Find((temp) => temp.User.ID == id);
            return userPlayState;
        }

        void client_Closed()
        {
            var networkData = new CloseNetworkData();

            lock (this)
            {
                clientHandledData.Enqueue(networkData);
            }
        }

        void mainGameComponent_Drawed(LayerType layerType)
        {
            switch (layerType)
            {
                case LayerType.AfterMarkEffect:
                    userScoreListComponent.Draw();
                    break;
                case LayerType.AfterMark:
                    filterSprite.Draw();
                    break;
                case LayerType.AfterInterface:
                    if (gameRule.ItemAvailable)
                    {
                        itemOverrayComponent.Draw();
                        itemManagerComponent.Draw();
                    }

                    if (paused)
                    {
                        black.Hidden = false;
                        black.Alpha = 0.5f;
                        black.Update();
                        black.Draw();
                        black.Hidden = true;
                        black.Alpha = 0;
                        black.Update();
                    }

                    break;
            }
        }

        void mainGameComponent_Finished(object sender, EventArgs e)
        {
            mainGameComponent.MovieFadeOut();
            fadeOutAction = FadeOutAction.SendNetworkData;
            fadeOut = true;
            black.Hidden = false;
            black.Alpha = 0;
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

        private bool IsDisturbEffect(ItemType itemType)
        {
            return itemType == ItemType.DoubleBPM
                || itemType == ItemType.FogFilter
                || itemType == ItemType.HalfScore
                || itemType == ItemType.Hidden
                || itemType == ItemType.StripeFilter
                || itemType == ItemType.Sudden
                || itemType == ItemType.PingPong;
        }

        private void GoToMenu(Tuple<int, Result>[] results)
        {
            lock (this)
            {
                clientHandledData.Clear();
            }
            SceneManager.PopCurrentScene(new Dictionary<string, object>
            {
                {"Result", results}
            });
        }

        public override void Update(InputInfoBase inputInfo, MouseInfo mouseInfo)
        {
            ProcessNetworkData();
            if (fadeOut)
            {
                if (black.Alpha >= 1)
                {
                    black.Alpha = 1;
                    switch (fadeOutAction)
                    {
                        case FadeOutAction.WaitFinish:
                            waitFinishCount++;
                            if (waitFinishCount % 10 == 9)
                            {
                                client.Write(MessagePackSerializer.Serialize(new SendResultNetworkData
                                {
                                    Result = new Result
                                    {
                                        Score = mainGameComponent.Score,
                                        CoolCount = mainGameComponent.CoolCount,
                                        GoodCount = mainGameComponent.GoodCount,
                                        SafeCount = mainGameComponent.SafeCount,
                                        SadCount = mainGameComponent.SadCount,
                                        WorstCount = mainGameComponent.WorstCount,
                                        MaxCombo = mainGameComponent.MaxCombo
                                    }
                                }));
                            }
                            break;
                        case FadeOutAction.SendNetworkData:
                            client.Write(MessagePackSerializer.Serialize(new SendResultNetworkData
                            {
                                Result = new Result
                                {
                                    Score = mainGameComponent.Score,
                                    CoolCount = mainGameComponent.CoolCount,
                                    GoodCount = mainGameComponent.GoodCount,
                                    SafeCount = mainGameComponent.SafeCount,
                                    SadCount = mainGameComponent.SadCount,
                                    WorstCount = mainGameComponent.WorstCount,
                                    MaxCombo = mainGameComponent.MaxCombo
                                }
                            }));
                            fadeOutAction = FadeOutAction.WaitFinish;
                            break;
                    }
                }
                else
                {
                    black.Alpha += 0.05f;
                    if (black.Alpha >= 1)
                    {
                        black.Alpha = 1;
                    }
                }
            }

            userScoreListComponent.Update();
            if (ready)
            {
                if (!paused)
                {
                    int lastWorstCount = mainGameComponent.GameResultManager.WorstCount;
                    mainGameComponent.Update(inputInfo, mouseInfo);

                    if (gameRule.ItemAvailable && gameRule.ItemSupplyType == ItemSupplyType.ComboWorstCount)
                    {
                        int currentWorstCount = mainGameComponent.GameResultManager.WorstCount;
                        int count = currentWorstCount / gameRule.ItemSupplyWorstCount - lastWorstCount / gameRule.ItemSupplyWorstCount;
                        for (int i = 0; i < count; i++)
                        {
                            itemOverrayComponent.AddItem(lastMarkPos);
                        }
                    }
                }
            }

            if (paused)
            {
                if (pauseMenu != null)
                {
                    pauseMenu.Update(inputInfo);
                }
            }
            else
            {
                if (lastStartPressCount >= 60)
                {
                    paused |= pauseMenu != null;
                }

                if (inputInfo.IsReleased(ButtonType.Start) && lastStartPressCount < 60)
                {
                    if (itemManagerComponent.CanUse)
                    {
                        UseItem(itemManagerComponent.Use());
                    }
                }
            }

            foreach (ItemUseEventArgs args in itemManagerComponent.EnumerateItem())
            {
                if (ItemUseManager.Manager.IsAutoUse(args.ItemType))
                {
                    UseItem(args.ItemType);
                    args.Use = true;
                }
            }

            itemManagerComponent.Update();
            itemOverrayComponent.Update();
            config.Update();
            filterSprite.Update();
            base.Update();
            CheckChange();
            lastStartPressCount = inputInfo.GetPressingFrame(ButtonType.Start);
            if (expansionClient != null)
            {
                var currentTime = mainGameComponent.MoviePosition;
                expansionClient.Send(new UpdateInfo
                {
                    Score = mainGameComponent.Score,
                    PlayerId = selfUser.ID,
                    CurrentTime = currentTime,
                    Life = mainGameComponent.GameResultManager.CurrentLife,
                    CoolCount = mainGameComponent.CoolCount,
                    GoodCount = mainGameComponent.GoodCount,
                    SafeCount = mainGameComponent.SafeCount,
                    SadCount = mainGameComponent.SadCount,
                    WorstCount = mainGameComponent.WorstCount,
                    MaxCombo = mainGameComponent.MaxCombo
                });
                foreach (var user in userPlayStateList)
                {
                    expansionClient.Send(new UpdateInfo
                    {
                        Score = user.Score,
                        PlayerId = user.User.ID,
                        CurrentTime = currentTime,
                        Life = user.Life
                    });
                }
            }
        }

        private void CheckChange()
        {
            if (lastLife != selfPlayState.Life)
            {
                client.Write(MessagePackSerializer.Serialize(new ChangeLifeNetworkData { Life = selfPlayState.Life }));
            }
            if (lastScore != selfPlayState.Score)
            {
                client.Write(MessagePackSerializer.Serialize(new ChangeScoreNetworkData { Score = selfPlayState.Score }));
            }
            lastLife = selfPlayState.Life;
            lastScore = selfPlayState.Score;
        }

        private void UseItem(ItemType itemType)
        {
            ItemEffect itemEffect = null;
            if (IsDisturbEffect(itemType) || itemType == ItemType.Stealth)
            {
                if (itemType == ItemType.Stealth)
                {
                    itemEffect = new StealthEffect();
                }

                client.Write(MessagePackSerializer.Serialize(new AddEffectNetworkData { ItemType = itemType }));
            }
            else
            {
                switch (itemType)
                {
                    case ItemType.Auto:
                        itemEffect = new AutoEffect();
                        break;
                    case ItemType.Barrier:
                        itemEffect = new BarrierEffect();
                        break;
                    case ItemType.DoubleCombo:
                        itemEffect = new DoubleComboEffect();
                        break;
                    case ItemType.DoubleScore:
                        itemEffect = new DoubleScoreEffect();
                        break;
                    case ItemType.HatenaBox:
                        var number = r.Next(1, 4);
                        for (int i = 0; i < number; i++)
                        {
                            itemManagerComponent.AddItem();
                        }
                        break;
                    case ItemType.TripleCombo:
                        itemEffect = new TripleComboEffect();
                        break;
                    case ItemType.TripleScore:
                        itemEffect = new TripleScoreEffect();
                        break;
                }
            }

            if (itemEffect != null)
            {
                itemManagerComponent.AddEffect(itemEffect);
            }

            if (expansionClient != null)
            {
                expansionClient.Send(new ItemInfo
                {
                    CurrentTime = mainGameComponent.MoviePosition,
                    ItemType = itemType,
                    PlayerId = selfUser.ID
                });
            }
        }

        private void AddEffect(ItemType itemType, int userID)
        {
            ItemEffect itemEffect = null;

            if (itemManagerComponent.ContainEffect(ItemType.Barrier))
            {
                // Reflect to person
                client.Write(MessagePackSerializer.Serialize(new AddEffectToPlayerNetworkData { UserId = userID, ItemType = itemType }));
            }
            else if (itemManagerComponent.ContainEffect(ItemType.Stealth))
            {
                // Nothing to do
            }
            else
            {
                switch (itemType)
                {
                    case ItemType.DoubleBPM:
                        if (itemManagerComponent.ContainEffect(ItemType.DoubleBPM))
                        {
                            var effect = itemManagerComponent.GetEffect(ItemType.DoubleBPM);
                            effect.AddEffect();
                        }
                        else
                        {
                            itemEffect = new DoubleBPMEffect();
                        }
                        break;
                    case ItemType.FogFilter:
                        if (itemManagerComponent.ContainEffect(ItemType.FogFilter))
                        {
                            var effect = itemManagerComponent.GetEffect(ItemType.FogFilter);
                            effect.AddEffect();
                        }
                        else
                        {
                            var fogComponent = new FogEffectComponent(device, ResourceManager);
                            itemEffect = new FogFilterEffect(fogComponent);
                            filterSprite.AddChild(fogComponent);
                        }
                        break;
                    case ItemType.HalfScore:
                        itemEffect = new HalfScoreEffect();
                        break;
                    case ItemType.Hidden:
                        itemEffect = new HiddenEffect();
                        break;
                    case ItemType.StripeFilter:
                        if (itemManagerComponent.ContainEffect(ItemType.StripeFilter))
                        {
                            var effect = itemManagerComponent.GetEffect(ItemType.StripeFilter);
                            effect.AddEffect();
                        }
                        else
                        {
                            var stripeComponent = new StripeEffectComponent(device, ResourceManager);
                            itemEffect = new StripeFilterEffect(stripeComponent);
                            filterSprite.AddChild(stripeComponent);
                        }
                        break;
                    case ItemType.Sudden:
                        itemEffect = new SuddenEffect();
                        break;
                    case ItemType.PingPong:
                        if (itemManagerComponent.ContainEffect(ItemType.PingPong))
                        {
                            var effect = itemManagerComponent.GetEffect(ItemType.PingPong);
                            effect.AddEffect();
                        }
                        else
                        {
                            var pingPongComponent = new PingPongEffectComponent(device, ResourceManager);
                            itemEffect = new PingPongEffect(pingPongComponent);
                            filterSprite.AddChild(pingPongComponent);
                        }
                        break;
                    case ItemType.Stealth:
                        itemEffect = new StealthEffect();
                        var userPlayState = FindUserPlayState(userID);
                        if (userPlayState != null)
                        {
                            userPlayState.IsStealth = true;
                        }
                        itemEffect.Removed += (sender, e) =>
                        {
                            userPlayState.IsStealth = false;
                        };
                        itemManagerComponent.AddOthersEffect(itemEffect);
                        itemEffect = null;
                        break;
                }
            }

            if (itemEffect != null)
            {
                itemManagerComponent.AddEffect(itemEffect);
            }

            if (expansionClient != null)
            {
                expansionClient.Send(new ItemInfo
                {
                    CurrentTime = mainGameComponent.MoviePosition,
                    ItemType = itemType,
                    PlayerId = userID
                });
            }
        }

        public override void Draw()
        {
            if (ready)
            {
                mainGameComponent.Draw();
            }
            else
            {
                userScoreListComponent.Draw();
            }

            if (paused)
            {
                pauseMenu.Draw();
            }

            base.Draw();
        }

        protected override void DisposeResource()
        {
            client.Closed -= client_Closed;
            byteReader.ByteReaded -= TcpByteReader_ByteReaded;

            if (expansionClient != null)
            {
                expansionClient.Close();
                expansionClient = null;
            }

            foreach (GameComponent gc in shouldDisposeItem)
            {
                if (gc != null)
                {
                    gc.Dispose();
                }
            }
            shouldDisposeItem.Clear();

            base.DisposeResource();
        }
    }
}
