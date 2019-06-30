using PPDFramework;
using PPDShareComponent;
using SharpDX;
using System;

namespace PPDSingle
{
    class TweetDialog : FocusableGameComponent
    {
        EffectObject back;
        int selection;

        TextureString message;
        TextureString content;
        TextureString processing;
        TextureString failedString;

        int failedStringShowCount;

        RectangleComponent black;

        Button[] buttons;

        ISound Sound;
        ITweetManager TweetManager;
        bool tweetWaiting;
        bool opening;

        public TweetDialog(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound, ITweetManager tweetManager) : base(device)
        {
            this.TweetManager = tweetManager;
            tweetManager.TweetFinished += tweetManager_TweetFinished;
            this.Sound = sound;
            back = new EffectObject(device, resourceManager, Utility.Path.Combine("tweetconfirm.etd"))
            {
                Position = new Vector2(400, 225)
            };
            message = new TextureString(device, Utility.Language["TweetBlowContent"], 16, true, PPDColors.White)
            {
                Position = new Vector2(400, 120)
            };
            processing = new TextureString(device, Utility.Language["Processing"], 16, true, PPDColors.White)
            {
                Position = new Vector2(400, 300)
            };
            failedString = new TextureString(device, Utility.Language["TweetFailed"], 16, true, PPDColors.White)
            {
                Position = new Vector2(400, 300)
            };
            content = new TextureString(device, "", 14, 340, 150, true, PPDColors.White)
            {
                Position = new Vector2(230, 150)
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
                buttons[i].Hidden = true;
                this.AddChild(buttons[i]);
            }

            buttons[0].Selected = true;

            black.Alpha = 0;

            back.Hidden = true;
            message.Hidden = content.Hidden = processing.Hidden = failedString.Hidden = buttons[0].Hidden = buttons[1].Hidden = true;

            GotFocused += TweetDialog_GotFocused;
            LostFocused += TweetDialog_LostFocused;
            Inputed += TweetDialog_Inputed;

            this.AddChild(failedString);
            this.AddChild(processing);
            this.AddChild(content);
            this.AddChild(message);
            this.AddChild(back);
            this.AddChild(black);
        }

        public bool Tweeted
        {
            get;
            private set;
        }

        void tweetManager_TweetFinished(bool ok)
        {
            if (ok)
            {
                processing.Hidden = true;
                tweetWaiting = false;
                FocusManager.RemoveFocus();
            }
            else
            {
                failedStringShowCount = 0;
                processing.Hidden = true;
                failedString.Hidden = false;
            }
        }

        void TweetDialog_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (tweetWaiting) return;
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
                switch (selection)
                {
                    case 0:
                        Sound.Play(PPDSetting.DefaultSounds[1], -1000);
                        tweetWaiting = true;
                        processing.Hidden = false;
                        buttons[0].Hidden = buttons[1].Hidden = true;
                        TweetManager.Tweet();
                        break;
                    case 1:
                        Sound.Play(PPDSetting.DefaultSounds[2], -1000);
                        FocusManager.RemoveFocus();
                        break;
                }
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
        }

        void TweetDialog_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            message.Hidden = content.Hidden = buttons[0].Hidden = buttons[1].Hidden = true;
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

        void TweetDialog_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            opening = true;
            Tweeted = false;
            TweetString = TweetManager.TweetText;
            back.Stop();
            back.PlayType = Effect2D.EffectManager.PlayType.Once;
            back.Finish += back_NormalFinish;
            back.Play();
            back.Hidden = false;
        }

        void back_NormalFinish(object sender, EventArgs e)
        {
            message.Hidden = content.Hidden = buttons[0].Hidden = buttons[1].Hidden = false;
            back.Finish -= back_NormalFinish;
            opening = false;
        }

        public string TweetString
        {
            get
            {
                return content.Text;
            }
            set
            {
                content.Text = value;
            }
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

                if (!failedString.Hidden)
                {
                    failedStringShowCount++;
                    if (failedStringShowCount >= 60)
                    {
                        processing.Hidden = failedString.Hidden = true;
                        buttons[0].Hidden = buttons[1].Hidden = false;
                        tweetWaiting = false;
                    }
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
