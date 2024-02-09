using PPDCore;
using PPDFramework;
using PPDFramework.Web;
using System;
using System.Collections.Generic;

namespace PPDSingle
{
    /// <summary>
    /// ゲーム結果のUI
    /// </summary>
    public class GameResult : GameResultBase
    {
        public override event EventHandler Returned;
        public override event EventHandler Retryed;
        public override event EventHandler Replayed;

        FocusManager focusManager;
        GameResultScore grs;
        bool sendResultSuccess;

        public override void Retry()
        {
            grs.Retry();
        }

        public GameResult(PPDDevice device) : base(device)
        {
        }

        public override void Load()
        {
            focusManager = new FocusManager();
            ResultSet += GameResult_ResultSet;
            ScoreSent += GameResult_ScoreSent;
            ScoreSending += GameResult_ScoreSending;
            grs = new GameResultScore(device, ResourceManager, PPDGameUtility, Sound, BlueSkyManager, ReviewManager, GameHost);
            grs.Retryed += grs_Retryed;
            grs.Returned += grs_Returned;
            grs.Replayed += grs_Replayed;
            focusManager.Focus(grs);
            grs.UpdateRanking();

            this.AddChild(grs);
        }

        void GameResult_ScoreSending()
        {
            GameHost.AddNotify(Utility.Language["SendingResult"]);
        }

        void GameResult_ScoreSent(ErrorReason errorReason, Dictionary<string, string> data)
        {
            switch (errorReason)
            {
                case ErrorReason.NetworkError:
                case ErrorReason.ArgumentError:
                case ErrorReason.AuthFailed:
                case ErrorReason.ValidateFailed:
                    GameHost.AddNotify(Utility.Language["SendResultFailed"]);
                    break;
                case ErrorReason.ScoreNotFound:
                    GameHost.AddNotify(Utility.Language["SendResultScoreNotFound"]);
                    break;
                case ErrorReason.InvalidPlayTime:
                    GameHost.AddNotify(Utility.Language["SendResultInvalidPlayTime"]);
                    break;
                case ErrorReason.OK:
                    GameHost.AddNotify(Utility.Language["SendResultCompleted"]);
                    break;
            }
            if (data != null)
            {
                if (data.ContainsKey("Exp"))
                {
                    var exp = int.Parse(data["Exp"]);
                    GameHost.AddNotify(String.Format(Utility.Language["GotExp"], exp));
                }
                if (data.ContainsKey("Item"))
                {
                    var itemType = (ItemType)int.Parse(data["Item"]);
                    GameHost.AddNotify(String.Format(Utility.Language["GotItem"], Utility.Language[String.Format("Item{0}Name", itemType)]));
                }
                if (data.ContainsKey("LastLevel") && data.ContainsKey("Level"))
                {
                    int lastLevel = int.Parse(data["LastLevel"]), level = int.Parse(data["Level"]);
                    if (lastLevel != level)
                    {
                        GameHost.AddNotify(String.Format(Utility.Language["LevelUp"], lastLevel, level));
                    }
                }
                if (data.TryGetValue("PerfectTrialResult", out string result))
                {
                    if (result == "1")
                    {
                        GameHost.AddNotify(Utility.Language["PerfectTrialSuccess"]);
                        int moneyFrom = int.Parse(data["MoneyFrom"]), moneyTo = int.Parse(data["MoneyTo"]);
                        GameHost.AddNotify(String.Format(Utility.Language["MoneyChange"], moneyFrom, moneyTo));
                    }
                    else
                    {
                        GameHost.AddNotify(Utility.Language["PerfectTrialMisstake"]);
                    }
                }
                sendResultSuccess = true;
            }
        }

        void grs_Returned(object sender, EventArgs e)
        {
            if (this.Returned != null)
            {
                this.Returned.Invoke(this, EventArgs.Empty);
            }
        }

        void grs_Retryed(object sender, EventArgs e)
        {
            if (this.Retryed != null)
            {
                this.Retryed.Invoke(this, EventArgs.Empty);
            }
        }

        void grs_Replayed(object sender, EventArgs e)
        {
            if (this.Replayed != null)
            {
                this.Replayed.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// リザルト画面開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GameResult_ResultSet(object sender, EventArgs e)
        {
            grs.ResultSet(Score, MarkEvals, MaxCombo, Result, HighScore, CanReplay);
        }

        public override void Update(InputInfoBase inputInfo)
        {
            if (Disposed) return;
            focusManager.ProcessInput(inputInfo);
            if (sendResultSuccess)
            {
                grs.UpdateRanking(true);
                sendResultSuccess = false;
            }
            Update();
        }
    }
}
