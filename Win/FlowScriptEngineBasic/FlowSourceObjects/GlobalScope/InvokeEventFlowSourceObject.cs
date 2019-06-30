using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.GlobalScope
{
    [ToolTipText("GlobalScope_InvokeEvent_Summary")]
    public partial class InvokeEventFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "GlobalScope.InvokeEvent"; }
        }

        [ToolTipText("GlobalScope_InvokeEvent_EventName")]
        public string EventName
        {
            private get;
            set;
        }

        [ToolTipText("GlobalScope_InvokeEvent_Arg")]
        public object Arg
        {
            private get;
            set;
        }

        [ToolTipText("GlobalScope_InvokeEvent_In")]
        public void In(FlowEventArgs e)
        {
            SetValue(nameof(EventName));
            SetValue(nameof(Arg));
            GlobalScope.InvokeEvent(EventName, Arg);
        }
    }
}
