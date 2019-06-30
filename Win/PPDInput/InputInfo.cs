using PPDFramework;
using System;

namespace PPDInput
{
    class InputInfo : InputInfoBase
    {
        private int[] pressCount;
        private bool[] released;

        public InputInfo(bool accurate)
            : base(accurate)
        {
        }

        public void Update(int[] keyPressCount, bool[] keyReleased, int[] buttonPressCount, bool[] buttonReleased)
        {
            if (pressCount == null)
            {
                pressCount = new int[keyPressCount.Length];
            }
            if (released == null)
            {
                released = new bool[keyReleased.Length];
            }

            for (int i = 0; i < keyPressCount.Length; i++)
            {
                pressCount[i] = Math.Max(keyPressCount[i], buttonPressCount[i]);
            released[i] = keyReleased[i] || buttonReleased[i];
            }
        }

        public override bool IsPressed(ButtonType buttonType)
        {
            return pressCount[(int)buttonType] == 1;
        }

        public override bool IsReleased(ButtonType buttonType)
        {
            return released[(int)buttonType];
        }

        public override int GetPressingFrame(ButtonType buttonType)
        {
            return pressCount[(int)buttonType];
        }
    }
}
