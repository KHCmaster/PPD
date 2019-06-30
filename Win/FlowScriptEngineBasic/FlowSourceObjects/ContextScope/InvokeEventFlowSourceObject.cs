using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ContextScope
{
    [ToolTipText("ContextScope_InvokeEvent_Summary")]
    public partial class InvokeEventFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ContextScope.InvokeEvent"; }
        }

        [ToolTipText("ContextScope_InvokeEvent_EventName")]
        public string EventName
        {
            private get;
            set;
        }

        [ToolTipText("ContextScope_InvokeEvent_Arg")]
        public object Arg
        {
            private get;
            set;
        }

        [ToolTipText("ContextScope_InvokeEvent_In")]
        public void In(FlowEventArgs e)
        {
            SetValue(nameof(EventName));
            SetValue(nameof(Arg));
            ContextScope.InvokeEvent(EventName, Arg);
        }
    }
}
