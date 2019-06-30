using PPDFramework;
using PPDFrameworkCore;
using PPDShareComponent;
using SharpDX;
using System;
using System.Drawing;
using System.IO;

namespace PPDSingle
{
    class GameResultScore : FocusableGameComponent
    {
        public event EventHandler Returned;
        public event EventHandler Retryed;
        public event EventHandler Replayed;

        enum State
        {
            Waiting = -1,
            CoolCounting = 0,
            GoodCounting = 1,
            SafeCounting = 2,
            SadCounting = 3,
            WorstCounting = 4,
            ComboCounting = 5,
            ScoreCounting = 6,
            Done = 7,
            WaitFadeForRetry = 8,
            WaitFadeForReplay = 9,
            WaitFadeForReturn = 10,
        }

        State state = State.Waiting;
        EffectObject[] result;
        EffectObject high;
        NumberPictureObject[] scoresmalls;
        NumberPictureObject scorebig;
        PictureObject scoreboard;
        PictureObject top;
        PictureObject bottom;
        PictureObject difficulty;
        BackGroundDisplay bgd;
        RectangleComponent black;
        TextureString songname;
        TextureString difficultstring;
        MenuRanking menuRanking;

        Button[] buttons;

        int scorex = 388;
        int[] scorey = { 150, 180, 210, 240, 270, 300, 373 };
        GridSelection gridSelection = new GridSelection();

        ISound Sound;
        IGameHost gameHost;

        TweetDialog td;
        ReviewDialog rd;
        ITweetManager TweetManager;
        IReviewManager ReviewManager;
        PPDGameUtility ppdGameUtility;

        public GameResultScore(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager,
            PPDGameUtility ppdGameUtility, ISound sound, ITweetManager tweetManager, IReviewManager reviewManager, IGameHost gameHost) : base(device)
        {
            this.ppdGameUtility = ppdGameUtility;
            this.TweetManager = tweetManager;
            this.ReviewManager = reviewManager;
            this.Sound = sound;
            this.gameHost = gameHost;
            menuRanking = new MenuRanking(device, resourceManager)
            {
                Position = new Vector2(455, 100)
            };
            td = new TweetDialog(device, resourceManager, sound, tweetManager);
            rd = new ReviewDialog(device, resourceManager, sound, reviewManager);
            this.AddChild(td);
            this.AddChild(rd);
            result = new EffectObject[6];
            for (int i = 0; i < result.Length; i++)
            {
                int x = 220;
                int y = 74;
                var anim = new EffectObject(device, resourceManager, Utility.Path.Combine("result", String.Format("{0}.etd", ((ResultEvaluateType)i).ToString().ToLower())))
                {
                    Position = new Vector2(x, y)
                };
                anim.PlayType = Effect2D.EffectManager.PlayType.ReverseLoop;
                anim.Play();
                result[i] = anim;
            }
            string filename = "";
            switch (ppdGameUtility.Difficulty)
            {
                case Difficulty.Easy:
                    filename = "easy.png";
                    break;
                case Difficulty.Normal:
                    filename = "normal.png";
                    break;
                case Difficulty.Hard:
                    filename = "hard.png";
                    break;
                case Difficulty.Extreme:
                    filename = "extreme.png";
                    break;
                default:
                    filename = "normal.png";
                    break;
            }
            difficulty = new PictureObject(device, resourceManager, Utility.Path.Combine("result", filename))
            {
                Position = new Vector2(45, 105)
            };
            high = new EffectObject(device, resourceManager, Utility.Path.Combine("result", "high.etd"))
            {
                Position = new Vector2(-25, 300)
            };
            high.PlayType = Effect2D.EffectManager.PlayType.Loop;
            high.Play();
            high.Alignment = EffectObject.EffectAlignment.TopLeft;
            scoresmalls = new NumberPictureObject[6];
            for (int i = 0; i < scoresmalls.Length; i++)
            {
                scoresmalls[i] = new NumberPictureObject(device, resourceManager, Utility.Path.Combine("result", "scoresmall.png"))
                {
                    Position = new Vector2(scorex, scorey[i]),
                    Alignment = Alignment.Right,
                    MaxDigit = -1
                };
            }
            scorebig = new NumberPictureObject(device, resourceManager, Utility.Path.Combine("result", "scorebig.png"))
            {
                Position = new Vector2(scorex, scorey[6]),
                Alignment = Alignment.Right,
                MaxDigit = -1
            };
            scoreboard = new PictureObject(device, resourceManager, Utility.Path.Combine("result", "score.png"))
            {
                Position = new Vector2(18, 52)
            };
            scoreboard.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("eva", "cool.png"), true)
            {
                Position = new Vector2(105, 108),
                Scale = new Vector2(0.75f, 0.75f)
            });
            scoreboard.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("eva", "good.png"), true)
            {
                Position = new Vector2(105, 138),
                Scale = new Vector2(0.75f, 0.75f)
            });
            scoreboard.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("eva", "safe.png"), true)
            {
                Position = new Vector2(105, 168),
                Scale = new Vector2(0.75f, 0.75f)
            });
            scoreboard.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("eva", "sad.png"), true)
            {
                Position = new Vector2(105, 198),
                Scale = new Vector2(0.75f, 0.75f)
            });
            scoreboard.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("eva", "worst.png"), true)
            {
                Position = new Vector2(105, 228),
                Scale = new Vector2(0.75f, 0.75f)
            });
            scoreboard.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("eva", "combo.png"), true)
            {
                Position = new Vector2(105, 258),
                Scale = new Vector2(0.75f, 0.75f)
            });

            top = new PictureObject(device, resourceManager, Utility.Path.Combine("result", "top.png"));
            top.AddChild(new TextureString(device, Utility.Language["Result"], 20, PPDColors.White)
            {
                Position = new Vector2(70, 2)
            });
            bottom = new PictureObject(device, resourceManager, Utility.Path.Combine("result", "bottom.png"));
            bottom.AddChild(new TextureString(device, Utility.Language["Move"], 16, PPDColors.White)
            {
                Position = new Vector2(587, 21)
            });
            bottom.AddChild(new TextureString(device, Utility.Language["Decide"], 16, PPDColors.White)
            {
                Position = new Vector2(708, 21)
            });
            bottom.Position = new Vector2(0, 450 - bottom.Height + 1);
            black = new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800
            };
            black.Alpha = 0;
            buttons = new Button[5];
            for (int i = 0; i < buttons.Length; i++)
            {
                string text = "";
                switch (i)
                {
                    case 0:
                        text = "TWEET";
                        break;
                    case 1:
                        text = "REVIEW";
                        break;
                    case 2:
                        text = "RETRY";
                        break;
                    case 3:
                        text = "REPLAY";
                        break;
                    case 4:
                        text = "RETURN";
                        break;
                }
                Vector2 pos;
                if (i >= 2)
                {
                    pos = new Vector2(500 + ((i - 2) % 3) * 100, 380);
                }
                else
                {
                    pos = new Vector2(500 + (i % 2) * 100, 340);
                }
                buttons[i] = new Button(device, resourceManager, Utility.Path, text)
                {
                    Position = pos
                };
                gridSelection.Add(pos);
                buttons[i].Selected = false;
                this.AddChild(buttons[i]);
            }
            gridSelection.Initialize();
            songname = new TextureString(device, ppdGameUtility.SongInformation.DirectoryName, 20, 500, PPDColors.White)
            {
                Position = new Vector2(35, 4)
            };
            songname.Position = new Vector2(790 - songname.JustWidth, songname.Position.Y);
            difficultstring = new TextureString(device, ppdGameUtility.DifficultString, 24, 200, PPDColors.White)
            {
                Position = new Vector2(35, 103),
                AllowScroll = true
            };
            difficultstring.Position = new Vector2(400 - Math.Min(200, difficultstring.Width), difficultstring.Position.Y);

            bgd = new BackGroundDisplay(device, resourceManager, "skins\\PPDSingle_BackGround.xml", "Result");

            Inputed += GameResultScore_Inputed;
            GotFocused += GameResultScore_GotFocused;

            this.AddChild(menuRanking);
            this.AddChild(high);
            this.AddChild(difficulty);
            for (int i = 0; i < result.Length; i++)
            {
                this.AddChild(result[i]);
            }
            for (int i = 0; i < scoresmalls.Length; i++)
            {
                this.AddChild(scoresmalls[i]);
            }
            this.AddChild(scorebig);
            this.AddChild(difficultstring);
            this.AddChild(scoreboard);
            this.AddChild(songname);
            this.AddChild(top);
            this.AddChild(bottom);
            this.AddChild(bgd);

            this.InsertChild(black, 0);
        }

        void GameResultScore_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            if (args.FocusObject == td)
            {
                if (!TweetManager.CanTweet)
                {
                    gridSelection.SetAt(buttons[2].Position);
                    buttons[0].Selected = false;
                    buttons[0].Enabled = false;
                    buttons[2].Selected = true;
                }
            }
            else if (args.FocusObject == rd)
            {
                if (!ReviewManager.CanReview)
                {
                    gridSelection.SetAt(buttons[2].Position);
                    buttons[1].Selected = buttons[1].Enabled = false;
                    buttons[2].Selected = true;
                }
            }
        }

        public void ResultSet(int score, int[] markevals, int maxcombo, ResultEvaluateType result, bool highscore, bool canReplay)
        {
            Score = score;
            MarkEvals = markevals;
            MaxCombo = maxcombo;
            Result = result;
            HighScore = highscore;
            state = State.CoolCounting;
            buttons[gridSelection.Current].Selected = false;
            gridSelection.SetAt(buttons[2].Position);
            buttons[2].Selected = true;
            buttons[0].Enabled = TweetManager.CanTweet;
            buttons[1].Enabled = ReviewManager.CanReview;
            buttons[3].Enabled = canReplay;
            foreach (NumberPictureObject picture in scoresmalls)
            {
                picture.Hidden = true;
            }
            scorebig.Hidden = true;
            high.Hidden = true;
            foreach (EffectObject res in this.result)
            {
                res.Hidden = true;
            }
            foreach (Button button in buttons)
            {
                button.Hidden = true;
            }
            menuRanking.Hidden = true;
            menuRanking.CurrentDifficulty = ppdGameUtility.Difficulty;
        }

        public void UpdateRanking(bool forceUpdate = false)
        {
            if (ppdGameUtility.RankingUpdateFunc == null)
            {
                menuRanking.ChangeSongInfo(() => ppdGameUtility.SongInformation.GetRanking(forceUpdate),
                    () => ppdGameUtility.SongInformation.GetRivalRanking(forceUpdate));
            }
            else
            {
                menuRanking.ChangeSongInfo(ppdGameUtility.RankingUpdateFunc, null);
            }
        }

        public void Retry()
        {
            foreach (NumberPictureObject npo in scoresmalls) npo.Value = 0;
            scorebig.Value = 0;
            black.Alpha = 0;
            state = State.Waiting;
        }

        public int Score
        {
            get;
            private set;
        }

        public int[] MarkEvals
        {
            get;
            private set;
        }

        public int MaxCombo
        {
            get;
            private set;
        }

        public ResultEvaluateType Result
        {
            get;
            private set;
        }

        public bool HighScore
        {
            get;
            private set;
        }

        void GameResultScore_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (state == State.Done)
            {
                // カーソル移動
                if (args.InputInfo.IsPressed(ButtonType.Left))
                {
                    buttons[gridSelection.Current].Selected = false;
                    do
                    {
                        gridSelection.Left();
                    } while (!buttons[gridSelection.Current].Enabled);

                    buttons[gridSelection.Current].Selected = true;
                    Sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
                if (args.InputInfo.IsPressed(ButtonType.Right))
                {
                    buttons[gridSelection.Current].Selected = false;
                    do
                    {
                        gridSelection.Right();
                    } while (!buttons[gridSelection.Current].Enabled);
                    buttons[gridSelection.Current].Selected = true;
                    Sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
                if (args.InputInfo.IsPressed(ButtonType.Up))
                {
                    buttons[gridSelection.Current].Selected = false;
                    do
                    {
                        gridSelection.Up();
                        if (!buttons[gridSelection.Current].Enabled)
                        {
                            gridSelection.Left();
                        }
                    } while (!buttons[gridSelection.Current].Enabled);
                    buttons[gridSelection.Current].Selected = true;
                    Sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }
                if (args.InputInfo.IsPressed(ButtonType.Down))
                {
                    buttons[gridSelection.Current].Selected = false;
                    do
                    {
                        gridSelection.Down();
                        if (!buttons[gridSelection.Current].Enabled)
                        {
                            gridSelection.Left();
                        }
                    } while (!buttons[gridSelection.Current].Enabled);
                    buttons[gridSelection.Current].Selected = true;
                    Sound.Play(PPDSetting.DefaultSounds[0], -1000);
                }


                // 丸だったら戻る準備
                if (args.InputInfo.IsPressed(ButtonType.Circle))
                {
                    switch (gridSelection.Current)
                    {
                        case 0:
                            FocusManager.Focus(td);
                            break;
                        case 1:
                            FocusManager.Focus(rd);
                            break;
                        case 2:
                            state = State.WaitFadeForRetry;
                            break;
                        case 3:
                            state = State.WaitFadeForReplay;
                            break;
                        case 4:
                            state = State.WaitFadeForReturn;
                            break;
                    }
                }
            }
            if ((args.InputInfo.IsPressed(ButtonType.Circle) ||
                args.InputInfo.IsPressed(ButtonType.Left) ||
                args.InputInfo.IsPressed(ButtonType.Right)) && state < State.Done)
            {
                state = State.Done;
                for (int i = 0; i < scoresmalls.Length; i++)
                {
                    if (i >= MarkEvals.Length) scoresmalls[i].Value = (uint)MaxCombo;
                    else scoresmalls[i].Value = (uint)MarkEvals[i];
                }
                scorebig.Value = (uint)Score;
                ShowAll();
            }
        }

        private void SaveScreenShot()
        {
            if (TweetManager.CanTweet)
            {
                TweetManager.TweetFilePath = "result.png";
                gameHost.SaveScreenShot(TweetManager.TweetFilePath, s =>
                {
                    using (Bitmap bitmap = new Bitmap(s))
                    using (Graphics g = Graphics.FromImage(bitmap))
                    using (System.Drawing.Font font = new System.Drawing.Font(PPDSetting.Setting.FontName, 20))
                    {
                        var text = TweetManager.FinishDate.ToString();
                        var size = g.MeasureString(text, font);
                        var p = new PointF(0, 450 - size.Height);
                        g.DrawString(text, font, Brushes.Black, new PointF(p.X - 1, p.Y - 1));
                        g.DrawString(text, font, Brushes.Black, new PointF(p.X + 1, p.Y - 1));
                        g.DrawString(text, font, Brushes.Black, new PointF(p.X - 1, p.Y + 1));
                        g.DrawString(text, font, Brushes.Black, new PointF(p.X + 1, p.Y + 1));
                        g.DrawString(text, font, Brushes.White, p);
                        bitmap.Save("temp.png");
                    }
                    try
                    {
                        File.Delete("result.png");
                        File.Move("temp.png", "result.png");
                    }
                    catch (Exception)
                    {
                    }
                });
            }
        }

        protected override void UpdateImpl()
        {
            if (state >= State.WaitFadeForRetry)
            {
                if (black.Alpha >= 1)
                {
                    switch (state)
                    {
                        case State.WaitFadeForRetry:
                            // リトライ
                            if (this.Retryed != null)
                            {
                                this.Retryed(this, EventArgs.Empty);
                                state = State.Waiting;
                                black.Alpha = 0;
                            }

                            break;
                        case State.WaitFadeForReturn:
                            // リターン
                            if (this.Returned != null)
                            {
                                this.Returned(this, EventArgs.Empty);
                                state = State.Waiting;
                                black.Alpha = 0;
                                return;
                            }

                            break;
                        case State.WaitFadeForReplay:
                            if (this.Replayed != null)
                            {
                                this.Replayed(this, EventArgs.Empty);
                                state = State.Waiting;
                                black.Alpha = 0;
                                return;
                            }

                            break;
                    }
                }
                else
                {
                    black.Alpha += 0.04f;
                    if (black.Alpha >= 1)
                    {
                        black.Alpha = 1;
                    }
                }
            }
            // カウントしていく
            if (state > State.Waiting)
            {
                if (state < State.ComboCounting)
                {
                    int delta = (MarkEvals[(int)state] / 60 < 1 ? 1 : MarkEvals[(int)state] / 60);
                    scoresmalls[(int)state].Hidden = false;
                    scoresmalls[(int)state].Value += (uint)delta;
                    if (scoresmalls[(int)state].Value >= MarkEvals[(int)state])
                    {
                        scoresmalls[(int)state].Value = (uint)MarkEvals[(int)state];
                        state++;
                    }
                }
                if (state == State.ComboCounting)
                {
                    int delta = (MaxCombo / 60 < 1 ? 1 : MaxCombo / 60);
                    scoresmalls[(int)state].Value += (uint)delta;
                    scoresmalls[(int)state].Hidden = false;
                    if (scoresmalls[(int)state].Value >= MaxCombo)
                    {
                        scoresmalls[(int)state].Value = (uint)MaxCombo;
                        state = State.ScoreCounting;
                    }
                }
                if (state == State.ScoreCounting)
                {
                    int delta = (Score / 60 > 99 ? Score / 60 : 99);
                    scorebig.Value += (uint)delta;
                    scorebig.Hidden = false;
                    if (scorebig.Value >= Score)
                    {
                        scorebig.Value = (uint)Score;
                        state = State.Done;
                        ShowAll();
                    }
                }
            }
        }

        protected override bool OnCanUpdate()
        {
            return !Hidden && OverFocused;
        }

        private void ShowAll()
        {
            foreach (NumberPictureObject picture in scoresmalls)
            {
                picture.Hidden = false;
            }
            scorebig.Hidden = false;
            high.Hidden = !HighScore;
            result[(int)Result].Hidden = false;
            foreach (Button button in buttons)
            {
                button.Hidden = false;
            }
            menuRanking.Hidden = false;
            SaveScreenShot();
        }

        protected override bool OnCanDraw(PPDFramework.Shaders.AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return OverFocused;
        }
    }
}
