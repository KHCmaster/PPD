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
    public class GameResult(PPDDevice device) : GameResultBase(device)
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

        public override void Load()
        {
            focusManager = new FocusManager();
            ResultSet += GameResult_ResultSet;
            ScoreSent += GameResult_ScoreSent;
            ScoreSending += GameResult_ScoreSending;
            grs = new GameResultScore(device, ResourceManager, PPDGameUtility, Sound, BlueSkyManager, ReviewManager, GameHost);
            grs.Retryed += Grs_Retryed;
            grs.Returned += Grs_Returned;
            grs.Replayed += Grs_Replayed;
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
                if (data.TryGetValue("Exp", out string expValue))
                {
                    var exp = int.Parse(expValue);
                    GameHost.AddNotify(String.Format(Utility.Language["GotExp"], exp));
                }
                if (data.TryGetValue("Item", out string itemValue))
                {
                    var itemType = (ItemType)int.Parse(itemValue);
                    GameHost.AddNotify(String.Format(Utility.Language["GotItem"], Utility.Language[String.Format("Item{0}Name", itemType)]));
                }
                if (data.TryGetValue("LastLevel", out string lastLevelValue) && data.TryGetValue("Level", out string levelValue))
                {
                    int lastLevel = int.Parse(lastLevelValue), level = int.Parse(levelValue);
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

        void Grs_Returned(object sender, EventArgs e)
        {
            this.Returned?.Invoke(this, EventArgs.Empty);
        }

        void Grs_Retryed(object sender, EventArgs e)
        {
            this.Retryed?.Invoke(this, EventArgs.Empty);
        }

        void Grs_Replayed(object sender, EventArgs e)
        {
            this.Replayed?.Invoke(this, EventArgs.Empty);
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
