using PPDExpansion.Model;
using PPDExpansionCore;
using PPDFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace PPDExpansion.ViewModel
{
    class MultiPlayViewModel : PlotViewModel
    {
        private int score;
        private int coolCount;
        private int goodCount;
        private int safeCount;
        private int sadCount;
        private int worstCount;
        private GridLength chartHeight;
        int lastTime;
        float fLastTime;
        private string scoreName;
        private Difficulty difficulty;

        private Dictionary<int, Player> dict;

        public override PlayType PlayType
        {
            get { return PlayType.MultiPlay; }
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

        public override int ActualYMaximum
        {
            get
            {
                int max = 0;
                if (currentScoreInfo != null)
                {
                    max = Math.Max(max, Math.Max(currentScoreInfo.WebHighScore, currentScoreInfo.UserHighScore));
                }
                if (Players != null && Players.Count > 0)
                {
                    foreach (var player in Players)
                    {
                        if (player.Scores.Count > 0)
                        {
                            max = Math.Max(max, player.Scores[player.Scores.Count - 1].Value);
                        }
                    }
                }
                return max;
            }
        }

        public ObservableCollection<Player> Players
        {
            get;
            private set;
        }

        public ObservableCollection<Item> Items
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
                    PPDExpansionSetting.Setting.MultiPlayChartHeight = chartHeight = value;
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

        public MultiPlayViewModel(MainWindowViewModel mainWindowViewModel)
            : base(mainWindowViewModel)
        {
            Players = new ObservableCollection<Player>();
            Items = new ObservableCollection<Item>();
            dict = new Dictionary<int, Player>();
            ChartHeight = PPDExpansionSetting.Setting.MultiPlayChartHeight;
            SetScoreInfo(new ScoreInfo
            {
                EndTime = 60,
                StartTime = 0,
                UserHighScore = 0,
                WebHighScore = 0,
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
            else if (packable is PlayerInfo)
            {
                AddPlayerInfo((PlayerInfo)packable);
            }
            else if (packable is UpdateInfo)
            {
                AddUpdateInfo((UpdateInfo)packable);
            }
            else if (packable is ItemInfo)
            {
                AddItemInfo((ItemInfo)packable);
            }
        }

        private void SetScoreInfo(ScoreInfo scoreInfo)
        {
            currentScoreInfo = scoreInfo;
            dispatcher.Invoke((Action)(() =>
            {
                ScoreName = currentScoreInfo.ScoreName;
                Difficulty = currentScoreInfo.Difficulty;
                ResetZoom();
                Players.Clear();
                Items.Clear();
                dict.Clear();
            }));
        }

        private void AddPlayerInfo(PlayerInfo playerInfo)
        {
            dispatcher.Invoke((Action)(() =>
            {
                var player = new Player(playerInfo);
                Players.Add(player);
                dict.Add(playerInfo.PlayerId, player);
            }));
        }

        private void AddUpdateInfo(UpdateInfo updateInfo)
        {
            dispatcher.Invoke((Action)(() =>
            {
                if (!dict.TryGetValue(updateInfo.PlayerId, out Player player))
                {
                    return;
                }
                if (lastTime != (int)updateInfo.CurrentTime || fLastTime == updateInfo.CurrentTime)
                {
                    player.AddScore(baseTime.AddSeconds(updateInfo.CurrentTime - currentScoreInfo.StartTime), updateInfo.Score);
                    lastTime = (int)updateInfo.CurrentTime;
                    fLastTime = updateInfo.CurrentTime;
                    ResetZoom();
                }
                if (player.PlayerInfo.IsSelf)
                {
                    Score = updateInfo.Score;
                    CoolCount = updateInfo.CoolCount;
                    GoodCount = updateInfo.GoodCount;
                    SafeCount = updateInfo.SafeCount;
                    SadCount = updateInfo.SadCount;
                    WorstCount = updateInfo.WorstCount;
                }
            }));
        }

        private void AddItemInfo(ItemInfo itemInfo)
        {
            dispatcher.Invoke((Action)(() =>
            {
                if (!dict.TryGetValue(itemInfo.PlayerId, out Player player))
                {
                    return;
                }

                Items.Insert(0, new Item(itemInfo.ItemType, player.PlayerInfo.UserName, new TimeSpan(0, 0, (int)(itemInfo.CurrentTime - currentScoreInfo.StartTime))));
            }));
        }
    }
}
