using PPDFramework;
using PPDMultiCommon.Web;
using PPDShareComponent;
using SharpDX;
using System;

namespace PPDMulti
{
    class PasswordDialog : FocusableGameComponent
    {
        public event EventHandler Processed;

        bool shouldFocusTextBox;
        DxTextBox textBox;
        RoomInfo roomInfo;

        public bool IsValid
        {
            get;
            private set;
        }

        public PasswordDialog(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, IGameHost gameHost, RoomInfo roomInfo) : base(device)
        {
            this.roomInfo = roomInfo;

            textBox = new DxTextBox(device, gameHost, resourceManager)
            {
                DrawOnlyFocus = false
            };
            textBox.LostFocused += textBox_LostFocused;
            textBox.Position = new SharpDX.Vector2(300, 250);
            textBox.TextBoxHeight = 20;
            textBox.TextBoxWidth = textBox.MaxWidth = 200;
            textBox.Text = "";
            this.AddChild(textBox);
            this.AddChild(new TextureString(device, Utility.Language["ValidatePassword"], 20, true, PPDColors.White)
            {
                Position = new Vector2(400, 200)
            });
            this.AddChild(new RectangleComponent(device, resourceManager, PPDColors.Black)
            {
                Alpha = 0.75f,
                RectangleWidth = 800,
                RectangleHeight = 450
            });

            GotFocused += PasswordDialog_GotFocused;
            Inputed += PasswordDialog_Inputed;
        }

        void PasswordDialog_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (args.InputInfo.IsPressed(ButtonType.Cross))
            {
                FocusManager.RemoveFocus();
                this.Parent.RemoveChild(this);
                args.Handled = true;
            }
        }

        void textBox_LostFocused(IFocusable sender, FocusEventArgs args)
        {
            string text = textBox.Text;
            IsValid = roomInfo.PasswordHash == WebManager.GetPasswordHash(text);

            textBox.Parent.RemoveChild(textBox);
            if (Processed != null)
            {
                Processed.Invoke(this, EventArgs.Empty);
            }

            if (!IsValid)
            {
                this.InsertChild(new TextureString(device, Utility.Language["NotCorrectPassword"], 20, true, PPDColors.White)
                {
                    Position = new Vector2(400, 250)
                }, 0);
            }
        }

        void PasswordDialog_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            shouldFocusTextBox = true;
            this.GotFocused -= PasswordDialog_GotFocused;
        }

        protected override void UpdateImpl()
        {
            if (shouldFocusTextBox)
            {
                FocusManager.Focus(textBox);
                shouldFocusTextBox = false;
            }
        }
    }
}
