using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Scope
{
    [ToolTipText("Scope_InvokeEvent_Summary")]
    public partial class InvokeEventFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Scope.InvokeEvent"; }
        }

        [ToolTipText("Scope_InvokeEvent_EventName")]
        public string EventName
        {
            private get;
            set;
        }

        [ToolTipText("Scope_InvokeEvent_Arg")]
        public object Arg
        {
            private get;
            set;
        }

        [ToolTipText("Scope_InvokeEvent_In")]
        public void In(FlowEventArgs e)
        {
            SetValue(nameof(EventName));
            SetValue(nameof(Arg));
            Scope.InvokeEvent(EventName, Arg);
        }
    }
}
