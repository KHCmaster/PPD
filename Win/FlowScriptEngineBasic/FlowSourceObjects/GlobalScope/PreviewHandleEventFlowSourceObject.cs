using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.GlobalScope
{
    [ToolTipText("GlobalScope_PreviewHandleEvent_Summary")]
    public partial class PreviewHandleEventFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("GlobalScope_PreviewHandleEvent_Invoked")]
        public event FlowEventHandler Invoked;
        protected override void OnInitialize()
        {
            GlobalScope.PreviewEventInvoked += GlobalScope_PreviewEventInvoked;
        }

        void GlobalScope_PreviewEventInvoked(FunctionScopeArg obj)
        {
            if (obj.EventName == EventName)
            {
                Arg = obj.Arg;
                FireEvent(Invoked);
                FireEvent(e =>
                {
                    SetValue(nameof(Handled));
                    obj.Handled = Handled;
                    obj.OnEndInvoked();
                }, true);
            }
            else
            {
                obj.OnEndInvoked();
            }
        }

        public override string Name
        {
            get { return "GlobalScope.PreviewHandleEvent"; }
        }

        [ToolTipText("GlobalScope_PreviewHandleEvent_Arg")]
        public object Arg
        {
            get;
            private set;
        }

        [ToolTipText("GlobalScope_PreviewHandleEvent_EventName")]
        public string EventName
        {
            private get;
            set;
        }

        [ToolTipText("GlobalScope_PreviewHandleEvent_Handled")]
        public bool Handled
        {
            private get;
            set;
        }

        [ToolTipText("GlobalScope_PreviewHandleEvent_In")]
        public void In(FlowEventArgs e)
        {
            SetValue(nameof(EventName));
        }
    }
}
