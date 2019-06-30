using PPDFramework;

namespace PPDCore
{
    class SingleInputInfo : InputInfoBase
    {
        private ButtonType buttonType;
        private bool isPressed;
        private int pressingFrame;

        public SingleInputInfo(ButtonType buttonType, bool isPressed, int pressingFrame)
            : base(false)
        {
            this.buttonType = buttonType;
            this.isPressed = isPressed;
            this.pressingFrame = pressingFrame;
        }

        public override bool IsPressed(ButtonType buttonType)
        {
            return this.buttonType == buttonType && isPressed;
        }

        public override bool IsReleased(PPDFramework.ButtonType buttonType)
        {
            return this.buttonType == buttonType && !isPressed;
        }

        public override int GetPressingFrame(ButtonType buttonType)
        {
            return pressingFrame;
        }
    }
}
