using PPDFramework;
using PPDFramework.Web;
using PPDFrameworkCore;
using System;

namespace PPDSingle
{
    class MenuRanking : GameComponent
    {
        MenuRankingBoard globalRanking;
        MenuRankingBoard rivalRanking;
        Difficulty currentDifficulty = Difficulty.Normal;

        public Difficulty CurrentDifficulty
        {
            get { return currentDifficulty; }
            set
            {
                if (currentDifficulty != value)
                {
                    currentDifficulty = value;
                    globalRanking.CurrentDifficulty = rivalRanking.CurrentDifficulty = value;
                }
            }
        }

        public MenuRanking(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            AddChild(globalRanking = new MenuRankingBoard(device, resourceManager)
            {
                RankKindText = Utility.Language["GlobalRanking"]
            });
            AddChild(rivalRanking = new MenuRankingBoard(device, resourceManager)
            {
                RankKindText = Utility.Language["RivalRanking"],
                Position = new SharpDX.Vector2(0, 90)
            });
        }

        public void ChangeSongInfo(Func<Ranking> rankingFunc, Func<Ranking> rivalRankingFunc)
        {
            globalRanking.ChangeSongInfo(rankingFunc);
            if (rivalRankingFunc != null)
            {
                rivalRanking.ChangeSongInfo(rivalRankingFunc);
            }
            rivalRanking.Hidden = rivalRankingFunc == null;
        }
    }

    class MenuRankingBoard : GameComponent
    {
        PPDFramework.Resource.ResourceManager resourceManager;
        Ranking currentRanking;
        Difficulty currentDifficulty = Difficulty.Normal;
        Ranking ranking;
        bool findFinished;

        RankingChild[] ranks;

        TextureString yourRankText;
        TextureString rankKindText;

        public Difficulty CurrentDifficulty
        {
            get { return currentDifficulty; }
            set
            {
                if (currentDifficulty != value)
                {
                    currentDifficulty = value;
                    if (currentRanking != null)
                    {
                        UpdateDisplay();
                    }
                }
            }
        }

        public string RankKindText
        {
            get { return rankKindText.Text; }
            set { rankKindText.Text = value; }
        }

        public MenuRankingBoard(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            this.resourceManager = resourceManager;
            PictureObject info;
            this.AddChild(info = new PictureObject(device, resourceManager, Utility.Path.Combine("infoboard2.png")));
            this.AddChild(yourRankText = new TextureString(device, "", 12, PPDColors.White)
            {
                Position = new SharpDX.Vector2(280, -10),
                Alignment = Alignment.Right,
                Border = new Border
                {
                    Color = PPDColors.Black,
                    Thickness = 1
                }
            });
            this.AddChild(rankKindText = new TextureString(device, "", 12, PPDColors.White)
            {
                Position = new SharpDX.Vector2(5, -10),
                Border = new Border
                {
                    Color = PPDColors.Black,
                    Thickness = 1
                }
            });
            ranks = new RankingChild[3];
            info.AddChild(ranks[0] = new RankingChild(device, resourceManager) { Position = new SharpDX.Vector2(15, 10) });
            info.AddChild(ranks[1] = new RankingChild(device, resourceManager) { Position = new SharpDX.Vector2(15, 30) });
            info.AddChild(ranks[2] = new RankingChild(device, resourceManager) { Position = new SharpDX.Vector2(15, 50) });
        }

        public void ChangeSongInfo(Func<Ranking> rankingFunc)
        {
            currentRanking = null;
            foreach (RankingChild rank in ranks)
            {
                rank.Text = "";
                rank.Rank = -1;
            }
            yourRankText.Text = "";
            ranks[0].Text = Utility.Language["GettingRanking"];
            findFinished = false;
            ThreadManager.Instance.GetThread(() =>
            {
                ranking = rankingFunc();
                findFinished = true;
            }).Start();
        }

        protected override void UpdateImpl()
        {
            if (findFinished)
            {
                if (ranking != null)
                {
                    currentRanking = ranking;
                    ranking = null;
                    UpdateDisplay();
                }
                else
                {
                    ranks[0].Text = Utility.Language["GettingRankingFailed"];
                    currentRanking = null;
                }
                findFinished = false;
            }
        }

        private void UpdateDisplay()
        {
            foreach (RankingChild rank in ranks)
            {
                rank.Text = "";
                rank.Rank = -1;
            }
            yourRankText.Text = "";
            int iter = 0;
            var infos = currentRanking.GetInfo(currentDifficulty);
            if (infos == null)
            {
                ranks[0].Text = Utility.Language["InvalidRankingDifficulty"];
                return;
            }
            RankingInfo myRank = null;
            foreach (RankingInfo info in infos)
            {
                if (info.ID.ToLower() == WebManager.Instance.CurrentAccountId.ToLower())
                {
                    myRank = info;
                }
                if (iter < ranks.Length)
                {
                    ranks[iter].Text = String.Format("{0} {1}", info.Score, info.Nickname);
                    ranks[iter].Rank = info.Rank;
                }
                iter++;
            }
            yourRankText.Text = myRank == null ? Utility.Language["NotRankin"] : myRank.Rank + Utility.Language["Rank"];
        }

        class RankingChild : GameComponent
        {
            private int rank;

            private PictureObject gold;
            private PictureObject silver;
            private PictureObject bronze;
            private TextureString text;

            public string Text
            {
                get
                {
                    return text.Text;
                }
                set
                {
                    if (text.Text != value)
                    {
                        text.Text = value;
                        text.AllowScroll = true;
                    }
                }
            }

            public int Rank
            {
                get
                {
                    return rank;
                }
                set
                {
                    if (rank != value)
                    {
                        rank = value;
                        gold.Hidden = silver.Hidden = bronze.Hidden = true;
                        switch (rank)
                        {
                            case 1:
                                gold.Hidden = false;
                                break;
                            case 2:
                                silver.Hidden = false;
                                break;
                            case 3:
                                bronze.Hidden = false;
                                break;
                        }
                    }
                }
            }

            public RankingChild(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
            {
                gold = new PictureObject(device, resourceManager, Utility.Path.Combine("1st.png"));
                silver = new PictureObject(device, resourceManager, Utility.Path.Combine("2nd.png"));
                bronze = new PictureObject(device, resourceManager, Utility.Path.Combine("3rd.png"));
                gold.Hidden = silver.Hidden = bronze.Hidden = true;
                text = new TextureString(device, "", 12, 230, PPDColors.White)
                {
                    Position = new SharpDX.Vector2(25, 0),
                    AllowScroll = true
                };

                Rank = 1;
                this.AddChild(gold);
                this.AddChild(silver);
                this.AddChild(bronze);
                this.AddChild(text);
            }
        }
    }
}
