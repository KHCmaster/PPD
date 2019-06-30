using FlowScriptEngine;

namespace PPDCoreModel
{
    public class ProcessMarkBPMManager : TemplateManager<IProcessMarkBPM>
    {
        public ProcessMarkBPMManager(Engine engine)
            : base(engine)
        {

        }

        public bool Process(IMarkInfo markInfo, out float bpm)
        {
            bpm = 0;
            var handled = false;
            foreach (IProcessMarkBPM proc in list)
            {
                if (!handled || (handled && proc.IsEvaluateRequired()))
                {
                    proc.Process(markInfo);
                    engine.Update();
                }

                if (!handled && proc.EvaluateHandled)
                {
                    bpm = proc.ProcessBPM;
                    handled = true;
                }
            }

            return handled;
        }
    }
}
