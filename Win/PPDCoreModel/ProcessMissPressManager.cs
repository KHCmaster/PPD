using FlowScriptEngine;
using PPDCoreModel.Data;

namespace PPDCoreModel
{
    public class ProcessMissPressManager : TemplateManager<IProcessMissPress>
    {
        public ProcessMissPressManager(Engine engine)
            : base(engine)
        {
        }

        public bool Process(IMarkInfo markInfo, MarkType pressedButton, out bool isMissPress)
        {
            isMissPress = false;
            var handled = false;
            foreach (IProcessMissPress proc in list)
            {
                if (!handled || (handled && proc.IsEvaluateRequired()))
                {
                    proc.Process(markInfo, pressedButton);
                    engine.Update();
                }

                if (!handled && proc.EvaluateHandled)
                {
                    isMissPress = proc.ProcessMissPress;
                    handled = true;
                }
            }

            return handled;
        }
    }
}
