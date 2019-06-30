using PPDFramework;
using PPDFrameworkCore;
using PPDShareComponent;
using SharpDX;
using System;

namespace PPDSingle
{
    /// <summary>
    /// 譜面情報表示UIクラス
    /// </summary>
    class SongInfoControl : FocusableGameComponent
    {
        enum State
        {
            normal = 0,
            focused = 1,
            fadeout = 2,
            fadein = 3
        }
        Difficulty selectdifficulty;
        SelectedSongInfo songinfo;
        State state = State.normal;
        TextureString focusedbpm;
        TextureString hiscore;
        TextureString difficulty;
        TextureString[] infos;
        PictureObject[] infoboards;
        Button[] buttons;
        string[] score = new string[4];
        bool[] exist = new bool[6];
        const int xx = 610;
        const int xxx = 640;
        const int height = 220;
        const int secondheight = 250;
        MenuRanking menuRanking;

        BestEvaluate[] bestEvaluates;
        PictureObject[] acs;
        PictureObject[] fts;
        PictureObject[] perfects;
        PictureObject[] buttonAcs;
        PictureObject[] buttonFts;
        PictureObject[] buttonPerfects;

        public SongInfoControl(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            menuRanking = new MenuRanking(device, resourceManager)
            {
                Position = new Vector2(500, 105)
            };
            menuRanking.Hidden = true;
            this.AddChild(menuRanking);
            state = State.normal;
            infoboards = new PictureObject[2];
            bestEvaluates = new BestEvaluate[4];
            buttonAcs = new PictureObject[4];
            buttonFts = new PictureObject[4];
            buttonPerfects = new PictureObject[4];
            acs = new PictureObject[4];
            fts = new PictureObject[4];
            perfects = new PictureObject[4];
            infoboards[0] = new PictureObject(device, resourceManager, Utility.Path.Combine("infoboard1.png"))
            {
                Position = new Vector2(520, height - 14)
            };
            infoboards[1] = new PictureObject(device, resourceManager, Utility.Path.Combine("infoboard2.png"))
            {
                Position = new Vector2(500, 280)
            };
            infoboards[1].Hidden = true;
            this.AddChild(infoboards[0]);
            this.AddChild(infoboards[1]);
            TextureString easy, normal, hard, extreme;
            infoboards[0].AddChild(new TextureString(device, "BPM", 20, PPDColors.White)
            {
                Position = new Vector2(0, 15)
            });
            infoboards[0].AddChild(easy = new TextureString(device, "EASY", 20, PPDColors.White)
            {
                Position = new Vector2(0, 45)
            });
            infoboards[0].AddChild(normal = new TextureString(device, "NORMAL", 20, PPDColors.White)
            {
                Position = new Vector2(0, 75)
            });
            infoboards[0].AddChild(hard = new TextureString(device, "HARD", 20, PPDColors.White)
            {
                Position = new Vector2(0, 105)
            });
            infoboards[0].AddChild(extreme = new TextureString(device, "EXTREME", 20, PPDColors.White)
            {
                Position = new Vector2(0, 135)
            });
            infoboards[0].AddChild(new TextureString(device, "S.A.", 20, PPDColors.White)
            {
                Position = new Vector2(0, 165)
            });
            foreach (GameComponent gc in infoboards[0].Children)
            {
                gc.Position = new Vector2(90 - gc.Width, gc.Position.Y);
            }
            for (int i = 0; i < 4; i++)
            {
                infoboards[0].InsertChild(acs[i] = new PictureObject(device, resourceManager, Utility.Path.Combine("ac.png"))
                {
                    Position = new Vector2(0, 35 + 30 * i)
                }, 0);

                infoboards[0].InsertChild(fts[i] = new PictureObject(device, resourceManager, Utility.Path.Combine("ft.png"))
                {
                    Position = new Vector2(0, 35 + 30 * i)
                }, 0);

                infoboards[0].InsertChild(perfects[i] = new PictureObject(device, resourceManager, Utility.Path.Combine("perfect.png"))
                {
                    Position = new Vector2(35, 35 + 30 * i)
                }, 0);

                infoboards[0].InsertChild(bestEvaluates[i] = new BestEvaluate(device, resourceManager)
                {
                    Position = new Vector2(80, 35 + 30 * i)
                }, 0);
            }
            infos = new TextureString[6];
            for (int i = 0; i < 6; i++)
            {
                var st = new TextureString(device, "", 20, 150, PPDColors.White)
                {
                    Position = new Vector2(100, 15 + 30 * i)
                };
                infos[i] = st;
                infoboards[0].AddChild(st);
            }
            focusedbpm = new TextureString(device, "BPM", 12, true, PPDColors.White)
            {
                Position = new Vector2(140, 10)
            };
            hiscore = new TextureString(device, "BPM", 12, true, PPDColors.White)
            {
                Position = new Vector2(140, 30)
            };
            difficulty = new TextureString(device, "BPM", 12, true, PPDColors.White)
            {
                Position = new Vector2(140, 50)
            };
            infoboards[1].AddChild(focusedbpm);
            infoboards[1].AddChild(hiscore);
            infoboards[1].AddChild(difficulty);
            buttons = new Button[4];
            for (int i = 0; i < buttons.Length; i++)
            {
                string text = "";
                switch (i)
                {
                    case 0:
                        text = "EASY";
                        break;
                    case 1:
                        text = "NORMAL";
                        break;
                    case 2:
                        text = "HARD";
                        break;
                    case 3:
                        text = "EXTREME";
                        break;
                }
                infoboards[1].AddChild(buttonAcs[i] = new PictureObject(device, resourceManager, Utility.Path.Combine("ac.png"))
                {
                    Position = new Vector2(85 * i - 40, 75)
                });
                infoboards[1].AddChild(buttonFts[i] = new PictureObject(device, resourceManager, Utility.Path.Combine("ft.png"))
                {
                    Position = new Vector2(85 * i - 40, 75)
                });
                infoboards[1].AddChild(buttonPerfects[i] = new PictureObject(device, resourceManager, Utility.Path.Combine("perfect.png"))
                {
                    Position = new Vector2(85 * i - 40 + 30, 75)
                });
                buttons[i] = new Button(device, resourceManager, Utility.Path, text)
                {
                    Position = new Vector2(85 * i, 100)
                };
                infoboards[1].AddChild(buttons[i]);
            }

            GotFocused += SongInfoControl_GotFocused;
            LostFocused += SongInfoControl_LostFocused;
        }

        void SongInfoControl_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            state = State.normal;
        }

        void SongInfoControl_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            if (args.FocusObject.GetType() == typeof(MenuSelectSong))
            {
                if (songinfo is ContestSelectedSongInfo)
                {
                    var contest = songinfo as ContestSelectedSongInfo;
                    menuRanking.ChangeSongInfo(contest.GetRanking, null);
                }
                else
                {
                    menuRanking.ChangeSongInfo(() => songinfo.SongInfo.GetRanking(), () => songinfo.SongInfo.GetRivalRanking());
                }
                InnerFocus();
            }
            else
            {
                FocusManager.RemoveFocus();
            }
        }

        private void InnerFocus()
        {
            state = State.focused;
            infoboards[0].Hidden = true;
            infoboards[1].Hidden = false;
            menuRanking.Hidden = false;
            selectdifficulty = Difficulty.Normal;
            int count = 0;
            while (!exist[(int)selectdifficulty + 1] && count < 3)
            {
                selectdifficulty += 1;
                if (selectdifficulty > Difficulty.Extreme) selectdifficulty = Difficulty.Easy;
                if (selectdifficulty < 0) selectdifficulty = Difficulty.Extreme;
                count++;
            }
            if (count >= 4)
            {
                selectdifficulty = Difficulty.Other;
                return;
            }
            focusedbpm.Text = "BPM " + infos[0].Text;
            hiscore.Text = String.Format("{0} {1}", Utility.Language["HighScore"], score[(int)selectdifficulty]);
            switch (selectdifficulty)
            {
                case Difficulty.Easy:
                    difficulty.Text = infos[1].Text;
                    break;
                case Difficulty.Normal:
                    difficulty.Text = infos[2].Text;
                    break;
                case Difficulty.Hard:
                    difficulty.Text = infos[3].Text;
                    break;
                case Difficulty.Extreme:
                    difficulty.Text = infos[4].Text;
                    break;
            }
            focusedbpm.Alpha = 1f;
            difficulty.Alpha = 1f;
            hiscore.Alpha = 1f;
            foreach (Button button in buttons)
            {
                button.Alpha = 1f;
                button.Selected = false;
            }
            buttons[(int)selectdifficulty].Selected = true;
            menuRanking.CurrentDifficulty = selectdifficulty;
        }

        /// <summary>
        /// 譜面変更
        /// </summary>
        /// <param name="songinfo"></param>
        public void ChangeSongInfo(SelectedSongInfo songinfo)
        {
            this.songinfo = songinfo;
            HideInfo();

            if (songinfo.SongInfo == null || !songinfo.SongInfo.IsPPDSong) return;
            exist = new bool[6];
            exist[0] = true;
            exist[1] = (songinfo.Difficulty & SongInformation.AvailableDifficulty.Easy) == SongInformation.AvailableDifficulty.Easy;
            exist[2] = (songinfo.Difficulty & SongInformation.AvailableDifficulty.Normal) == SongInformation.AvailableDifficulty.Normal;
            exist[3] = (songinfo.Difficulty & SongInformation.AvailableDifficulty.Hard) == SongInformation.AvailableDifficulty.Hard;
            exist[4] = (songinfo.Difficulty & SongInformation.AvailableDifficulty.Extreme) == SongInformation.AvailableDifficulty.Extreme;
            exist[5] = songinfo.SongInfo.IsPPDSong;
            infos[0].Text = songinfo.SongInfo.GetBPMString();
            infos[5].Text = songinfo.SongInfo.AuthorName;
            for (int i = 1; i <= 4; i++)
            {
                infos[i].Text = exist[i] ? songinfo.SongInfo.GetDifficultyString((Difficulty)(i - 1)) : "";
                buttons[i - 1].Enabled = exist[i];
                buttonAcs[i - 1].Hidden = acs[i - 1].Hidden = !exist[i] || !songinfo.SongInfo.GetIsAC((Difficulty)(i - 1));
                buttonFts[i - 1].Hidden = fts[i - 1].Hidden = !exist[i] || !songinfo.SongInfo.GetIsACFT((Difficulty)(i - 1));
            }
            UpdatePerfectTrials();
            foreach (TextureString str in infos)
            {
                str.AllowScroll = true;
            }

            UpdateResult();
        }

        public void UpdatePerfectTrials()
        {
            for (int i = 1; i <= 4; i++)
            {
                buttonPerfects[i - 1].Hidden = perfects[i - 1].Hidden = !exist[i] || !songinfo.Perfects[i - 1];
            }
        }

        public void UpdateResult()
        {
            for (int i = 1; i <= 4; i++)
            {
                score[i - 1] = songinfo.SongInfo.GetScore((Difficulty)(i - 1));
            }

            var temp = PPDFramework.ResultInfo.GetInfoFromSongInformation(songinfo.SongInfo);

            foreach (PPDFramework.ResultInfo resultInfo in temp)
            {
                if (resultInfo.ResultEvaluate != ResultEvaluateType.Mistake && resultInfo.Difficulty < Difficulty.Other
                    && songinfo.SongInfo.Difficulty.HasFlag(SongInformation.ConvertDifficulty(resultInfo.Difficulty)))
                {
                    if (bestEvaluates[(int)resultInfo.Difficulty].Result < resultInfo.ResultEvaluate)
                    {
                        bestEvaluates[(int)resultInfo.Difficulty].Result = resultInfo.ResultEvaluate;
                    }
                }
            }
        }

        public void HideInfo()
        {
            exist = new bool[6];
            foreach (BestEvaluate be in bestEvaluates)
            {
                be.Result = ResultEvaluateType.Mistake;
            }
            foreach (TextureString st in infos)
            {
                st.Text = "";
            }
            foreach (PictureObject po in acs)
            {
                po.Hidden = true;
            }
            foreach (PictureObject po in fts)
            {
                po.Hidden = true;
            }
            foreach (PictureObject po in perfects)
            {
                po.Hidden = true;
            }
        }
        public bool CanGoNext()
        {
            return selectdifficulty != Difficulty.Other;
        }

        protected override void UpdateImpl()
        {
            if (state == State.fadeout)
            {
                for (int i = 0; i < infoboards.Length; i++)
                {
                    infoboards[i].Alpha -= 0.05f;
                    if (infoboards[i].Alpha <= 0)
                    {
                        infoboards[i].Alpha = 0;
                    }
                }
                menuRanking.Alpha = infoboards[1].Alpha;
            }
            else
            {
                for (int i = 0; i < infoboards.Length; i++)
                {
                    infoboards[i].Alpha += 0.05f;
                    if (infoboards[i].Alpha >= 1)
                    {
                        infoboards[i].Alpha = 1;
                    }
                }
                menuRanking.Alpha = infoboards[1].Alpha;
            }
        }

        protected override bool OnCanUpdate()
        {
            return !Hidden;
        }

        /// <summary>
        /// 難易度変更
        /// </summary>
        /// <param name="diff"></param>
        public bool ChangeDifficulty(int diff)
        {
            Difficulty last = selectdifficulty;
            selectdifficulty += diff;
            if (selectdifficulty > Difficulty.Extreme) selectdifficulty = Difficulty.Easy;
            if (selectdifficulty < Difficulty.Easy) selectdifficulty = Difficulty.Extreme;
            int count = 0;
            while (!exist[(int)selectdifficulty + 1] && count < 4)
            {
                selectdifficulty += diff;
                if (selectdifficulty > Difficulty.Extreme) selectdifficulty = Difficulty.Easy;
                if (selectdifficulty < Difficulty.Easy) selectdifficulty = Difficulty.Extreme;
                count++;
            }
            if (count >= 4)
            {
                selectdifficulty = Difficulty.Other;
                return false;
            }
            switch (selectdifficulty)
            {
                case Difficulty.Easy:
                    difficulty.Text = infos[1].Text;
                    break;
                case Difficulty.Normal:
                    difficulty.Text = infos[2].Text;
                    break;
                case Difficulty.Hard:
                    difficulty.Text = infos[3].Text;
                    break;
                case Difficulty.Extreme:
                    difficulty.Text = infos[4].Text;
                    break;
            }
            hiscore.Text = String.Format("{0} {1}", Utility.Language["HighScore"], score[(int)selectdifficulty]);
            buttons[(int)last].Selected = false;
            buttons[(int)selectdifficulty].Selected = true;
            menuRanking.CurrentDifficulty = selectdifficulty;
            return last != selectdifficulty;
        }
        public string Difficult
        {
            get
            {
                switch (selectdifficulty)
                {
                    case Difficulty.Easy:
                        return "EASY";
                    case Difficulty.Normal:
                        return "NORMAL";
                    case Difficulty.Hard:
                        return "HARD";
                    case Difficulty.Extreme:
                        return "EXTREME";
                    default:
                        return "";
                }
            }
        }
        public Difficulty Difficulty
        {
            get
            {
                return selectdifficulty;
            }
        }
        public string DiifficultyString
        {
            get
            {
                return songinfo.SongInfo.GetDifficultyString(selectdifficulty);
            }
        }
        public float BPM
        {
            get
            {
                if (!float.TryParse(infos[0].Text, out float ret))
                {
                    ret = 100;
                }
                return ret;
            }
        }
        public void FadeOut()
        {
            state = State.fadeout;
        }

        public void Show()
        {
            state = State.normal;
            infoboards[0].Hidden = false;
            infoboards[1].Hidden = true;
            menuRanking.Hidden = true;
        }

        class BestEvaluate : GameComponent
        {
            PPDFramework.Resource.ResourceManager resourceManager;

            ResultEvaluateType result;

            public BestEvaluate(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
            {
                this.resourceManager = resourceManager;

                this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("cheap_star.png")));
                this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("standard_star.png")));
                this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("great_star.png")));
                this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("excellent_star.png")));
                this.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("perfect_star.png")));

                Result = ResultEvaluateType.Mistake;
            }

            public ResultEvaluateType Result
            {
                get
                {
                    return result;
                }
                set
                {
                    result = value;
                    foreach (GameComponent gc in Children)
                    {
                        gc.Hidden = true;
                    }

#pragma warning disable RECS0093 // Convert 'if' to '&&' expression
                    if (value != ResultEvaluateType.Mistake)
#pragma warning restore RECS0093 // Convert 'if' to '&&' expression
                    {
                        this[(int)value - 1].Hidden = false;
                    }
                }
            }
        }
    }
}
