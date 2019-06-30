using PPDExpansion.Model;
using PPDExpansionCore;
using PPDFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace PPDExpansion.ViewModel
{
    class SinglePlayViewModel : PlotViewModel
    {
        private int score;
        private int coolCount;
        private int goodCount;
        private int safeCount;
        private int sadCount;
        private int worstCount;
        private int userHighScore;
        private int webHighScore;
        private ObservableCollection<KeyValuePair<DateTime, int>> scores;
        private ObservableCollection<KeyValuePair<DateTime, int>> bestScores;
        private GridLength chartHeight;

        private string scoreName;
        private Difficulty difficulty;

        private ResultInfo bestResultInfo;
        private ResultInfo currentResultInfo;
        private Result bestResult;
        private int lastTime;
        private bool isBestScore;

        private string scorePercent;
        private string scoreWhitePercent;
        private string userHighScorePercent;
        private string userHighScoreWhitePercent;
        private string userHighScoreRealTimePercent;
        private string userHighScoreRealTimeWhitePercent;
        private string webHighScoreWhitePercent;
        private string webHighScorePercent;
        private int highScoreScoreDiff;
        private bool isHighScoreScoreDiffNegative;
        private int realTimeScoreDiff;
        private bool isRealTimeScoreDiffNagative;

        public override PlayType PlayType
        {
            get { return PlayType.SinglePlay; }
        }

        public override int ActualYMaximum
        {
            get
            {
                int max = 0;
                if (currentScoreInfo != null)
                {
                    max = Math.Max(max, Math.Max(currentScoreInfo.WebHighScore, currentScoreInfo.UserHighScore));
                }
                if (currentResultInfo != null)
                {
                    max = Math.Max(max, currentResultInfo.Score);
                }
                if (Scores != null && Scores.Count > 0)
                {
                    max = Math.Max(max, Scores[Scores.Count - 1].Value);
                }
                if (BestScores != null && BestScores.Count > 0)
                {
                    max = Math.Max(max, BestScores[BestScores.Count - 1].Value);
                }
                return max;
            }
        }

        public int Score
        {
            get { return score; }
            set
            {
                if (score != value)
                {
                    score = value;
                    RaisePropertyChanged("Score");
                }
            }
        }

        public int CoolCount
        {
            get { return coolCount; }
            set
            {
                if (coolCount != value)
                {
                    coolCount = value;
                    RaisePropertyChanged("CoolCount");
                }
            }
        }

        public int GoodCount
        {
            get { return goodCount; }
            set
            {
                if (goodCount != value)
                {
                    goodCount = value;
                    RaisePropertyChanged("GoodCount");
                }
            }
        }

        public int SafeCount
        {
            get { return safeCount; }
            set
            {
                if (safeCount != value)
                {
                    safeCount = value;
                    RaisePropertyChanged("SafeCount");
                }
            }
        }

        public int SadCount
        {
            get { return sadCount; }
            set
            {
                if (sadCount != value)
                {
                    sadCount = value;
                    RaisePropertyChanged("SadCount");
                }
            }
        }

        public int WorstCount
        {
            get { return worstCount; }
            set
            {
                if (worstCount != value)
                {
                    worstCount = value;
                    RaisePropertyChanged("WorstCount");
                }
            }
        }

        public int UserHighScore
        {
            get { return userHighScore; }
            set
            {
                if (userHighScore != value)
                {
                    userHighScore = value;
                    RaisePropertyChanged("UserHighScore");
                }
            }
        }

        public int WebHighScore
        {
            get { return webHighScore; }
            set
            {
                if (webHighScore != value)
                {
                    webHighScore = value;
                    RaisePropertyChanged("WebHighScore");
                }
            }
        }

        public ObservableCollection<KeyValuePair<DateTime, int>> Scores
        {
            get { return scores; }
            private set
            {
                if (scores != value)
                {
                    scores = value;
                    RaisePropertyChanged("Scores");
                }
            }
        }

        public ObservableCollection<KeyValuePair<DateTime, int>> BestScores
        {
            get { return bestScores; }
            private set
            {
                if (bestScores != value)
                {
                    bestScores = value;
                    RaisePropertyChanged("BestScores");
                }
            }
        }

        public ObservableCollection<KeyValuePair<DateTime, int>> HighScores
        {
            get;
            private set;
        }

        public ObservableCollection<KeyValuePair<DateTime, int>> WebHighScores
        {
            get;
            private set;
        }

        public ObservableCollection<ResultInfo> Results
        {
            get;
            private set;
        }

        public GridLength ChartHeight
        {
            get { return chartHeight; }
            set
            {
                if (chartHeight != value)
                {
                    PPDExpansionSetting.Setting.SinglePlayChartHeight = chartHeight = value;
                    RaisePropertyChanged("ChartHeight");
                }
            }
        }

        public string ScoreName
        {
            get { return scoreName; }
            set
            {
                if (scoreName != value)
                {
                    scoreName = value;
                    RaisePropertyChanged("ScoreName");
                }
            }
        }

        public Difficulty Difficulty
        {
            get { return difficulty; }
            set
            {
                if (difficulty != value)
                {
                    difficulty = value;
                    RaisePropertyChanged("Difficulty");
                }
            }
        }

        public string ScorePercent
        {
            get { return scorePercent; }
            set
            {
                if (scorePercent != value)
                {
                    scorePercent = value;
                    RaisePropertyChanged("ScorePercent");
                }
            }
        }

        public string ScoreWhitePercent
        {
            get { return scoreWhitePercent; }
            set
            {
                if (scoreWhitePercent != value)
                {
                    scoreWhitePercent = value;
                    RaisePropertyChanged("ScoreWhitePercent");
                }
            }
        }

        public string UserHighScorePercent
        {
            get { return userHighScorePercent; }
            set
            {
                if (userHighScorePercent != value)
                {
                    userHighScorePercent = value;
                    RaisePropertyChanged("UserHighScorePercent");
                }
            }
        }

        public string UserHighScoreWhitePercent
        {
            get { return userHighScoreWhitePercent; }
            set
            {
                if (userHighScoreWhitePercent != value)
                {
                    userHighScoreWhitePercent = value;
                    RaisePropertyChanged("UserHighScoreWhitePercent");
                }
            }
        }

        public string UserHighScoreRealTimePercent
        {
            get { return userHighScoreRealTimePercent; }
            set
            {
                if (userHighScoreRealTimePercent != value)
                {
                    userHighScoreRealTimePercent = value;
                    RaisePropertyChanged("UserHighScoreRealTimePercent");
                }
            }
        }

        public string UserHighScoreRealTimeWhitePercent
        {
            get { return userHighScoreRealTimeWhitePercent; }
            set
            {
                if (userHighScoreRealTimeWhitePercent != value)
                {
                    userHighScoreRealTimeWhitePercent = value;
                    RaisePropertyChanged("UserHighScoreRealTimeWhitePercent");
                }
            }
        }

        public string WebHighScorePercent
        {
            get { return webHighScorePercent; }
            set
            {
                if (webHighScorePercent != value)
                {
                    webHighScorePercent = value;
                    RaisePropertyChanged("WebHighScorePercent");
                }
            }
        }

        public string WebHighScoreWhitePercent
        {
            get { return webHighScoreWhitePercent; }
            set
            {
                if (webHighScoreWhitePercent != value)
                {
                    webHighScoreWhitePercent = value;
                    RaisePropertyChanged("WebHighScoreWhitePercent");
                }
            }
        }

        public int HighScoreScoreDiff
        {
            get { return highScoreScoreDiff; }
            set
            {
                if (highScoreScoreDiff != value)
                {
                    highScoreScoreDiff = value;
                    RaisePropertyChanged("HighScoreScoreDiff");
                }
            }
        }

        public bool IsHighScoreScoreDiffNegative
        {
            get { return isHighScoreScoreDiffNegative; }
            set
            {
                if (isHighScoreScoreDiffNegative != value)
                {
                    isHighScoreScoreDiffNegative = value;
                    RaisePropertyChanged("IsHighScoreScoreDiffNegative");
                }
            }
        }

        public int RealTimeScoreDiff
        {
            get { return realTimeScoreDiff; }
            set
            {
                if (realTimeScoreDiff != value)
                {
                    realTimeScoreDiff = value;
                    RaisePropertyChanged("RealTimeScoreDiff");
                }
            }
        }

        public bool IsRealTimeScoreDiffNagative
        {
            get { return isRealTimeScoreDiffNagative; }
            set
            {
                if (isRealTimeScoreDiffNagative != value)
                {
                    isRealTimeScoreDiffNagative = value;
                    RaisePropertyChanged("IsRealTimeScoreDiffNagative");
                }
            }
        }

        public SinglePlayViewModel(MainWindowViewModel mainWindowViewModel)
            : base(mainWindowViewModel)
        {
            Scores = new ObservableCollection<KeyValuePair<DateTime, int>>();
            HighScores = new ObservableCollection<KeyValuePair<DateTime, int>>();
            WebHighScores = new ObservableCollection<KeyValuePair<DateTime, int>>();
            Results = new ObservableCollection<ResultInfo>();
            ChartHeight = PPDExpansionSetting.Setting.SinglePlayChartHeight;
            SetScoreInfo(new ScoreInfo
            {
                EndTime = 60,
                StartTime = 0,
                UserHighScore = 10000,
                WebHighScore = 10000,
                ScoreHash = new byte[0],
                ScoreName = "",
                Difficulty = Difficulty.Other
            });
        }

        public override void ProcessData(PackableBase packable)
        {
            if (packable is ScoreInfo)
            {
                SetScoreInfo((ScoreInfo)packable);
            }
            else if (packable is UpdateInfo)
            {
                AddUpdateInfo((UpdateInfo)packable);
            }
            else if (packable is ResultInfo)
            {
                SetResultInfo((ResultInfo)packable);
            }
        }

        private void SetScoreInfo(ScoreInfo scoreInfo)
        {
            var lastScoreInfo = currentScoreInfo;
            currentScoreInfo = scoreInfo;
            lastTime = 0;
            dispatcher.Invoke((Action)(() =>
            {
                ResetZoom();
                HighScores.Clear();
                WebHighScores.Clear();
                HighScores.Add(new KeyValuePair<DateTime, int>(XMinimum, scoreInfo.UserHighScore));
                HighScores.Add(new KeyValuePair<DateTime, int>(XMaximum, scoreInfo.UserHighScore));
                WebHighScores.Add(new KeyValuePair<DateTime, int>(XMinimum, scoreInfo.WebHighScore));
                WebHighScores.Add(new KeyValuePair<DateTime, int>(XMaximum, scoreInfo.WebHighScore));
                Score = CoolCount = GoodCount = SafeCount = SadCount = WorstCount = 0;
                UserHighScore = scoreInfo.UserHighScore;
                WebHighScore = scoreInfo.WebHighScore;
                ScoreName = currentScoreInfo.ScoreName;
                Difficulty = currentScoreInfo.Difficulty;
                if (lastScoreInfo != null && lastScoreInfo.ScoreHashAsString != currentScoreInfo.ScoreHashAsString)
                {
                    BestScores = null;
                    bestResult = ExpansionDatabase.Instance.FindResult(currentScoreInfo.ScoreHashAsString);
                    if (bestResult != null)
                    {
                        BestScores = new ObservableCollection<KeyValuePair<DateTime, int>>(bestResult.Data);
                    }
                    Scores.Clear();
                    Results.Clear();
                }
                else
                {
                    if (isBestScore)
                    {
                        BestScores = Scores;
                        bestResultInfo = currentResultInfo;
                    }
                    Scores = new ObservableCollection<KeyValuePair<DateTime, int>>();
                }
                isBestScore = false;
                UpdateScorePercent();
            }));
        }

        private void UpdateScorePercent()
        {
            var highScore = Math.Max(score, Math.Max(userHighScore, webHighScore));
            ScorePercent = String.Format("{0}*", (float)score / highScore);
            ScoreWhitePercent = String.Format("{0}*", (float)(highScore - score) / highScore);
            UserHighScorePercent = String.Format("{0}*", (float)userHighScore / highScore);
            if (BestScores != null && BestScores.Count > 0)
            {
                KeyValuePair<DateTime, int> best;
                try
                {
                    best = BestScores.First(p => p.Key >= baseTime.AddSeconds(lastTime - currentScoreInfo.StartTime));
                }
                catch
                {
                    best = new KeyValuePair<DateTime, int>(DateTime.Now, userHighScore);
                }
                UserHighScoreRealTimePercent = String.Format("{0}*", (float)best.Value / userHighScore);
                UserHighScoreRealTimeWhitePercent = String.Format("{0}*", (float)(userHighScore - best.Value) / userHighScore);
                RealTimeScoreDiff = score - best.Value;
            }
            else
            {
                UserHighScoreRealTimePercent = "1*";
                userHighScoreRealTimeWhitePercent = "0*";
                RealTimeScoreDiff = score - userHighScore;
            }
            UserHighScoreWhitePercent = String.Format("{0}*", (float)(highScore - userHighScore) / highScore);
            WebHighScorePercent = String.Format("{0}*", (float)webHighScore / highScore);
            WebHighScoreWhitePercent = String.Format("{0}*", (float)(highScore - webHighScore) / highScore);
            HighScoreScoreDiff = score - Math.Max(userHighScore, webHighScore);
            IsHighScoreScoreDiffNegative = HighScoreScoreDiff < 0;
            IsRealTimeScoreDiffNagative = RealTimeScoreDiff < 0;
        }

        private void AddUpdateInfo(UpdateInfo updateInfo)
        {
            dispatcher.Invoke((Action)(() =>
            {
                if (lastTime != (int)updateInfo.CurrentTime)
                {
                    // リトライ後にリトライ前の時間でデータを送信してくることがあるので
                    // 一度もデータが記録されていないときに開始時間よりも２秒大きいデータは無視する
                    if (Scores.Count > 0 || updateInfo.CurrentTime <= currentScoreInfo.StartTime + 2)
                    {
                        Scores.Add(new KeyValuePair<DateTime, int>(baseTime.AddSeconds(updateInfo.CurrentTime - currentScoreInfo.StartTime), updateInfo.Score));
                        lastTime = (int)updateInfo.CurrentTime;
                        ResetZoom();
                    }
                }
                Score = updateInfo.Score;
                CoolCount = updateInfo.CoolCount;
                GoodCount = updateInfo.GoodCount;
                SafeCount = updateInfo.SafeCount;
                SadCount = updateInfo.SadCount;
                WorstCount = updateInfo.WorstCount;
                UpdateScorePercent();
            }));
        }

        private void SetResultInfo(ResultInfo resultInfo)
        {
            currentResultInfo = resultInfo;
            dispatcher.Invoke((Action)(() =>
            {
                resultInfo.DateTime = DateTime.Now;
                Results.Insert(0, resultInfo);
                if (currentScoreInfo.IsRegular)
                {
                    if (bestResult == null || bestResult.Score < currentResultInfo.Score)
                    {
                        bestResult = new Result(currentScoreInfo.ScoreHashAsString, currentResultInfo.Score, Scores.ToArray());
                        ExpansionDatabase.Instance.InsertOrUpdateResult(bestResult);
                        isBestScore = true;
                    }
                }
            }));
        }
    }
}
