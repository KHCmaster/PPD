using PPDFramework;

namespace PPDCore
{
    class SingleInputInfo(ButtonType buttonType, bool isPressed, int pressingFrame) : InputInfoBase(false)
    {
        private readonly ButtonType buttonType = buttonType;
        private readonly bool isPressed = isPressed;
        private readonly int pressingFrame = pressingFrame;

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
