using PPDCoreModel;
using PPDCoreModel.Data;
using PPDExpansionCore;
using PPDExpansionCore.Tcp;
using PPDFramework;
using PPDFramework.Scene;
using PPDFramework.Web;
using System;
using System.Collections.Generic;

namespace PPDCore
{
    public class MainGame : SceneBase, IDisposable, IBlueSkyManager, IReviewManager
    {
        enum FadeOutAction
        {
            None,
            SetResult,
            Retry,
            Replay,
        }

        const float defaultbpm = 130;
        public event BoolEventHandler PostFinished;
        public event BoolEventHandler ReviewFinished;
        public event Action CannotStartPerfectTrial;
        public event Action<ErrorReason> PerfectTrialError;
        public event Action<int, int> PerfectTrialStart;

        protected MainGameComponent mainGameComponent;

        protected GameResultBase gr;
        protected PauseMenuBase pd;
        protected GameInterfaceBase cgi;
        RectangleComponent black;
        protected PPDGameUtility gameUtility;
        protected float startTime;
        protected float mmStartTime;

        bool PostFinish;
        bool PostSuccess;
        const string PostHashTag = "#PPDGame";

        bool reviewFinish;
        bool reviewSuccess;

        bool exitOnReturn;
        bool fadeOut;
        bool paused;
        bool isResult;
        FadeOutAction fadeOutAction = FadeOutAction.None;

        private Client client;

        public override string SpriteDir
        {
            get { return Utility.Path.BaseDir; }
        }

        public MainGame(PPDDevice device) : base(device)
        {
        }

        public override bool Load()
        {
            if (!Param.ContainsKey("PPDGameUtility"))
            {
                throw new PPDException(PPDExceptionType.SkinIsNotCorrectlyImplemented,
                    "Parameter PPDGameUtility is not contained", null);
            }
            gameUtility = Param["PPDGameUtility"] as PPDGameUtility;
            exitOnReturn |= Param.ContainsKey("ExitOnReturn");
            if (Param.ContainsKey("StartTime"))
            {
                gameUtility.IsDebug = true;
                startTime = (float)Param["StartTime"] - 3;
                mmStartTime = (float)Param["StartTime"];
            }
            else if (Param.ContainsKey("StartTimeEx"))
            {
                gameUtility.IsDebug = true;
                gameUtility.LatencyType = LatencyType.File;
                startTime = (float)Param["StartTimeEx"];
                mmStartTime = (float)Param["StartTimeEx"];
            }
            else
            {
                startTime = gameUtility.SongInformation.StartTime;
                mmStartTime = startTime;
            }

            black = new RectangleComponent(device, ResourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0,
                Hidden = true
            };

            cgi = (GameInterfaceBase)Param["GameInterface"];
            cgi.Sound = Sound;
            cgi.PPDGameUtility = gameUtility;
            cgi.ResourceManager = ResourceManager;
            cgi.Load();
            gr = Param["GameResult"] as GameResultBase;
            if (gr != null)
            {
                gr.GameHost = GameHost;
                gr.Sound = Sound;
                gr.PPDGameUtility = gameUtility;
                gr.ResourceManager = ResourceManager;
                gr.BlueSkyManager = this;
                gr.ReviewManager = this;
                gr.Load();
            }
            pd = (PauseMenuBase)Param["PauseMenu"];
            pd.Sound = Sound;
            pd.PPDGameUtility = gameUtility;
            pd.ResourceManager = ResourceManager;
            pd.Load();

            mainGameComponent = new MainGameComponent(device, GameHost, ResourceManager, Sound, this,
                gameUtility, cgi, (MarkImagePathsBase)Param["MarkImagePath"], gr, pd, MainGameConfigBase.Default, startTime, mmStartTime);
            mainGameComponent.Finished += mainGameComponent_Finished;
            mainGameComponent.Paused += mainGameComponent_Paused;
            mainGameComponent.Drawed += mainGameComponent_Drawed;

            pd.Resumed += pd_Resumed;
            pd.Retryed += pd_Retryed;
            pd.Replayed += pd_Replayed;
            pd.Returned += pd_Returned;

            if (gr != null)
            {
                gr.Returned += gr_Returned;
                gr.Retryed += gr_Retryed;
            }

            ConnectExpansion();
            OnBeforeInitialize();
            mainGameComponent.CannotStartPerfectTrial += mainGameComponent_CannotStartPerfectTrial;
            mainGameComponent.PerfectTrialError += mainGameComponent_PerfectTrialError;
            mainGameComponent.PerfectTrialStart += mainGameComponent_PerfectTrialStart;
            try
            {
                mainGameComponent.Initialize(true);
            }
            catch (Exception e)
            {
                if (!OnInitializeError(e))
                {
                    throw;
                }
                return false;
            }

            this.AddChild(black);
            this.AddChild(mainGameComponent);

            return true;
        }

        void mainGameComponent_PerfectTrialStart(int arg1, int arg2)
        {
            PerfectTrialStart?.Invoke(arg1, arg2);
        }

        void mainGameComponent_PerfectTrialError(ErrorReason obj)
        {
            PerfectTrialError?.Invoke(obj);
        }

        void mainGameComponent_CannotStartPerfectTrial()
        {
            CannotStartPerfectTrial?.Invoke();
        }

        private void ConnectExpansion()
        {
            if (Client.IsListening(PPDSetting.Setting.ExpansionWaitPort) && gameUtility.SongInformation != null)
            {
                client = new Client(PPDSetting.Setting.ExpansionWaitPort);
                client.Start();
                SendScoreInfo();
            }
        }

        private void SendScoreInfo()
        {
            var userHighScore = 0;
            var results = PPDFramework.ResultInfo.GetInfoFromSongInformation(gameUtility.SongInformation);
            if (results.Length > 0)
            {
                foreach (var result in results)
                {
                    if (result.Difficulty == gameUtility.Difficulty && userHighScore < result.Score)
                    {
                        userHighScore = result.Score;
                    }
                }
            }
            var webHighScore = 0;
            var ranking = gameUtility.SongInformation.GetRanking();
            if (ranking != null)
            {
                foreach (var r in ranking.GetInfo(gameUtility.Difficulty))
                {
                    if (webHighScore < r.Score)
                    {
                        webHighScore = r.Score;
                    }
                }
            }
            client.Send(new ScoreInfo
            {
                StartTime = startTime,
                EndTime = gameUtility.SongInformation.EndTime,
                UserHighScore = userHighScore,
                WebHighScore = webHighScore,
                PlayType = PlayType.SinglePlay,
                ScoreHash = gameUtility.SongInformation.GetScoreHash(gameUtility.Difficulty),
                IsRegular = gameUtility.IsRegular,
                ScoreName = gameUtility.SongInformation.DirectoryName,
                Difficulty = gameUtility.Difficulty
            });
        }

        void gr_Retryed(object sender, EventArgs e)
        {
            isResult = false;
            PostFinish = PostSuccess = false;
            reviewFinish = reviewSuccess = false;
            if (client != null)
            {
                SendScoreInfo();
            }
        }

        void gr_Returned(object sender, EventArgs e)
        {
            ReturnToMenu();
        }

        void mainGameComponent_Drawed(LayerType layerType)
        {
            if (layerType == LayerType.AfterInterface && paused)
            {
                black.Hidden = false;
                black.Alpha = 0.5f;
                black.Update();
                black.Draw();
                black.Hidden = true;
                black.Alpha = 0;
                black.Update();
            }
        }

        void mainGameComponent_Paused(object sender, EventArgs e)
        {
            paused = true;
        }

        void pd_Resumed(object sender, EventArgs e)
        {
            paused = false;
        }

        void pd_Returned(object sender, EventArgs e)
        {
            paused = false;
            ReturnToMenu();
        }

        void pd_Retryed(object sender, EventArgs e)
        {
            paused = false;
            fadeOutAction = FadeOutAction.Retry;
            fadeOut = true;
            black.Hidden = false;
            black.Alpha = 0;
            isResult = false;
            if (client != null)
            {
                SendScoreInfo();
            }
        }

        void pd_Replayed(object sender, EventArgs e)
        {
            paused = false;
            fadeOutAction = FadeOutAction.Replay;
            fadeOut = true;
            black.Hidden = false;
            black.Alpha = 0;
        }

        void mainGameComponent_Finished(object sender, EventArgs e)
        {
            mainGameComponent.MovieFadeOut();
            fadeOutAction = FadeOutAction.SetResult;
            fadeOut = true;
            black.Hidden = false;
            black.Alpha = 0;
        }

        private void ReturnToMenu()
        {
            if (exitOnReturn)
            {
                GameHost.Exit();
            }
            else
            {
                var param = new Dictionary<string, object> {
                { "PerfectTrialSucceess",gr != null && gr.PerfectTrialSucceess},                { "GameUtility", gameUtility}                };
                if (IsInSceneStack)
                {
                    SceneManager.PopCurrentScene(param);
                }
                else
                {
                    var type = (Type)Param["NextScene"];
                    SceneManager.PrepareNextScene(this, (ISceneBase)Activator.CreateInstance(type), param, null);
                }
            }
        }

        public void ShowResult()
        {
            if (gr == null)
            {
                ReturnToMenu();
                return;
            }

            if (client != null && !mainGameComponent.Replaying)
            {
                client.Send(new PPDExpansionCore.ResultInfo
                {
                    CoolCount = mainGameComponent.CoolCount,
                    GoodCount = mainGameComponent.GoodCount,
                    SafeCount = mainGameComponent.SafeCount,
                    SadCount = mainGameComponent.SadCount,
                    WorstCount = mainGameComponent.WorstCount,
                    MaxCombo = mainGameComponent.MaxCombo,
                    Score = mainGameComponent.Score
                });
            }
            isResult = true;
            mainGameComponent.ShowResult();
            PostFinish = false;
            PostSuccess = false;
            if (CanPost)
            {
                PostText = CreatePostText(mainGameComponent.GameResultManager.CurrentScore, gr.Result, mainGameComponent.GameResultManager);
            }
            else
            {
                PostText = "";
            }
        }

        public bool CanPost
        {
            get
            {
                return BlueSkyManager.Manager.IsAvailable && gameUtility.IsRegular && !mainGameComponent.Mistake && !PostSuccess && !mainGameComponent.Replaying;
            }
        }

        public string PostText
        {
            get;
            private set;
        }

        public DateTime FinishDate
        {
            get;
            private set;
        }

        public string PostFilePath
        {
            get;
            set;
        }

        private string CreatePostText(int score, ResultEvaluateType result, GameResultManager gameResultManager)
        {
            FinishDate = DateTime.Now;
            var text = string.Format("譜面:{0} 難易度:{1} スコア:{2} 評価:{3} C{4} G{5} SF{6} SD{7} W{8} MC{9} {10} ",
                gameUtility.SongInformation.DirectoryName,
                gameUtility.Difficulty,
                score,
                result,
                gameResultManager.CoolCount,
                gameResultManager.GoodCount,
                gameResultManager.SafeCount,
                gameResultManager.SadCount,
                gameResultManager.WorstCount,
                gameResultManager.MaxCombo,
                FinishDate);

            return text;
        }

        public void Post()
        {
            if (CanPost)
            {
                BlueSkyManager.Manager.PostStatus(PostText, PostHashTag, PostFilePath, EndPostCallback);
            }
        }

        private void EndPostCallback(bool success)
        {
            PostFinish = true;
            PostSuccess = success;
        }

        public void Return(object sender, EventArgs e)
        {
            this.ReturnToMenu();
        }

        public override void Update(InputInfoBase inputInfo, MouseInfo mouseInfo)
        {
            if (fadeOut)
            {
                if (black.Alpha >= 1)
                {
                    black.Alpha = 1;
                    switch (fadeOutAction)
                    {
                        case FadeOutAction.Retry:
                            mainGameComponent.Retry();
                            OnAfterRetry();
                            break;
                        case FadeOutAction.Replay:
                            mainGameComponent.Replay();
                            OnAfterReplay();
                            break;
                        case FadeOutAction.SetResult:
                            ShowResult();
                            break;
                    }
                    fadeOutAction = FadeOutAction.None;
                    black.Hidden = true;
                    fadeOut = false;
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

            if (PostFinish)
            {
                PostFinish = false;
                OnPostFinish();
            }
            if (reviewFinish)
            {
                reviewFinish = false;
                OnReviewFinish();
            }

            mainGameComponent.Update(inputInfo, mouseInfo);
            base.Update();

            if (!isResult && client != null && !mainGameComponent.Replaying)
            {
                client.Send(new UpdateInfo
                {
                    CurrentTime = mainGameComponent.MoviePosition,
                    CoolCount = mainGameComponent.CoolCount,
                    GoodCount = mainGameComponent.GoodCount,
                    SafeCount = mainGameComponent.SafeCount,
                    SadCount = mainGameComponent.SadCount,
                    WorstCount = mainGameComponent.WorstCount,
                    MaxCombo = mainGameComponent.MaxCombo,
                    Score = mainGameComponent.Score,
                    Life = mainGameComponent.GameResultManager.CurrentLife
                });
            }
        }

        protected void OnPostFinish()
        {
            if (PostFinished != null)
            {
                PostFinished.Invoke(PostSuccess);
            }
        }

        protected virtual void OnAfterRetry()
        {
        }

        protected virtual void OnAfterReplay()
        {
        }

        protected virtual void OnBeforeInitialize()
        {

        }

        protected virtual bool OnInitializeError(Exception e)
        {
            return false;
        }

        #region IReviewManager メンバー


        protected void OnReviewFinish()
        {
            if (ReviewFinished != null)
            {
                ReviewFinished.Invoke(reviewSuccess);
            }
        }

        public bool CanReview
        {
            get
            {
                return WebManager.Instance.IsLogined && WebManager.Instance.IsRankingAvailable(gameUtility.SongInformation.GetPrimaryHash()) && !reviewSuccess;
            }
        }

        public void Review(string str, int rate)
        {
            if (CanReview)
            {
                ReviewManager.Manager.Review(str, rate, gameUtility.SongInformation.GetScoreHash(gameUtility.Difficulty), EndReviewCallback);
            }
        }

        private void EndReviewCallback(bool success)
        {
            reviewFinish = true;
            reviewSuccess = success;
        }

        #endregion

        protected override void DisposeResource()
        {
            if (client != null)
            {
                client.Close();
                client = null;
            }
            base.DisposeResource();
        }
    }
}
