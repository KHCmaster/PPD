using FlowScriptEngine;
using PPDFramework;

namespace PPDCoreModel
{
    public class ProcessAllowedButtonsManager : TemplateManager<IProcessAllowedButtons>
    {
        public ProcessAllowedButtonsManager(Engine engine)
            : base(engine)
        {

        }

        public bool Process(IMarkInfo markInfo, out ButtonType[] allowedButtons)
        {
            allowedButtons = null;
            var handled = false;
            foreach (IProcessAllowedButtons proc in list)
            {
                if (!handled || (handled && proc.IsEvaluateRequired()))
                {
                    proc.Process(markInfo);
                    engine.Update();
                }

                if (!handled && proc.EvaluateHandled)
                {
                    allowedButtons = proc.ProcessAllowedButtons;
                    handled = true;
                }
            }
            return handled;
        }
    }
}
