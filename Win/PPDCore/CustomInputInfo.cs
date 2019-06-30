using PPDFramework;

namespace PPDCore
{
    class CustomInputInfo : InputInfoBase
    {
        private int[] pressCount;
        private bool[] pressed;
        private bool[] released;
        private InputInfoBase baseInputInfo;

        public CustomInputInfo(int[] pressCount, bool[] pressed, bool[] released, InputInfoBase inputInfo)
            : base(false)
        {
            this.pressCount = pressCount;
            this.pressed = pressed;
            this.released = released;
            this.baseInputInfo = inputInfo;
        }

        public override bool IsPressed(ButtonType buttonType)
        {
            if (buttonType > ButtonType.L)
            {
                return baseInputInfo.IsPressed(buttonType);
            }
            return pressed[(int)buttonType];
        }

        public override bool IsReleased(ButtonType buttonType)
        {
            if (buttonType > ButtonType.L)
            {
                return baseInputInfo.IsReleased(buttonType);
            }
            return released[(int)buttonType];
        }

        public override int GetPressingFrame(ButtonType buttonType)
        {
            if (buttonType > ButtonType.L)
            {
                return baseInputInfo.GetPressingFrame(buttonType);
            }
            return pressCount[(int)buttonType];
        }
    }
}
