using PPDMath;
using PPDCore;
using PPDCoreModel;
using PPDCoreModel.Data;
using PPDFramework;
using PPDFramework.Web;
using PPDFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PPDSingle
{
    class MainGame(PPDDevice device) : PPDCore.MainGame(device)
    {
        UserScoreListComponent userScoreListComponent;
        UserPlayState selfPlayState;
        GhostPlayInfo[] ghostPlayInfos;

        protected override void OnBeforeInitialize()
        {
            base.OnBeforeInitialize();

            if (gameUtility.RivalGhost)
            {
                var newGameUtility = new PPDGameUtility
                {
                    SongInformation = gameUtility.SongInformation,
                    Difficulty = gameUtility.Difficulty,
                    DifficultString = gameUtility.DifficultString,
                    Profile = ProfileManager.Instance.Default,
                    SpeedScale = 1,
                    GodMode = true,
                    MuteSE = true
                };
                using var disposable = Sound.Disable();
                var reason = WebManager.Instance.GetGhost(CryptographyUtility.Getx2Encoding(gameUtility.SongInformation.GetScoreHash(gameUtility.Difficulty)),
                    gameUtility.RivalGhostCount, out GhostInfo[] ghosts);
                if (reason == ErrorReason.OK)
                {
                    var games = new MainGameComponent[ghosts.Length];
                    for (var i = 0; i < games.Length; i++)
                    {
                        games[i] = new MainGameComponent(device, GameHost, ResourceManager, Sound, this, newGameUtility,
                            null, (MarkImagePathsBase)Param["MarkImagePath"], null, null, MainGameConfigBase.Default, startTime, mmStartTime);
                        games[i].Initialize(false, true);
                    }
                    userScoreListComponent = new UserScoreListComponent(device, ResourceManager)
                    {
                        Position = new System.Numerics.Vector2(680, 45)
                    };

                    var ghostInfo = new Dictionary<GhostInfo, GhostFrame[]>();
                    var actions = new Action[] {
                            () => {
                                var progresses =new int[ghosts.Length];
                                var prevAllProgress = 0;
                                void allProgressCallback (int i, int p)                                 {
                                    progresses[i] = p;
                                    var allProgress = progresses.Sum() / ghosts.Length;
                                    if (prevAllProgress != allProgress)
                                    {
                                        GameHost.SendToLoading(new Dictionary<string, object>
                                        {
                                            {"Progress", allProgress}                                    });
                                        prevAllProgress = allProgress;
                                    }
                                }
                                Parallel.For(0, ghosts.Length, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },  i =>
                                {
                                    var ghost = ghosts[i];
                                    var tempMainGameComponent = games[i];
                                    var prevProgress = 0;
                                    void progressCallback(int p) {
                                        if (prevProgress != p)
                                        {
                                            allProgressCallback(i, p);
                                            prevProgress = p;
                                        }
                                    }
                                    var ghostFrame = tempMainGameComponent.GetGhostFrames(ghost.ReplayData, progressCallback);
                                    lock(ghostInfo)
                                    {
                                         ghostInfo.Add(ghost,ghostFrame);
                                    }
                                });
                            },
                            () => {
                                InitializeUserList(ghosts);
                            }
                        };
                    Parallel.ForEach(actions, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, a => a());
                    ghostPlayInfos = [.. ghostInfo.Select(p =>
                        new GhostPlayInfo(
                            userScoreListComponent.Players.FirstOrDefault(pi => !pi.UserPlayState.User.IsSelf && pi.UserPlayState.User.AccountId == p.Key.AccountId).UserPlayState,
                            p.Value))];
                }
            }
        }

        private void InitializeUserList(GhostInfo[] ghosts)
        {
            selfPlayState = new UserPlayState(new UserInfo
            {
                AccountId = WebManager.Instance.CurrentAccountId,
                Name = WebManager.Instance.CurrentUserName,
                IsSelf = true
            });
            userScoreListComponent.AddUser(selfPlayState, true, true, true);
            foreach (var t in ghosts.Select(g => new Tuple<GhostInfo, UserInfo>(g, new UserInfo
            {
                AccountId = g.AccountId,
                Name = g.AccountName
            })))
            {
                var userPlayState = new UserPlayState(t.Item2);
                userScoreListComponent.AddUser(userPlayState, t.Item1.ShowScore, t.Item1.ShowEvaluate, t.Item1.ShowLife);
            }
            userScoreListComponent.Retry();
            gr.Retryed += Gr_Retryed;
            gr.Replayed += Gr_Replayed;
            mainGameComponent.Drawed += MainGameComponent_Drawed;
            mainGameComponent.ScoreChanged += MainGameComponent_ScoreChanged;
            mainGameComponent.LifeChanged += MainGameComponent_LifeChanged;
            mainGameComponent.EvaluateChanged += MainGameComponent_EvaluateChanged;
            foreach (var u in userScoreListComponent.Players)
            {
                var path = Path.GetTempFileName();
                File.WriteAllBytes(path, WebManager.Instance.GetAccountImage(u.UserPlayState.User.AccountId));
                u.UserImagePath = path;
            }
        }

        void Gr_Replayed(object sender, EventArgs e)
        {
            Reset();
        }

        void Gr_Retryed(object sender, EventArgs e)
        {
            Reset();
        }

        protected override void OnAfterRetry()
        {
            base.OnAfterRetry();
            Reset();
        }

        protected override void OnAfterReplay()
        {
            base.OnAfterReplay();
            Reset();
        }

        protected override bool OnInitializeError(Exception e)
        {
            if (e is PPDException exception && exception.ExceptionType == PPDExceptionType.CannotOpenMovie)
            {
                var ppde = exception;
                var message = PPDFramework.PPDExceptionContentProvider.Provider.GetContent(ppde.ExceptionType);
                GameHost.AddNotify(message);
                return true;
            }
            return base.OnInitializeError(e);
        }

        private void Reset()
        {
            if (selfPlayState != null)
            {
                selfPlayState.Score = 0;
                selfPlayState.Life = GameResultManager.DEFAULTLIFE;
                selfPlayState.Evaluate = (MarkEvaluateType)(-1);
            }
            if (ghostPlayInfos != null)
            {
                foreach (var ghost in ghostPlayInfos)
                {
                    ghost.Retry();
                }
            }
            if (userScoreListComponent != null)
            {
                userScoreListComponent.Retry();
                userScoreListComponent.Update();
            }
        }

        void MainGameComponent_EvaluateChanged(PPDFramework.MarkEvaluateType arg1, bool arg2)
        {
            if (selfPlayState != null)
            {
                selfPlayState.Evaluate = arg1;
                selfPlayState.IsMissPress = arg2;
            }
        }

        void MainGameComponent_LifeChanged(int obj)
        {
            selfPlayState?.Life = obj;
        }

        void MainGameComponent_ScoreChanged(int obj)
        {
            selfPlayState?.Score = obj;
        }

        void MainGameComponent_Drawed(LayerType layerType)
        {
            if (layerType == LayerType.AfterMarkEffect)
            {
                userScoreListComponent.Draw();
            }
        }

        public override void Update(InputInfoBase inputInfo, MouseInfo mouseInfo)
        {
            base.Update(inputInfo, mouseInfo);

            if (userScoreListComponent != null)
            {
                foreach (var ghostPlayInfo in ghostPlayInfos)
                {
                    ghostPlayInfo.Update(mainGameComponent.CurrentUpdateTime);
                }
                userScoreListComponent.Update();
            }
        }

        class GhostPlayInfo(UserPlayState userPlayState, GhostFrame[] ghostFrames)
        {
            readonly UserPlayState userPlayState = userPlayState;
            readonly GhostFrame[] ghostFrames = ghostFrames;
            int index;

            public void Retry()
            {
                index = 0;
                userPlayState.Score = 0;
                userPlayState.Life = GameResultManager.DEFAULTLIFE;
                userPlayState.Evaluate = (MarkEvaluateType)(-1);
            }

            public void Update(float time)
            {
                if (index >= ghostFrames.Length)
                {
                    return;
                }

                while (index < ghostFrames.Length && ghostFrames[index].Time <= time)
                {
                    var frame = ghostFrames[index];
                    if (frame.Score.HasValue)
                    {
                        userPlayState.Score = frame.Score.Value;
                    }
                    if (frame.Life.HasValue)
                    {
                        userPlayState.Life = frame.Life.Value;
                    }
                    if (frame.MarkEvaluateType.HasValue)
                    {
                        userPlayState.Evaluate = frame.MarkEvaluateType.Value;
                        userPlayState.IsMissPress = frame.IsMissPress;
                    }
                    index++;
                }
            }
        }
    }
}
