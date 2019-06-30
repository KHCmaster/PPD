using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Logic
{
    [ToolTipText("Logic_OnceGate_Summary")]
    public partial class OnceGateFlowSourceObject : ExecuteOnlyFlowSourceObject
    {
        [ToolTipText("Logic_OnceGate_Out")]
        public event FlowEventHandler Out;

        bool closed;


        public override string Name
        {
            get { return "Logic.OnceGate"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            closed = false;
        }

        protected override void OnReset()
        {
            base.OnReset();
            closed = false;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            if (closed)
            {
                return;
            }

            closed = true;
            FireEvent(Out);
        }
    }
}
