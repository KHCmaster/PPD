using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Misc
{
    [ToolTipText("Misc_EntryPoint_Summary")]
    public partial class EntryPointFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Misc_EntryPoint_Start")]
        public event FlowEventHandler Start;
        protected override void OnInitialize()
        {
            Manager.RegisterCallBack(FlowSourceManager.START, FlowScriptEngineStart);
        }

        private void FlowScriptEngineStart(object[] args)
        {
            FireEvent(Start);
        }

        public override string Name
        {
            get { return "Misc.EntryPoint"; }
        }
    }
}
