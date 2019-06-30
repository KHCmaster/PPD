using PPDFramework;
using SharpDX;
using System;

namespace PPDShareComponent
{
    public class BoolEventArgs : EventArgs
    {

        public bool Value
        {
            get;
            set;
        }
    }

    public class ConfirmComponent : FocusableGameComponent
    {
        public event EventHandler<BoolEventArgs> ButtonPressed;

        public enum ConfirmButtonType
        {
            YesNo,
            OK
        }

        TextureString confirm;
        Button[] buttons;

        private int selection;

        PPDFramework.Resource.ResourceManager resourceManager;

        public bool OK
        {
            get;
            private set;
        }

        public ConfirmComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, PathManager pathManager, string message, string yes, string no, string ok, ConfirmButtonType buttonType) : base(device)
        {
            this.resourceManager = resourceManager;

            confirm = new TextureString(device, message, 15, 200, 400, true, true, PPDColors.White)
            {
                Position = new Vector2(300, 150)
            };
            this.AddChild(confirm);

            if (buttonType == ConfirmButtonType.YesNo)
            {
                buttons = new Button[2];
                for (int i = 0; i < buttons.Length; i++)
                {
                    buttons[i] = new Button(device, resourceManager, pathManager, i == 0 ? yes : no)
                    {
                        Position = new Vector2(i == 0 ? 350 : 450, 280)
                    };

                    buttons[i].Selected = false;
                    this.AddChild(buttons[i]);
                }
            }
            else
            {
                buttons = new Button[1];
                buttons[0] = new Button(device, resourceManager, pathManager, ok)
                {
                    Position = new Vector2(400, 280)
                };
            }
            buttons[0].Selected = true;

            this.AddChild(new PictureObject(device, resourceManager, pathManager.Combine("conftop.png"))
            {
                Position = new Vector2(266, 225 - 107)
            });
            this.AddChild(new PictureObject(device, resourceManager, pathManager.Combine("confbottom.png"))
            {
                Position = new Vector2(266, 225 + 107 - 17)
            });
            this.AddChild(new PictureObject(device, resourceManager, pathManager.Combine("confirmpause.png"))
            {
                Position = new Vector2(266, 118)
            });
            this.AddChild(new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                RectangleWidth = 800,
                RectangleHeight = 450,
                Alpha = 0.75f
            });

            Alpha = 1;

            Inputed += ConfirmComponent_Inputed;
        }

        void ConfirmComponent_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Circle))
            {
                switch (selection)
                {
                    case 0:
                        Close(true);
                        break;
                    case 1:
                        Close(false);
                        break;
                }
            }
            else if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                Close(false);
            }
            else if (args.InputInfo.IsPressed(ButtonType.Left))
            {
                buttons[selection].Selected = false;
                selection--;
                if (selection < 0)
                {
                    selection = buttons.Length - 1;
                }
                buttons[selection].Selected = true;
            }
            else if (args.InputInfo.IsPressed(ButtonType.Right))
            {
                buttons[selection].Selected = false;
                selection++;
                if (selection >= buttons.Length)
                {
                    selection = 0;
                }
                buttons[selection].Selected = true;
            }
        }

        private void Close(bool ok)
        {
            OK = ok;

            var e = new BoolEventArgs
            {
                Value = true
            };

            if (ButtonPressed != null)
            {
                ButtonPressed.Invoke(this, e);
            }

            if (e.Value)
            {
                FocusManager.RemoveFocus();
            }
        }
    }
}
