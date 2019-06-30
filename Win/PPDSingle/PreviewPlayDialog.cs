using PPDFramework;
using PPDShareComponent;
using SharpDX;
using System;

namespace PPDSingle
{
    class PreviewPlayDialog : FocusableGameComponent
    {
        EffectObject back;
        int selection;

        TextureString message;
        TextureString songname;
        TextureString difficulty;
        TextureString startTime;
        TextureString diffTime;

        TextureString songnameContent;
        TextureString difficultyContent;
        TextureString startTimeContent;
        TextureString diffTimeContent;


        SpriteObject sprite;

        RectangleComponent black;

        Button[] buttons;

        ISound Sound;

        int diffCount;

        public bool OK
        {
            get;
            private set;
        }

        public string SongName
        {
            get { return songnameContent.Text; }
            set
            {
                songnameContent.Text = value;
            }
        }

        public string Difficulty
        {
            get { return difficultyContent.Text; }
            set
            {
                difficultyContent.Text = value;
            }
        }

        public string StartTime
        {
            get { return startTimeContent.Text; }
            set
            {
                startTimeContent.Text = value;
            }
        }

        public int MinusTime
        {
            get { return diffCount; }
        }

        public PreviewPlayDialog(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound) : base(device)
        {
            this.Sound = sound;
            sprite = new SpriteObject(device);
            back = new EffectObject(device, resourceManager, Utility.Path.Combine("tweetconfirm.etd"))
            {
                Position = new Vector2(400, 225)
            };
            message = new TextureString(device, Utility.Language["PracticeScoreFromSpecificTime"], 16, PPDColors.White)
            {
                Position = new Vector2(300, 120)
            };
            message.Position = new Vector2(400 - message.Width / 2, message.Position.Y);
            songname = new TextureString(device, Utility.Language["Score"], 14, 350, PPDColors.White)
            {
                Position = new Vector2(225, 150)
            };
            difficulty = new TextureString(device, Utility.Language["Difficulty"], 14, 350, PPDColors.White)
            {
                Position = new Vector2(225, 180)
            };
            startTime = new TextureString(device, Utility.Language["StartTime"], 14, 350, PPDColors.White)
            {
                Position = new Vector2(225, 210)
            };
            diffTime = new TextureString(device, Utility.Language["StartMinusTime"], 14, 350, PPDColors.White)
            {
                Position = new Vector2(225, 240)
            };

            float x = diffTime.Position.X + diffTime.Width;
            songnameContent = new TextureString(device, "", 14, 180, PPDColors.White)
            {
                Position = new Vector2(x, 150),
                AllowScroll = true
            };
            difficultyContent = new TextureString(device, "", 14, true, PPDColors.White)
            {
                Position = new Vector2(480, 180)
            };
            startTimeContent = new TextureString(device, "", 14, true, PPDColors.White)
            {
                Position = new Vector2(480, 210)
            };
            diffTimeContent = new TextureString(device, "0", 14, true, PPDColors.White)
            {
                Position = new Vector2(480, 240)
            };

            black = new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800
            };

            buttons = new Button[2];
            for (int i = 0; i < buttons.Length; i++)
            {
                string text = "";
                switch (i)
                {
                    case 0:
                        text = Utility.Language["OK"];
                        break;
                    case 1:
                        text = Utility.Language["Cancel"];
                        break;
                }
                buttons[i] = new Button(device, resourceManager, Utility.Path, text)
                {
                    Position = new Vector2(300 + i * 200, 315)
                };
                buttons[i].Selected = false;
                sprite.AddChild(buttons[i]);
            }

            buttons[0].Selected = true;

            black.Alpha = 0;

            back.Hidden = true;
            sprite.Hidden = true;

            GotFocused += PreviewPlayDialog_GotFocused;
            LostFocused += PreviewPlayDialog_LostFocused;
            Inputed += PreviewPlayDialog_Inputed;

            sprite.AddChild(message);
            sprite.AddChild(songname);
            sprite.AddChild(difficulty);
            sprite.AddChild(startTime);
            sprite.AddChild(diffTime);
            sprite.AddChild(songnameContent);
            sprite.AddChild(difficultyContent);
            sprite.AddChild(startTimeContent);
            sprite.AddChild(diffTimeContent);
            sprite.AddChild(new PictureObject(device, resourceManager, Utility.Path.Combine("playrecord", "updown.png"), true)
            {
                Position = new Vector2(550, 250)
            });

            this.AddChild(sprite);
            this.AddChild(back);
            this.AddChild(black);
        }

        void PreviewPlayDialog_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                switch (selection)
                {
                    case 0:
                        OK = true;
                        Sound.Play(PPDSetting.DefaultSounds[1], -1000);
                        break;
                    case 1:
                        Sound.Play(PPDSetting.DefaultSounds[2], -1000);
                        break;
                }
                FocusManager.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                Sound.Play(PPDSetting.DefaultSounds[2], -1000);
                FocusManager.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Left))
            {
                buttons[selection].Selected = false;
                selection--;
                if (selection < 0)
                {
                    selection = 1;
                }
                buttons[selection].Selected = true;
                Sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Right))
            {
                buttons[selection].Selected = false;
                selection++;
                if (selection > 1)
                {
                    selection = 0;
                }
                buttons[selection].Selected = true;
                Sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                if (diffCount > 0)
                {
                    diffCount -= ((MenuInputInfo)args.InputInfo).GetPressingInterval(ButtonType.Down);
                    if (diffCount < 0)
                    {
                        diffCount = 0;
                    }
                    Sound.Play(PPDSetting.DefaultSounds[3], -1000);
                }
                diffTimeContent.Text = diffCount.ToString();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                diffCount += ((MenuInputInfo)args.InputInfo).GetPressingInterval(ButtonType.Up);
                diffTimeContent.Text = diffCount.ToString();
                Sound.Play(PPDSetting.DefaultSounds[3], -1000);
            }
        }

        void PreviewPlayDialog_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            back.Stop();
            back.PlayType = Effect2D.EffectManager.PlayType.ReverseOnce;
            back.Finish += back_ReverseFinish;
            back.Play();
        }

        void back_ReverseFinish(object sender, EventArgs e)
        {
            back.Hidden = true;
            back.Finish -= back_ReverseFinish;
        }

        void PreviewPlayDialog_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            back.Stop();
            back.PlayType = Effect2D.EffectManager.PlayType.Once;
            back.Finish += back_NormalFinish;
            back.Play();
            back.Hidden = false;
        }

        void back_NormalFinish(object sender, EventArgs e)
        {
            sprite.Hidden = false;
            back.Finish -= back_NormalFinish;
        }

        protected override void UpdateImpl()
        {
            if (Focused)
            {
                black.Alpha += 0.05f;
                if (black.Alpha >= 0.7f)
                {
                    black.Alpha = 0.7f;
                }
            }
            else
            {
                black.Alpha -= 0.05f;
                if (black.Alpha <= 0f)
                {
                    black.Alpha = 0f;
                }
            }
        }

        protected override bool OnCanUpdate()
        {
            return !Hidden;
        }
    }
}
