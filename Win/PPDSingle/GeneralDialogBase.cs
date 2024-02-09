using PPDFramework;
using PPDShareComponent;
using SharpDX;
using System;

namespace PPDSingle
{
    abstract class GeneralDialogBase : FocusableGameComponent
    {
        GridSelection selection;

        EffectObject back;
        TextureString message;
        SpriteObject sprite;
        RectangleComponent black;
        Button[] buttons;
        ISound sound;

        int rowCount;

        protected virtual int CancelIndex
        {
            get { return -1; }
        }

        public int Result
        {
            get;
            private set;
        }

        protected GeneralDialogBase(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager,
            ISound sound, string displayText, string[] buttonTexts) : base(device)
        {
            this.sound = sound;
            selection = new GridSelection();
            sprite = new SpriteObject(device);
            back = new EffectObject(device, resourceManager, Utility.Path.Combine("postconfirm.etd"))
            {
                Position = new Vector2(400, 225)
            };
            message = new TextureString(device, displayText, 16, 340, 300, true, PPDColors.White)
            {
                Position = new Vector2(230, 120)
            };

            black = new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleHeight = 450,
                RectangleWidth = 800
            };

            buttons = new Button[buttonTexts.Length];
            if (buttons.Length == 1)
            {
                buttons[0] = new Button(device, resourceManager, Utility.Path, buttonTexts[0])
                {
                    Position = new Vector2(400, 315)
                };
                buttons[0].Selected = false;
                sprite.AddChild(buttons[0]);
                selection.Add(buttons[0].Position);
            }
            else
            {
                rowCount = (int)Math.Ceiling(buttonTexts.Length / 2f);
                for (int i = 0; i < buttons.Length; i++)
                {
                    int rowIndex = i / 2;
                    int columnIndex = i % 2;
                    buttons[i] = new Button(device, resourceManager, Utility.Path, buttonTexts[i])
                    {
                        Position = new Vector2(300 + columnIndex * 200, 315 - (rowCount - rowIndex - 1) * 50)
                    };
                    buttons[i].Selected = false;
                    sprite.AddChild(buttons[i]);
                    selection.Add(buttons[i].Position);
                }
            }

            buttons[0].Selected = true;

            black.Alpha = 0;

            back.Hidden = true;
            sprite.Hidden = true;

            GotFocused += PreviewPlayDialog_GotFocused;
            LostFocused += PreviewPlayDialog_LostFocused;
            Inputed += PreviewPlayDialog_Inputed;

            sprite.AddChild(message);

            this.AddChild(sprite);
            this.AddChild(back);
            this.AddChild(black);
        }

        void PreviewPlayDialog_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                if (selection.Current == CancelIndex)
                {
                    sound.Play(PPDSetting.DefaultSounds[2], -1000);
                }
                else
                {
                    sound.Play(PPDSetting.DefaultSounds[1], -1000);
                }
                Result = selection.Current;
                FocusManager.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                Result = -1;
                sound.Play(PPDSetting.DefaultSounds[2], -1000);
                FocusManager.RemoveFocus();
            }
            else if (args.InputInfo.IsPressed(ButtonType.Left))
            {
                buttons[selection.Current].Selected = false;
                selection.Left();
                buttons[selection.Current].Selected = true;
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Right))
            {
                buttons[selection.Current].Selected = false;
                selection.Right();
                buttons[selection.Current].Selected = true;
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Down) && rowCount >= 2)
            {
                buttons[selection.Current].Selected = false;
                selection.Down();
                buttons[selection.Current].Selected = true;
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Up) && rowCount >= 2)
            {
                buttons[selection.Current].Selected = false;
                selection.Up();
                buttons[selection.Current].Selected = true;
                sound.Play(PPDSetting.DefaultSounds[0], -1000);
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
