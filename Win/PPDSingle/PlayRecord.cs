using PPDFramework;
using PPDFrameworkCore;
using PPDShareComponent;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PPDSingle
{
    class PlayRecord : FocusableGameComponent
    {
        enum GraphDrawType
        {
            Score = 0,
            CoolCount = 1,
            GoodCount = 2,
            SafeCount = 3,
            SadCount = 4,
            WorstCount = 5,
            MaxCombo = 6,
            FinishTime = 7
        }

        string[] graphNameStrings;

        GraphDrawType graphDrawType = GraphDrawType.Score;

        PictureObject back;
        RectangleComponent black;
        PictureObject top;
        PictureObject bottom;
        PictureObject triangle;

        TextureString difficulty;
        string[] difficultyStrings = { "Easy", "Normal", "Hard", "Extreme" };
        Difficulty selectedDifficulty;
        SongInformation selectedSongInformation;

        GraphDrawer gd;
        PPDFramework.ResultInfo[] results;
        List<PPDFramework.ResultInfo> currentDifficultyRecults = new List<PPDFramework.ResultInfo>();
        ISound sound;
        PPDFramework.Resource.ResourceManager resourceManager;

        TextureString cool;
        TextureString good;
        TextureString safe;
        TextureString sad;
        TextureString worst;
        TextureString maxCombo;

        TextureString songname;

        LineRectangleComponent rectangle;

        int selectedIndex = -1;

        SpriteObject sprite;

        EffectObject cursor;

        const int contentHeight = 19;
        const int maxContentNum = 14;

        public PlayRecord(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound) : base(device)
        {
            this.sound = sound;
            this.resourceManager = resourceManager;

            graphNameStrings = new string[] { Utility.Language["Score"], "Cool", "Good", "Safe", "Sad", "Worst", "MaxCombo", Utility.Language["FinishTime"] };

            back = new PictureObject(device, resourceManager, Utility.Path.Combine("playrecord", "back.png"));
            back.AddChild(new TextureString(device, Utility.Language["PlayRecord"], 20, PPDColors.White)
            {
                Position = new Vector2(30, 28)
            });
            top = new PictureObject(device, resourceManager, Utility.Path.Combine("playrecord", "recordtop.png"))
            {
                Position = new Vector2(440, 70)
            };
            top.AddChild(new TextureString(device, Utility.Language["Difficulty"], 14, true, PPDColors.White)
            {
                Position = new Vector2(33, 4)
            });
            top.AddChild(new TextureString(device, Utility.Language["Score"], 14, true, PPDColors.White)
            {
                Position = new Vector2(105, 4)
            });
            top.AddChild(new TextureString(device, Utility.Language["Result"], 14, true, PPDColors.White)
            {
                Position = new Vector2(181, 4)
            });
            top.AddChild(new TextureString(device, Utility.Language["PlayDate"], 14, true, PPDColors.White)
            {
                Position = new Vector2(269, 4)
            });
            bottom = new PictureObject(device, resourceManager, Utility.Path.Combine("playrecord", "recordbottom.png"))
            {
                Position = new Vector2(440, 364)
            };
            triangle = new PictureObject(device, resourceManager, Utility.Path.Combine("playrecord", "triangle.png"), true)
            {
                Position = new Vector2(300, 410),
                Hidden = true
            };
            black = new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0.65f
            };
            cursor = new EffectObject(device, resourceManager, Utility.Path.Combine("playrecord", "cursor.etd"))
            {
                PlayType = Effect2D.EffectManager.PlayType.ReverseLoop
            };
            cursor.Play();

            cool = new TextureString(device, "", 14, PPDColors.White)
            {
                Position = new Vector2(510, 369)
            };
            good = new TextureString(device, "", 14, PPDColors.White)
            {
                Position = new Vector2(510, 388)
            };
            safe = new TextureString(device, "", 14, PPDColors.White)
            {
                Position = new Vector2(510, 409)
            };
            sad = new TextureString(device, "", 14, PPDColors.White)
            {
                Position = new Vector2(705, 369)
            };
            worst = new TextureString(device, "", 14, PPDColors.White)
            {
                Position = new Vector2(705, 388)
            };
            maxCombo = new TextureString(device, "", 14, PPDColors.White)
            {
                Position = new Vector2(705, 409)
            };

            songname = new TextureString(device, "", 20, PPDColors.White)
            {
                Position = new Vector2(250, 30)
            };

            difficulty = new TextureString(device, "Easy", 20, PPDColors.White);
            difficulty.Position = new Vector2(240 - difficulty.Width / 2, 80 - difficulty.CharacterHeight / 2);

            gd = new GraphDrawer(device, resourceManager)
            {
                Position = new Vector2(30, 120),
                GraphWidth = 390,
                Name = Utility.Language["Score"],
                NamePositionCenter = new Vector2(220, 285)
            };

            sprite = new SpriteObject(device)
            {
                Position = new Vector2(440, 98)
            };

            rectangle = new LineRectangleComponent(device, resourceManager, PPDColors.Selection)
            {
                RectangleWidth = 314,
                RectangleHeight = 16,
                BorderThickness = 2
            };

            Inputed += PlayRecord_Inputed;
            GotFocused += PlayRecord_GotFocused;

            this.AddChild(cursor);
            this.AddChild(gd);
            this.AddChild(difficulty);
            this.AddChild(songname);
            this.AddChild(cool);
            this.AddChild(good);
            this.AddChild(safe);
            this.AddChild(sad);
            this.AddChild(worst);
            this.AddChild(maxCombo);
            this.AddChild(triangle);
            this.AddChild(top);
            this.AddChild(bottom);
            this.AddChild(rectangle);
            this.AddChild(sprite);
            this.AddChild(back);
            this.AddChild(black);
        }

        void PlayRecord_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            if (args.FocusObject is LeftMenu lm)
            {
                songname.Text = lm.SelectedSongInformation.DirectoryName;
                songname.Position = new Vector2(750 - songname.Width, songname.Position.Y);
                selectedSongInformation = lm.SelectedSongInformation;
                results = PPDFramework.ResultInfo.GetInfoFromSongInformation(lm.SelectedSongInformation);
                Array.Reverse(results);
                graphDrawType = GraphDrawType.Score;
                selectedDifficulty = Difficulty.Extreme;
                int iter = 0;
                while (!CheckExist() && iter < 4)
                {
                    selectedDifficulty++;
                    if (selectedDifficulty >= Difficulty.Other)
                    {
                        selectedDifficulty = Difficulty.Easy;
                    }
                    iter++;
                }
                ChangeResultTableDifficulty();
                ChangeGraphData();
                ChangeResultTable();
            }
        }

        private void ChangeResultTableDifficulty()
        {
            currentDifficultyRecults.Clear();
            sprite.ClearChildren();
            for (int i = 0; i < results.Length; i++)
            {
                if (selectedDifficulty == results[i].Difficulty)
                {
                    currentDifficultyRecults.Add(results[i]);
                }
            }

            int iter = 0;
            foreach (PPDFramework.ResultInfo result in currentDifficultyRecults)
            {
                var tempSprite = new SpriteObject(device)
                {
                    Position = new Vector2(0, iter * 19)
                };
                tempSprite.AddChild(new TextureString(device, result.Difficulty.ToString(), 12, true, PPDColors.White)
                {
                    Position = new Vector2(35, 2)
                });
                tempSprite.AddChild(new TextureString(device, String.Format("{0:D7}", result.Score), 12, true, PPDColors.White)
                {
                    Position = new Vector2(105, 2)
                });
                tempSprite.AddChild(new TextureString(device, result.ResultEvaluate.ToString(), 12, true, PPDColors.White)
                {
                    Position = new Vector2(180, 2)
                });
                tempSprite.AddChild(new TextureString(device, result.Date.ToShortDateString(), 12, true, PPDColors.White)
                {
                    Position = new Vector2(270, 2)
                });
                tempSprite.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("playrecord", "recordcontent.png")));
                sprite.AddChild(tempSprite);
            }
        }

        private void ChangeGraphData()
        {
            var list = new List<PPDFramework.ResultInfo>();
            Array.ForEach<PPDFramework.ResultInfo>(results, (resultInfo =>
            {
                if (resultInfo.Difficulty == selectedDifficulty)
                {
                    list.Add(resultInfo);
                }
            }));

            // 右側最近
            list.Reverse();

            var values = new List<GraphData>();
            list.ForEach(resultInfo =>
            {
                var data = new GraphData
                {
                    XValue = resultInfo.Date.ToString()
                };
                switch (graphDrawType)
                {
                    case GraphDrawType.Score:
                        data.YValue = resultInfo.Score;
                        break;
                    case GraphDrawType.CoolCount:
                        data.YValue = resultInfo.CoolCount;
                        break;
                    case GraphDrawType.GoodCount:
                        data.YValue = resultInfo.GoodCount;
                        break;
                    case GraphDrawType.SafeCount:
                        data.YValue = resultInfo.SafeCount;
                        break;
                    case GraphDrawType.SadCount:
                        data.YValue = resultInfo.SadCount;
                        break;
                    case GraphDrawType.WorstCount:
                        data.YValue = resultInfo.WorstCount;
                        break;
                    case GraphDrawType.MaxCombo:
                        data.YValue = resultInfo.MaxCombo;
                        break;
                    case GraphDrawType.FinishTime:
                        data.YValue = resultInfo.FinishTime;
                        break;
                }
                values.Add(data);
            });

            if (graphDrawType == GraphDrawType.FinishTime)
            {
                gd.Formatter = FloatToTimeFormatter.Formatter;
            }
            else
            {
                gd.Formatter = FloatToIntFormatter.Formatter;
            }
            gd.Data = values.ToArray();

            gd.Name = graphNameStrings[(int)graphDrawType];
            gd.NamePositionCenter = new Vector2(220, 285);

            difficulty.Text = String.Format("{0}({1}/{2})", difficultyStrings[(int)selectedDifficulty], list.Count, results.Length);
            difficulty.Position = new Vector2(240 - difficulty.Width / 2, 80 - difficulty.CharacterHeight / 2);

            cursor.Hidden = values.Count < 2;
            triangle.Hidden = graphDrawType != GraphDrawType.FinishTime;
        }

        private void ChangeResultTable()
        {
            selectedIndex = -1;
            if (currentDifficultyRecults.Count > 0)
            {
                selectedIndex = 0;
                SetResultInfo();
            }
            else
            {
                cool.Text = "";
                good.Text = "";
                safe.Text = "";
                sad.Text = "";
                worst.Text = "";
                maxCombo.Text = "";
            }
        }

        private void SetResultInfo()
        {
            cool.Text = currentDifficultyRecults[selectedIndex].CoolCount.ToString();
            good.Text = currentDifficultyRecults[selectedIndex].GoodCount.ToString();
            safe.Text = currentDifficultyRecults[selectedIndex].SafeCount.ToString();
            sad.Text = currentDifficultyRecults[selectedIndex].SadCount.ToString();
            worst.Text = currentDifficultyRecults[selectedIndex].WorstCount.ToString();
            maxCombo.Text = currentDifficultyRecults[selectedIndex].MaxCombo.ToString();
        }

        void PlayRecord_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                FocusManager.RemoveFocus();
                sound.Play(PPDSetting.DefaultSounds[2], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Left))
            {
                graphDrawType--;
                if (graphDrawType < 0)
                {
                    graphDrawType = GraphDrawType.FinishTime;
                }
                ChangeGraphData();
                sound.Play(PPDSetting.DefaultSounds[3], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Right))
            {
                graphDrawType++;
                if (graphDrawType > GraphDrawType.FinishTime)
                {
                    graphDrawType = GraphDrawType.Score;
                }
                ChangeGraphData();
                sound.Play(PPDSetting.DefaultSounds[3], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.R))
            {
                Difficulty last = selectedDifficulty;
                selectedDifficulty++;
                int iter = 0;
                while (!CheckExist() && iter < 4)
                {
                    selectedDifficulty++;
                    if (selectedDifficulty >= Difficulty.Other)
                    {
                        selectedDifficulty = Difficulty.Easy;
                    }
                    iter++;
                }
                selectedIndex = 0;
                ChangeResultTableDifficulty();
                ChangeGraphData();
                ChangeResultTable();
                if (last != selectedDifficulty)
                {
                    sound.Play(PPDSetting.DefaultSounds[3], -1000);
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.L))
            {
                Difficulty last = selectedDifficulty;
                selectedDifficulty--;
                int iter = 0;
                while (!CheckExist() && iter < 4)
                {
                    selectedDifficulty--;
                    if (selectedDifficulty < Difficulty.Easy)
                    {
                        selectedDifficulty = Difficulty.Extreme;
                    }
                    iter++;
                }
                selectedIndex = 0;
                ChangeResultTableDifficulty();
                ChangeGraphData();
                ChangeResultTable();
                if (last != selectedDifficulty)
                {
                    sound.Play(PPDSetting.DefaultSounds[3], -1000);
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                if (currentDifficultyRecults.Count > 0)
                {
                    selectedIndex--;
                    if (selectedIndex < 0)
                    {
                        selectedIndex = 0;
                    }
                    else
                    {
                        SetResultInfo();
                        sound.Play(PPDSetting.DefaultSounds[0], -1000);
                    }
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                if (currentDifficultyRecults.Count > 0)
                {
                    selectedIndex++;
                    if (selectedIndex >= currentDifficultyRecults.Count)
                    {
                        selectedIndex = currentDifficultyRecults.Count - 1;
                    }
                    else
                    {
                        SetResultInfo();
                        sound.Play(PPDSetting.DefaultSounds[0], -1000);
                    }
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Triangle))
            {
                if (graphDrawType == GraphDrawType.FinishTime && currentDifficultyRecults.Count > 0)
                {
                    var ppd = new PreviewPlayDialog(device, resourceManager, sound)
                    {
                        SongName = songname.Text,
                        Difficulty = difficultyStrings[(int)selectedDifficulty],
                        StartTime = FloatToFloatFormatter.Formatter.Format(currentDifficultyRecults[selectedIndex].FinishTime)
                    };
                    FocusManager.Focus(ppd);
                    this.InsertChild(ppd, 0);
                    ppd.LostFocused += ppd_LostFocused;
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Square))
            {
                if (currentDifficultyRecults.Count > 0)
                {
                    var drd = new GeneralDialog(device, resourceManager, sound, Utility.Language["DeleteRecordConfirm"], GeneralDialog.ButtonTypes.OkCancel);
                    FocusManager.Focus(drd);
                    this.InsertChild(drd, 0);
                    drd.LostFocused += drd_LostFocused;
                }
            }
        }

        void drd_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            var drd = sender as GeneralDialog;
            if (drd.OK)
            {
                if (currentDifficultyRecults.Count > 0)
                {
                    PPDFramework.ResultInfo resultInfo = currentDifficultyRecults[selectedIndex];
                    resultInfo.Delete();
                    results = results.Where(r => r != resultInfo).ToArray();
                    selectedIndex--;
                    if (selectedIndex < 0)
                    {
                        selectedIndex++;
                    }
                    if (selectedIndex >= currentDifficultyRecults.Count)
                    {
                        selectedIndex = currentDifficultyRecults.Count - 1;
                    }
                    ChangeResultTableDifficulty();
                    ChangeGraphData();
                    ChangeResultTable();
                }
            }
            this.RemoveChild(drd);
            drd.Dispose();
        }

        void ppd_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            var ppd = sender as PreviewPlayDialog;
            if (ppd.OK)
            {
                if (FocusManager.BaseScene is Menu menu)
                {
                    menu.PreviewPlay(selectedDifficulty, currentDifficultyRecults[selectedIndex].FinishTime - ppd.MinusTime);
                }
            }
            this.RemoveChild(ppd);
            ppd.Dispose();
        }

        private bool CheckExist()
        {
            switch (selectedDifficulty)
            {
                case Difficulty.Easy:
                    return (selectedSongInformation.Difficulty & SongInformation.AvailableDifficulty.Easy) == SongInformation.AvailableDifficulty.Easy;
                case Difficulty.Normal:
                    return (selectedSongInformation.Difficulty & SongInformation.AvailableDifficulty.Normal) == SongInformation.AvailableDifficulty.Normal;
                case Difficulty.Hard:
                    return (selectedSongInformation.Difficulty & SongInformation.AvailableDifficulty.Hard) == SongInformation.AvailableDifficulty.Hard;
                case Difficulty.Extreme:
                    return (selectedSongInformation.Difficulty & SongInformation.AvailableDifficulty.Extreme) == SongInformation.AvailableDifficulty.Extreme;
            }
            return false;
        }

        protected override void UpdateImpl()
        {
            if (sprite.ChildrenCount > 0)
            {
                float y = sprite[selectedIndex].Position.Y;
                if (y <= 0)
                {
                    sprite[selectedIndex].Position = new Vector2(0, y - y * 0.3f);
                }
                else if (y >= (maxContentNum - 1) * contentHeight)
                {
                    sprite[selectedIndex].Position = new Vector2(0, y + ((maxContentNum - 1) * contentHeight - y) * 0.3f);
                }
                for (int i = 0; i < sprite.ChildrenCount; i++)
                {
                    sprite[i].Position = new Vector2(sprite[i].Position.X, sprite[selectedIndex].Position.Y + contentHeight * (i - selectedIndex));
                }
                for (int i = 0; i < sprite.ChildrenCount; i++)
                {
                    if ((int)(sprite[i].Position.Y + contentHeight) <= +1 || (int)sprite[i].Position.Y >= maxContentNum * contentHeight - 1)
                    {
                        sprite[i].Hidden = true;
                    }
                    else
                    {
                        sprite[i].Hidden = false;
                    }
                }
                rectangle.Hidden = false;
                rectangle.Position = sprite[selectedIndex].Position + sprite.Position;
            }
            else
            {
                rectangle.Hidden = true;
            }

            if (selectedIndex >= 0 && selectedIndex < gd.Poses.Count)
            {
                cursor.Position = gd.Poses[gd.Poses.Count - 1 - selectedIndex] + gd.Position;
            }
        }

        protected override bool OnCanUpdate()
        {
            return !Hidden && OverFocused;
        }

        protected override bool OnCanDraw(PPDFramework.Shaders.AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return OverFocused;
        }
    }
}
