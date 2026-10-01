using PPDFramework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PPDInput
{
    class AccurateInputInfo(InputAction[] actions, Stopwatch stopwatch, Dictionary<ButtonType, InputAction> lastPressInfos) : InputInfoBase(true)
    {
        private readonly InputAction[] actions = actions;
        private readonly Stopwatch stopwatch = stopwatch;
        private readonly Dictionary<ButtonType, InputAction> lastPressInfos = lastPressInfos;

        public double CurrentTime
        {
            get;
            private set;
        } = (double)stopwatch.ElapsedTicks / Stopwatch.Frequency;

        public override InputActionBase[] Actions
        {
            get
            {
                return actions;
            }
        }

        public override bool IsPressed(ButtonType buttonType)
        {
            return actions.Any(i => i.ButtonType == buttonType && i.IsPressed);
        }

        public override bool IsReleased(ButtonType buttonType)
        {
            return actions.Any(i => i.ButtonType == buttonType && !i.IsPressed);
        }

        public override int GetPressingFrame(ButtonType buttonType)
        {
            if (lastPressInfos.ContainsKey(buttonType))
            {
                return (int)((CurrentTime - lastPressInfos[buttonType].Time) * 60.0);
            }

            return 0;
        }
    }
}
