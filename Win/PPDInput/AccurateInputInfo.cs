using PPDFramework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PPDInput
{
    class AccurateInputInfo : InputInfoBase
    {
        private InputAction[] actions;
        private Stopwatch stopwatch;
        private Dictionary<ButtonType, InputAction> lastPressInfos;

        public double CurrentTime
        {
            get;
            private set;
        }

        public override InputActionBase[] Actions
        {
            get
            {
                return actions;
            }
        }

        public AccurateInputInfo(InputAction[] actions, Stopwatch stopwatch, Dictionary<ButtonType, InputAction> lastPressInfos)
            : base(true)
        {
            this.actions = actions;
            this.stopwatch = stopwatch;
            this.lastPressInfos = lastPressInfos;

            CurrentTime = (double)stopwatch.ElapsedTicks / Stopwatch.Frequency;
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
