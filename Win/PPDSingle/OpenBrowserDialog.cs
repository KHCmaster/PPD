using PPDFramework;
using PPDFramework.Web;
using PPDFrameworkCore;
using PPDShareComponent;
using SharpDX;
using System.Diagnostics;

namespace PPDSingle
{
    class OpenBrowserDialog : FocusableGameComponent
    {
        ISound sound;

        TextureString text;
        EffectObject confirm;
        RectangleComponent black;

        SpriteObject sprite;
        Button[] buttons;
        int selection;

        public OpenBrowserDialog(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, ISound sound) : base(device)
        {
            this.sound = sound;

            confirm = new EffectObject(device, resourceManager, Utility.Path.Combine("tweetconfirm.etd"))
            {
                Position = new Vector2(400, 225)
            };
            black = new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800,
                Alpha = 0.5f
            };
            sprite = new SpriteObject(device);
            text = new TextureString(device, Utility.Language["NotHaveScoreDownload"], 16, 300, 200, true, PPDColors.White)
            {
                Position = new Vector2(240, 120)
            };

            buttons = new Button[2];
            for (int i = 0; i < buttons.Length; i++)
            {
                string str = "";
                switch (i)
                {
                    case 0:
                        str = Utility.Language["OK"];
                        break;
                    case 1:
                        str = Utility.Language["Cancel"];
                        break;
                }
                buttons[i] = new Button(device, resourceManager, Utility.Path, str)
                {
                    Position = new Vector2(300 + i * 200, 315)
                };
                buttons[i].Selected = false;
                this.AddChild(buttons[i]);
            }

            buttons[0].Selected = true;

            this.AddChild(sprite);
            sprite.AddChild(text);
            sprite.AddChild(buttons[0]);
            sprite.AddChild(buttons[1]);
            this.AddChild(confirm);
            this.AddChild(black);

            confirm.PlayType = Effect2D.EffectManager.PlayType.Once;
            confirm.Play();
            confirm.Seek(EffectObject.SeekPosition.End);

            Inputed += OpenBrowserDialog_Inputed;
        }

        public string ScoreLibraryId
        {
            get;
            set;
        }

        private void OpenBrowser()
        {
            if (ScoreLibraryId != null)
            {
                ThreadManager.Instance.GetThread(() =>
                {
                    try
                    {
                        Process.Start(WebManager.Instance.GetScorePageUrl(ScoreLibraryId));
                    }
                    catch
                    {
                    }
                }).Start();
            }
        }

        void OpenBrowserDialog_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                switch (selection)
                {
                    case 0:
                        sound.Play(PPDSetting.DefaultSounds[1], -1000);
                        OpenBrowser();
                        break;
                    case 1:
                        sound.Play(PPDSetting.DefaultSounds[2], -1000);
                        break;
                }
                FocusManager.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                sound.Play(PPDSetting.DefaultSounds[2], -1000);
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
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
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
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
        }

        protected override bool OnCanUpdate()
        {
            return !Hidden && Focused;
        }

        protected override bool OnCanDraw(PPDFramework.Shaders.AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return Focused;
        }
    }
}
