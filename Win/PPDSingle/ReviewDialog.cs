using PPDFramework;
using PPDShareComponent;
using SharpDX;
using System;

namespace PPDSingle
{
    class ReviewDialog : FocusableGameComponent
    {
        EffectObject back;
        int selection;

        TextureString processing;
        TextureString rate;
        TextureString content;
        TextureString review;

        RectangleComponent black;

        Button[] buttons;
        StarObject[] stars;

        EffectObject select;

        ISound Sound;
        IReviewManager ReviewManager;
        bool reviewWaiting;

        GameComponent[] hiddenList;

        int reviewSelection = -1;
        bool opening;

        public ReviewDialog(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound, IReviewManager reviewManager) : base(device)
        {
            this.ReviewManager = reviewManager;
            reviewManager.ReviewFinished += reviewManager_ReviewFinished;
            this.Sound = sound;
            back = new EffectObject(device, resourceManager, Utility.Path.Combine("postconfirm.etd"))
            {
                Position = new Vector2(400, 225)
            };
            rate = new TextureString(device, String.Format("{0}:", Utility.Language["Evaluate"]), 16, PPDColors.White)
            {
                Position = new Vector2(250, 130)
            };
            content = new TextureString(device, String.Format("{0}:", Utility.Language["ReviewContent"]), 16, PPDColors.White)
            {
                Position = new Vector2(250, 160)
            };
            review = new TextureString(device, "", 16, 300, 100, true, PPDColors.White)
            {
                Position = new Vector2(250, 190)
            };
            processing = new TextureString(device, Utility.Language["Processing"], 16, PPDColors.White)
            {
                Position = new Vector2(400, 300)
            };
            processing.Position = new Vector2(400 - processing.Width / 2, processing.Position.Y);
            black = new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800
            };

            select = new EffectObject(device, resourceManager, Utility.Path.Combine("greenflare.etd"))
            {
                Position = new Vector2(235, 140),
                Scale = new Vector2(0.5f, 0.5f)
            };
            select.PlayType = Effect2D.EffectManager.PlayType.ReverseLoop;
            select.Play();

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
                buttons[i].Hidden = true;
                this.AddChild(buttons[i]);
            }

            stars = new StarObject[5];
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i] = new StarObject(device, resourceManager)
                {
                    Position = new Vector2(420 + i * 30, 130)
                };
                this.AddChild(stars[i]);
            }

            black.Alpha = 0;
            back.Hidden = true;
            processing.Hidden = true;
            hiddenList = new GameComponent[]{
                content,
                rate,
                buttons[0],
                buttons[1],
                stars[0],
                stars[1],
                stars[2],
                stars[3],
                stars[4],
                select,
                review
            };

            foreach (GameComponent hidden in hiddenList)
            {
                hidden.Hidden = true;
            }

            GotFocused += ReviewDialog_GotFocused;
            LostFocused += ReviewDialog_LostFocused;
            Inputed += ReviewDialog_Inputed;

            this.AddChild(select);
            this.AddChild(content);
            this.AddChild(rate);
            this.AddChild(review);
            this.AddChild(processing);
            this.AddChild(back);
            this.AddChild(black);

            Rate = 3;
        }

        void reviewManager_ReviewFinished(bool ok)
        {
            processing.Hidden = true;
            reviewWaiting = false;
            FocusManager.RemoveFocus();
        }

        public bool Reviewed
        {
            get;
            private set;
        }

        public int Rate
        {
            get
            {
                for (int i = stars.Length - 1; i >= 0; i--)
                {
                    if (stars[i].Enabled)
                    {
                        return i + 1;
                    }
                }

                return 0;
            }
            set
            {
                var temp = Math.Max(value, 1);
                for (int i = 0; i < stars.Length; i++)
                {
                    stars[i].Enabled = false;
                }
                for (int i = 0; i < Math.Min(temp, stars.Length); i++)
                {
                    stars[i].Enabled = true;
                }
            }
        }

        public int ReviewSelection
        {
            get
            {
                return reviewSelection;
            }
            set
            {
                reviewSelection = value;
                if (reviewSelection < -1)
                {
                    reviewSelection = PPDFramework.ReviewManager.Instance.Presets.Length - 1;
                }
                if (reviewSelection >= PPDFramework.ReviewManager.Instance.Presets.Length)
                {
                    reviewSelection = -1;
                }
                if (reviewSelection == -1)
                {
                    review.Text = "";
                }
                else
                {
                    review.Text = PPDFramework.ReviewManager.Instance.Presets[reviewSelection];
                }
            }
        }

        void ReviewDialog_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (reviewWaiting) return;
            if (opening)
            {
                if (args.AnyPressed)
                {
                    back.Seek(EffectObject.SeekPosition.End);
                    return;
                }
            }

            if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                if (buttons[0].Selected)
                {
                    Sound.Play(PPDSetting.DefaultSounds[1], -1000);
                    reviewWaiting = true;
                    processing.Hidden = false;
                    buttons[0].Hidden = buttons[1].Hidden = true;
                    ReviewManager.Review(review.Text, Rate);
                }
                else if (buttons[1].Selected)
                {
                    Sound.Play(PPDSetting.DefaultSounds[2], -1000);
                    FocusManager.RemoveFocus();
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                Sound.Play(PPDSetting.DefaultSounds[2], -1000);
                FocusManager.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Left))
            {
                switch (selection)
                {
                    case 0:
                        Rate--;
                        Sound.Play(PPDSetting.DefaultSounds[3], -1000);
                        break;
                    case 1:
                        ReviewSelection--;
                        Sound.Play(PPDSetting.DefaultSounds[3], -1000);
                        break;
                    case 2:
                        bool temp = buttons[0].Selected;
                        buttons[0].Selected = buttons[1].Selected;
                        buttons[1].Selected = temp;
                        Sound.Play(PPDSetting.DefaultSounds[0], -1000);
                        break;
                }

            }
            else if (args.InputInfo.IsPressed(ButtonType.Right))
            {
                switch (selection)
                {
                    case 0:
                        Rate++;
                        Sound.Play(PPDSetting.DefaultSounds[3], -1000);
                        break;
                    case 1:
                        ReviewSelection++;
                        Sound.Play(PPDSetting.DefaultSounds[3], -1000);
                        break;
                    case 2:
                        bool temp = buttons[0].Selected;
                        buttons[0].Selected = buttons[1].Selected;
                        buttons[1].Selected = temp;
                        Sound.Play(PPDSetting.DefaultSounds[0], -1000);
                        break;
                }

            }
            else if (args.InputInfo.IsPressed(ButtonType.Up))
            {
                selection--;
                if (selection < 0)
                {
                    selection = 2;
                }
                select.Position = new Vector2(235, 140 + 30 * selection);
                select.Hidden = selection == 2;
                if (selection == 2)
                {
                    buttons[0].Selected = true;
                    buttons[1].Selected = false;
                }
                else
                {
                    buttons[0].Selected = buttons[1].Selected = false;
                }
                Sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down))
            {
                selection++;
                if (selection > 2)
                {
                    selection = 0;
                }
                select.Position = new Vector2(235, 140 + 30 * selection);
                select.Hidden = selection == 2;
                if (selection == 2)
                {
                    buttons[0].Selected = true;
                    buttons[1].Selected = false;
                }
                else
                {
                    buttons[0].Selected = buttons[1].Selected = false;
                }
                Sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
        }

        void ReviewDialog_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            foreach (GameComponent hidden in hiddenList)
            {
                hidden.Hidden = true;
            }
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

        void ReviewDialog_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            opening = true;
            Reviewed = false;
            back.Stop();
            back.PlayType = Effect2D.EffectManager.PlayType.Once;
            back.Finish += back_NormalFinish;
            back.Play();
            back.Hidden = false;
        }

        void back_NormalFinish(object sender, EventArgs e)
        {
            foreach (GameComponent hidden in hiddenList)
            {
                hidden.Hidden = false;
            }
            selection = 0;
            select.Position = new Vector2(235, 140 + 30 * selection);
            buttons[0].Selected = buttons[1].Selected = false;
            back.Finish -= back_NormalFinish;
            opening = false;
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
