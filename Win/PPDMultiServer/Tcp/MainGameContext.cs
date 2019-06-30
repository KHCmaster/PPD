using MessagePack;
using PPDMultiCommon.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PPDMultiServer.Tcp
{
    class MainGameContext : ServerContextBase
    {
        const int WaitMilliSecond = 8000;

        enum State
        {
            None,
            NotReady,
            Ready,
            WaitFinish,
        }

        State state = State.NotReady;
        List<UserInfo> userList;
        List<UserPlayState> userPlayStateList;
        List<PPDMultiServer.Model.UserResult> resultList;
        Stopwatch stopWatch;
        long waitStartTime;
        long finishWaitStartTime;

        public MainGameContext(ServerContextBase previousContext)
            : base(previousContext)
        {
            stopWatch = new Stopwatch();
            userList = new List<UserInfo>();
            userPlayStateList = new List<UserPlayState>();
            foreach (UserInfo userInfo in (previousContext as MenuContext).UserList)
            {
                userList.Add(userInfo);
                userPlayStateList.Add(new UserPlayState { User = userInfo });
            }
            resultList = new List<PPDMultiServer.Model.UserResult>();

            stopWatch.Start();
            waitStartTime = stopWatch.ElapsedMilliseconds;

            AddProcessor<MainGameLoadedNetworkData>(networkData =>
            {
                var userPlayState = FindUserPlayState(networkData.Id);
                if (userPlayState != null)
                {
                    Host.Write(MessagePackSerializer.Serialize(new MainGameLoadedNetworkData { Id = userPlayState.User.ID }));
                    userPlayState.Loaded = true;
                    waitStartTime = stopWatch.ElapsedMilliseconds;
                }
            });
            AddProcessor<ChangeScoreNetworkData>(networkData =>
            {
                var userPlayState = FindUserPlayState(networkData.Id);
                if (userPlayState != null)
                {
                    userPlayState.Score = networkData.Score;
                    Host.WriteExceptID(MessagePackSerializer.Serialize(new ChangeScoreNetworkData { Id = userPlayState.User.ID, Score = networkData.Score }), userPlayState.User.ID);
                }
            });
            AddProcessor<ChangeLifeNetworkData>(networkData =>
            {
                var userPlayState = FindUserPlayState(networkData.Id);
                if (userPlayState != null)
                {
                    userPlayState.Life = networkData.Life;
                    Host.WriteExceptID(MessagePackSerializer.Serialize(new ChangeLifeNetworkData { Id = userPlayState.User.ID, Life = networkData.Life }), userPlayState.User.ID);
                }
            });
            AddProcessor<ChangeEvaluateNetworkData>(networkData =>
            {
                var userPlayState = FindUserPlayState(networkData.Id);
                if (userPlayState != null)
                {
                    userPlayState.Evaluate = networkData.Evaluate;
                    Host.WriteExceptID(MessagePackSerializer.Serialize(new ChangeEvaluateNetworkData { Id = userPlayState.User.ID, Evaluate = networkData.Evaluate }), userPlayState.User.ID);
                }
            });
            AddProcessor<SendResultNetworkData>(networkData =>
            {
                var user = FindUser(networkData.Id);
                if (user != null)
                {
                    AddResult(networkData.Result, user);
                    if (user.IsLeader)
                    {
                        if (state != State.WaitFinish)
                        {
                            state = State.WaitFinish;
                            finishWaitStartTime = stopWatch.ElapsedMilliseconds;
                        }
                    }
                }
            });
            AddProcessor<AddEffectToPlayerNetworkData>(networkData =>
            {
                Host.Write(MessagePackSerializer.Serialize(new AddEffectToPlayerNetworkData { ItemType = networkData.ItemType, Id = networkData.Id }), networkData.UserId);
                if (ContextManager.Logger != null)
                {
                    var user = FindUser(networkData.Id);
                    var targetUser = FindUser(networkData.UserId);
                    if (user != null && targetUser != null)
                    {
                        ContextManager.Logger.AddLog("{0}(@{1}) used {2} to {3}(@{4})", user.Name, user.AccountId, networkData.ItemType, targetUser.Name, targetUser.AccountId);
                    }
                }
            });
            AddProcessor<AddEffectNetworkData>(networkData =>
            {
                Host.WriteExceptID(MessagePackSerializer.Serialize(new AddEffectToPlayerNetworkData { ItemType = networkData.ItemType, Id = networkData.Id }), networkData.Id);
                if (ContextManager.Logger != null)
                {
                    var user = FindUser(networkData.Id);
                    if (user != null)
                    {
                        ContextManager.Logger.AddLog("{0}(@{1}) used {2}", user.Name, user.AccountId, networkData.ItemType);
                    }
                }
            });
            AddProcessor<JustGoToMenuNetworkData>(networkData =>
            {
                var user = FindUser(networkData.Id);
                if (user != null && user.IsLeader)
                {
                    Host.Write(MessagePackSerializer.Serialize(new JustGoToMenuNetworkData()));
                    ContextManager.PopContext();
                }
            });
            AddProcessor<DeleteUserNetworkData>(networkData =>
            {
                var user = FindUser(networkData.Id);
                if (user != null)
                {
                    DeleteUser(user);
                }
            });
        }

        private void DeleteUser(UserInfo user)
        {
            var userPlayState = FindUserPlayState(user.ID);
            if (userPlayState != null)
            {
                userPlayStateList.Remove(userPlayState);
            }
            if (user != null)
            {
                userList.Remove(user);
            }
        }

        private UserPlayState FindUserPlayState(int id)
        {
            var userPlayState = userPlayStateList.Find((temp) => temp.User.ID == id);
            return userPlayState;
        }

        private UserInfo FindUser(int id)
        {
            var user = userList.Find((tempUser) => tempUser.ID == id);
            return user;
        }

        private void AddResult(Result result, UserInfo user)
        {
            if (resultList.FirstOrDefault(r => r.User == user) == null)
            {
                resultList.Add(new Model.UserResult { Result = result, User = user });
            }
        }

        private void CheckReady()
        {
            if (state == State.NotReady)
            {
                bool ok = true;
                foreach (UserPlayState userPlayState in userPlayStateList)
                {
                    if (!userPlayState.Loaded)
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok || stopWatch.ElapsedMilliseconds - waitStartTime >= WaitMilliSecond)
                {
                    state = State.Ready;
                    Host.Write(MessagePackSerializer.Serialize(new PlayMainGameNetworkData()));
                }
            }
        }

        private string SerializeResult()
        {
            var sb = new StringBuilder();
            sb.Append("Result");
            sb.AppendLine();
            int iter = 1;
            foreach (Model.UserResult result in resultList)
            {
                sb.AppendFormat("No.{0} Name:{1} Score:{2} C:{3} G:{4} SF:{5} SD:{6} W:{7} MC:{8}", iter, result.User.Name, result.Result.Score,
                    result.Result.CoolCount, result.Result.GoodCount, result.Result.SafeCount, result.Result.SadCount, result.Result.WorstCount, result.Result.MaxCombo);
                sb.AppendLine();
                iter++;
            }
            return sb.ToString();
        }

        private void CheckFinish()
        {
            if (state == State.WaitFinish)
            {
                bool ok = true;

                foreach (UserInfo user in userList)
                {
                    if (resultList.Find((result) => result.User == user) == null)
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok || stopWatch.ElapsedMilliseconds - finishWaitStartTime >= WaitMilliSecond)
                {
                    state = State.None;
                    Host.Write(MessagePackSerializer.Serialize(new GoToMenuNetworkData
                    {
                        Results = resultList.Select(r => new Tuple<int, Result>(r.User.ID, r.Result)).ToArray()
                    }));
                    if (ContextManager.Logger != null)
                    {
                        ContextManager.Logger.AddLog(SerializeResult());
                    }
                    ContextManager.PopContext();
                }
            }
            else if (userList.Count == 0)
            {
                if (ContextManager.Logger != null)
                {
                    ContextManager.Logger.AddLog("No playing user. Back to menu.");
                }
                ContextManager.PopContext();
            }
        }

        public override void Update()
        {
            CheckReady();
            CheckFinish();
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
            stopWatch.Stop();
        }
    }
}
