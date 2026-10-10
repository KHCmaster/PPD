using PPDFramework;

namespace PPDCore
{
    class CustomInputInfo(int[] pressCount, bool[] pressed, bool[] released, InputInfoBase inputInfo) : InputInfoBase(false)
    {
        private readonly int[] pressCount = pressCount;
        private readonly bool[] pressed = pressed;
        private readonly bool[] released = released;
        private readonly InputInfoBase baseInputInfo = inputInfo;

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
