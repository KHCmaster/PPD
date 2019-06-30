using MessagePack;
using PPDFrameworkCore;
using PPDMultiCommon.Data;
using PPDMultiCommon.Model;
using PPDMultiCommon.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace PPDMultiServer.Tcp
{
    class MenuContext : ServerContextBase
    {
        enum State
        {
            None = 0,
            Prepare = 1,
            Moved = 2,
        }
        State state = State.None;

        PPDServer server;
        SongInfo currentSong;
        List<string> kickedIPs;
        List<UserInfo> userList;
        GameRule currentRule;
        string version;
        int updateTimerID;
        int pingTimerID;
        Stopwatch stopWatch;
        long prepareStartTime;
        bool forceStart;

        UserInfo[] playUserList;

        public UserInfo[] UserList
        {
            get
            {
                return playUserList;
            }
        }

        public UserInfo Leader
        {
            get
            {
                return userList.FirstOrDefault(u => u.IsLeader);
            }
        }

        public MenuContext(ServerContextBase previousContext, PPDServer server)
            : base(previousContext)
        {
            this.server = server;
            version = FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(this.GetType()).Location).FileVersion;

            kickedIPs = new List<string>();
            userList = new List<UserInfo>();
            currentRule = new GameRule();

            stopWatch = new Stopwatch();
            stopWatch.Start();

            AddProcessor<AddUserNetworkData>(networkData =>
            {
                if (networkData.Version != version)
                {
                    Host.Write(MessagePackSerializer.Serialize(new CloseConnectNetworkData { Reason = CloseConnectReason.VersionDifference }), networkData.Id);
                    return;
                }
                else if (userList.Count == 16)
                {
                    Host.Write(MessagePackSerializer.Serialize(new CloseConnectNetworkData { Reason = CloseConnectReason.MaximumCapacity }), networkData.Id);
                    return;
                }
                else if (kickedIPs.Contains(networkData.Ip))
                {
                    Host.Write(MessagePackSerializer.Serialize(new CloseConnectNetworkData { Reason = CloseConnectReason.Kicked }), networkData.Id);
                    return;
                }

                Host.Write(MessagePackSerializer.Serialize(new SendServerInfoNetworkData { Id = networkData.Id, AllowedModIds = server.AllowedModIds }), networkData.Id);
                if (currentSong != null)
                {
                    Host.Write(MessagePackSerializer.Serialize(new ChangeSongNetworkData { Hash = currentSong.Hash, Difficulty = currentSong.Difficulty }), networkData.Id);
                }
                Host.Write(MessagePackSerializer.Serialize(new ChangeGameRuleNetworkData { GameRule = currentRule }), networkData.Id);

                foreach (UserInfo tempUser in userList)
                {
                    Host.Write(MessagePackSerializer.Serialize(new AddUserNetworkData
                    {
                        Id = tempUser.ID,
                        UserName = tempUser.Name,
                        AccountId = tempUser.AccountId,
                        State = tempUser.CurrentState,
                    }), networkData.Id);
                }

                var user = AddUser((AddUserNetworkData)networkData);
                Host.WriteExceptID(MessagePackSerializer.Serialize(new AddUserNetworkData
                {
                    Id = user.ID,
                    UserName = user.Name,
                    AccountId = user.AccountId,
                    State = user.CurrentState,
                }), user.ID);
                Host.Write(MessagePackSerializer.Serialize(new ChangeLeaderNetworkData { UserId = Leader.ID }), user.ID);
                if (ContextManager.Logger != null)
                {
                    ContextManager.Logger.AddLog(String.Format("{0}({1}) joined", user.Name, user.AccountId));
                }
            });
            AddProcessor<DeleteUserNetworkData>(networkData =>
            {
                var user = FindUser(networkData.Id);
                if (user != null)
                {
                    DeleteUser(user);
                    Host.Write(MessagePackSerializer.Serialize(new DeleteUserNetworkData { Id = user.ID }));
                    SendCommonSongInfo();
                    if (ContextManager.Logger != null)
                    {
                        ContextManager.Logger.AddLog(String.Format("{0}({1}) leaved", user.Name, user.AccountId));
                    }
                }
            });
            AddProcessor<AddMessageNetworkData>(networkData =>
            {
                var user = FindUser(networkData.Id);
                if (user != null)
                {
                    Host.WriteExceptID(MessagePackSerializer.Serialize(new AddMessageNetworkData { Message = networkData.Message, Id = user.ID }), user.ID);
                    if (ContextManager.Logger != null)
                    {
                        ContextManager.Logger.AddLog(String.Format("{0}({1}) said '{2}'", user.Name, user.AccountId, networkData.Message));
                    }
                }
            });
            AddProcessor<AddPrivateMessageNetworkData>(networkData =>
            {
                var user = FindUser(networkData.Id);
                var targetUser = FindUser(networkData.UserId);
                if (user != null && targetUser != null)
                {
                    Host.Write(MessagePackSerializer.Serialize(new AddPrivateMessageNetworkData
                    {
                        Message = networkData.Message,
                        Id = networkData.Id,
                        UserId = networkData.UserId
                    }), networkData.UserId);
                    if (ContextManager.Logger != null)
                    {
                        ContextManager.Logger.AddLog(String.Format("{0}({1}) said to {2}({3}) '{4}'", user.Name, user.AccountId,
                            targetUser.Name, targetUser.AccountId, networkData.Message));
                    }
                }
            });
            AddProcessor<ChangeUserStateNetworkData>(networkData =>
            {
                var user = FindUser(networkData.Id);
                if (user != null)
                {
                    user.CurrentState = networkData.State;
                    Host.WriteExceptID(MessagePackSerializer.Serialize(new ChangeUserStateNetworkData { State = user.CurrentState, Id = user.ID }), user.ID);
                }
            });
            AddProcessor<HasSongNetworkData>(networkData =>
            {
                var user = FindUser(networkData.Id);
                if (user != null)
                {
                    user.HasSong = true;
                    Host.WriteExceptID(MessagePackSerializer.Serialize(new HasSongNetworkData { Id = networkData.Id }), user.ID);
                }
            });
            AddProcessor<FailedToCreateRoomNetworkData>(networkData =>
            {
                ContextManager.OnFailedToCreateRoom();
                if (ContextManager.Logger != null)
                {
                    ContextManager.Logger.AddLog("Failed to create room");
                }
            });
            AddProcessor<ChangeGameRuleNetworkData>(networkData =>
            {
                var user = FindUser(networkData.Id);
                if (user != null && user.IsLeader)
                {
                    if (state == State.None)
                    {
                        currentRule = networkData.GameRule;
                        Host.Write(MessagePackSerializer.Serialize(new ChangeGameRuleNetworkData { GameRule = currentRule }));
                        if (ContextManager.Logger != null)
                        {
                            ContextManager.Logger.AddLog(String.Format("Rule was changed to {0}", currentRule));
                        }
                    }
                }
            });
            AddProcessor<ChangeSongNetworkData>(networkData =>
            {
                var user = FindUser(networkData.Id);
                if (user != null && user.IsLeader)
                {
                    if (state == State.None)
                    {
                        currentSong = new SongInfo { Hash = networkData.Hash, Difficulty = networkData.Difficulty };
                        foreach (var u in userList)
                        {
                            u.HasSong = false;
                        }
                        Host.Write(MessagePackSerializer.Serialize(new ChangeSongNetworkData { Hash = networkData.Hash, Difficulty = networkData.Difficulty }));
                        if (ContextManager.Logger != null)
                        {
                            ContextManager.Logger.AddLog(String.Format("Song was changed {0}({1})",
                               GetStringArray(currentSong.Hash), currentSong.Difficulty));
                        }
                    }
                }
            });
            AddProcessor<PingNetworkData>(networkData =>
            {
                Host.Write(MessagePackSerializer.Serialize(new PingUserNetworkData { Ping = (int)(stopWatch.ElapsedMilliseconds - networkData.Time), Id = networkData.Id }));
            });
            AddProcessor<ForceStartNetworkData>(networkData =>
            {
                var user = FindUser(networkData.Id);
                if (user != null && user.IsLeader)
                {
                    forceStart = true;
                    Host.Write(MessagePackSerializer.Serialize(new ForceStartNetworkData()));
                }
            });
            AddProcessor<SendScoreListNetworkData>(networkData =>
            {
                var user = FindUser(networkData.Id);
                if (user != null)
                {
                    user.SongInfos = networkData.SongInfos;
                    SendCommonSongInfo();
                }
            });
            AddProcessor<KickUserNetworkData>(networkData =>
            {
                var user = FindUser(networkData.Id);
                var targetUser = FindUser(networkData.UserId);
                if (user != null && user.IsLeader && targetUser != null && targetUser != user)
                {
                    kickedIPs.Add(targetUser.Ip);
                    Host.Write(MessagePackSerializer.Serialize(new CloseConnectNetworkData { Reason = CloseConnectReason.Kicked }), targetUser.ID);
                    Host.WriteExceptID(MessagePackSerializer.Serialize(new KickUserNetworkData { UserId = targetUser.ID }), targetUser.ID);
                    DeleteUser(targetUser);
                    SendCommonSongInfo();
                    if (ContextManager.Logger != null)
                    {
                        ContextManager.Logger.AddLog(String.Format("{0}({1}) was kicked", targetUser.Name, targetUser.AccountId));
                    }
                }
            });
            AddProcessor<ChangeLeaderNetworkData>(networkData =>
            {
                var user = FindUser(networkData.Id);
                var targetUser = FindUser(networkData.UserId);
                if (user != null && user.IsLeader && targetUser != null && targetUser != user)
                {
                    Host.Write(MessagePackSerializer.Serialize(new ChangeLeaderNetworkData { UserId = targetUser.ID }));
                    user.IsLeader = false;
                    targetUser.IsLeader = true;
                    if (ContextManager.Logger != null)
                    {
                        ContextManager.Logger.AddLog(String.Format("Leader was changed to {0}({1})", targetUser.Name, targetUser.AccountId));
                    }
                }
            });
        }

        public override void Start()
        {
            if (ContextManager.WebManager != null)
            {
                var createRoomExecutor = new CreateRoomExecutor(ContextManager.WebManager, ContextManager.RoomInfo);
                createRoomExecutor.Finished += createRoomExecutor_Finished;
                createRoomExecutor.Start();
            }
            pingTimerID = ContextManager.TimerManager.AddTimerCallBack(pingTimerCallBack, 5000, false, true);
        }

        private void SendCommonSongInfo()
        {
            UserInfo leader = Leader;
            if (leader != null && leader.SongInfos != null)
            {
                var list = new List<SongInfo>(leader.SongInfos);
                var removeList = new List<SongInfo>();
                foreach (UserInfo userInfo in userList)
                {
                    if (userInfo.SongInfos == null || userInfo.IsLeader)
                    {
                        continue;
                    }

                    foreach (SongInfo songInfo in list)
                    {
                        if (userInfo.SongInfos.FirstOrDefault(s => s.Difficulty == songInfo.Difficulty && CompareArray(s.Hash, songInfo.Hash)) == null)
                        {
                            removeList.Add(songInfo);
                        }
                    }
                    foreach (SongInfo songInfo in removeList)
                    {
                        list.Remove(songInfo);
                    }
                }
                Host.Write(MessagePackSerializer.Serialize(new SendScoreListNetworkData { SongInfos = list.ToArray() }));
            }
        }

        private bool CompareArray(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }

            return true;
        }

        private string GetStringArray(byte[] array)
        {
            return array.Select(b => b.ToString("X2")).Aggregate("", (s1, s2) => String.Format("{0}{1}", s1, s2));
        }

        private UserInfo AddUser(AddUserNetworkData addUserNetworkData)
        {
            var user = new UserInfo
            {
                AccountId = addUserNetworkData.AccountId,
                Name = addUserNetworkData.UserName,
                CurrentState = addUserNetworkData.State,
                IsSelf = false,
                ID = addUserNetworkData.Id,
                Ip = addUserNetworkData.Ip
            };
            userList.Add(user);
            CheckLeaderExist();
            return user;
        }

        private void DeleteUser(UserInfo user)
        {
            if (user != null)
            {
                userList.Remove(user);
            }
            if (userList.Count == 0)
            {
                kickedIPs.Clear();
                currentSong = null;
                currentRule = new GameRule();
                state = State.None;
            }
            CheckLeaderExist();
        }

        private void CheckLeaderExist()
        {
            if (Leader == null)
            {
                if (userList.Count > 0)
                {
                    var leader = userList[0];
                    userList[0].IsLeader = true;
                    Host.Write(MessagePackSerializer.Serialize(new ChangeLeaderNetworkData { UserId = leader.ID }), leader.ID);
                    if (ContextManager.Logger != null)
                    {
                        ContextManager.Logger.AddLog(String.Format("Leader was changed to {0}({1})", leader.Name, leader.AccountId));
                    }
                }
            }
        }

        private UserInfo FindUser(int id)
        {
            var user = userList.Find((tempUser) => tempUser.ID == id);
            return user;
        }

        private void CheckUserReady()
        {
            if (state == State.None && currentSong != null && userList.Count > 0)
            {
                bool ok = true;
                foreach (UserInfo userInfo in userList)
                {
                    if (userInfo.HasSong)
                    {
                        if (userInfo.CurrentState != UserState.Ready)
                        {
                            ok = false;
                            break;
                        }
                    }
                    else
                    {
                        if (!forceStart)
                        {
                            ok = false;
                            break;
                        }
                    }
                }

                if (ok && userList.Any(u => u.HasSong))
                {
                    playUserList = userList.Where(u => u.HasSong).ToArray();
                    state = State.Prepare;
                    foreach (UserInfo user in playUserList)
                    {
                        user.CurrentState = UserState.NotReady;
                        Host.Write(MessagePackSerializer.Serialize(new GoToPlayPrepareNetworkData()), user.ID);
                    }
                    prepareStartTime = stopWatch.ElapsedMilliseconds;
                }
            }
        }

        private void CheckGoToPlay()
        {
            if (state == State.Prepare && stopWatch.ElapsedMilliseconds - prepareStartTime >= 5000)
            {
                state = State.Moved;
                var ids = playUserList.Select(u => u.ID).ToArray();
                foreach (UserInfo userInfo in playUserList)
                {
                    Host.Write(MessagePackSerializer.Serialize(new GoToPlayNetworkData { PlayerIds = ids }), userInfo.ID);
                }
                ContextManager.PushContext(new MainGameContext(this));
                if (ContextManager.Logger != null)
                {
                    ContextManager.Logger.AddLog("Main game started");
                }
            }
        }

        public override void OnChildPoped()
        {
            state = State.None;
            forceStart = false;
            foreach (var user in playUserList)
            {
                user.CurrentState = UserState.NotReady;
            }
        }

        public override void Update()
        {
            CheckUserReady();
            CheckGoToPlay();
        }

        protected override void DisposeResource()
        {
            if (ContextManager.WebManager != null)
            {
                ContextManager.TimerManager.RemoveTimerCallBack(updateTimerID);
                var deleteExecutor = new DeleteRoomExecutor(ContextManager.WebManager);
                deleteExecutor.Start();
            }
            ContextManager.TimerManager.RemoveTimerCallBack(pingTimerID);
            stopWatch.Stop();
        }

        void createRoomExecutor_Finished(object sender, EventArgs e)
        {
            var executor = sender as ExecutorBase;

            if (!executor.Success)
            {
                ContextManager.Enqueue(new FailedToCreateRoomNetworkData());
            }
            else
            {
                updateTimerID = ContextManager.TimerManager.AddTimerCallBack(updateTimerCallBack, 15000, false, true);
            }
        }

        void updateTimerCallBack(int obj)
        {
            var updateExecutor = new UpdateRoomInfoExecutor(ContextManager.WebManager, userList.Count,
                currentSong != null ? CryptographyUtility.Getx2Encoding(currentSong.Hash) : "");
            updateExecutor.Finished += UpdateExecutor_Finished;
            updateExecutor.Start();
        }

        private void UpdateExecutor_Finished(object sender, EventArgs e)
        {
            var executor = sender as ExecutorBase;

            if (!executor.Success)
            {
                var createRoomExecutor = new CreateRoomExecutor(ContextManager.WebManager, ContextManager.RoomInfo);
                createRoomExecutor.Start();
            }
        }

        void pingTimerCallBack(int obj)
        {
            Host.Write(MessagePackSerializer.Serialize(new PingNetworkData { Time = stopWatch.ElapsedMilliseconds }));
        }
    }
}
