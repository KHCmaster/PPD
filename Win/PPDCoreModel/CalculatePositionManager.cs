using FlowScriptEngine;

namespace PPDCoreModel
{
    public class CalculatePositionManager(Engine engine) : TemplateManager<ICalculatePosition>(engine)
    {
        public bool Calculate(IMarkInfo markInfo, float currentTime, float bpm)
        {
            var handled = false;
            foreach (ICalculatePosition calc in list)
            {
                if (!handled || (handled && calc.IsEvaluateRequired()))
                {
                    calc.Calculate(markInfo, currentTime, bpm);
                    engine.Update();
                }

                if (!handled && calc.EvaluateHandled)
                {
                    markInfo.ColorPosition = calc.CalculatePosition;
                    handled = true;
                }
            }

            return handled;
        }
    }
}
