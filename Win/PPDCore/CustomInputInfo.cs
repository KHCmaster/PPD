using PPDFramework;

namespace PPDCore
{
    class CustomInputInfo : InputInfoBase
    {
        private readonly int[] pressCount;
        private readonly bool[] pressed;
        private readonly bool[] released;
        private readonly InputInfoBase baseInputInfo;

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
