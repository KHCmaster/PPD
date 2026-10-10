using PPDFramework;
using System.Diagnostics;

namespace PPDInput
{
    class InputAction(ButtonType buttonType, double time, bool isPressed, Stopwatch stopwatch) : InputActionBase(buttonType, time, isPressed)
    {
        private readonly Stopwatch stopwatch = stopwatch;

        public override double GetAccurateTime(double currentTime)
        {
            double stopwatchTime = (double)stopwatch.ElapsedTicks / Stopwatch.Frequency;
            return currentTime - (stopwatchTime - Time);
        }
    }
}
