using FlowScriptEngine;
using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects
{
    [ToolTipText("Pause_Summary")]
    public partial class PauseFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Pause_Paused_Summary")]
        public event FlowEventHandler Paused;

        [ToolTipText("Pause_Resumed_Summary")]
        public event FlowEventHandler Resumed;

        public override string Name
        {
            get { return "PPD.Pause"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            Manager.RegisterCallBack(PauseManager.PausedCallbackName, PausedCallback);
            Manager.RegisterCallBack(PauseManager.ResumedCallbackName, ResumedCallback);
        }

        private void PausedCallback(object[] args)
        {
            FireEvent(Paused);
        }

        private void ResumedCallback(object[] args)
        {
            FireEvent(Resumed);
        }
    }
}
