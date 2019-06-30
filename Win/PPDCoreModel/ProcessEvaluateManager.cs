using FlowScriptEngine;
using PPDCoreModel.Data;
using SharpDX;

namespace PPDCoreModel
{
    public class ProcessEvaluateManager : TemplateManager<IProcessEvaluate>
    {
        public ProcessEvaluateManager(Engine engine)
            : base(engine)
        {

        }

        public bool ProcessEvaluate(IMarkInfo markInfo, EffectType effectType, bool missPress, bool release, Vector2 position)
        {
            var handled = false;
            foreach (IProcessEvaluate proc in list)
            {
                if (!handled || (handled && proc.IsEvaluateRequired()))
                {
                    proc.ProcessEvaluate(markInfo, effectType, missPress, release, position);
                    engine.Update();
                }

                if (!handled && proc.EvaluateHandled)
                {
                    handled = true;
                }
            }

            return handled;
        }
    }
}
