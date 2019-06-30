using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ContextScope
{
    [ToolTipText("ContextScope_PreviewHandleEvent_Summary")]
    public partial class PreviewHandleEventFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("ContextScope_PreviewHandleEvent_Invoked")]
        public event FlowEventHandler Invoked;
        protected override void OnInitialize()
        {
            ContextScope.PreviewEventInvoked += ContextScope_PreviewEventInvoked;
        }

        void ContextScope_PreviewEventInvoked(FunctionScopeArg obj)
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
            get { return "ContextScope.PreviewHandleEvent"; }
        }

        [ToolTipText("ContextScope_PreviewHandleEvent_Arg")]
        public object Arg
        {
            get;
            private set;
        }

        [ToolTipText("ContextScope_PreviewHandleEvent_EventName")]
        public string EventName
        {
            private get;
            set;
        }

        [ToolTipText("ContextScope_PreviewHandleEvent_Handled")]
        public bool Handled
        {
            private get;
            set;
        }

        [ToolTipText("ContextScope_PreviewHandleEvent_In")]
        public void In(FlowEventArgs e)
        {
            SetValue(nameof(EventName));
        }
    }
}
