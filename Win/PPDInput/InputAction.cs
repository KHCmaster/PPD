using PPDFramework;
using System.Diagnostics;

namespace PPDInput
{
    class InputAction : InputActionBase
    {
        private Stopwatch stopwatch;

        public InputAction(ButtonType buttonType, double time, bool isPressed, Stopwatch stopwatch)
            : base(buttonType, time, isPressed)
        {
            this.stopwatch = stopwatch;
        }

        public override double GetAccurateTime(double currentTime)
        {
            double stopwatchTime = (double)stopwatch.ElapsedTicks / Stopwatch.Frequency;
            return currentTime - (stopwatchTime - Time);
        }
    }
}
