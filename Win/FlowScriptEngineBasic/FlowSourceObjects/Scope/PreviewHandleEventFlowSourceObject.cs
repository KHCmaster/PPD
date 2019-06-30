using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Scope
{
    [ToolTipText("Scope_PreviewHandleEvent_Summary")]
    public partial class PreviewHandleEventFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Scope_PreviewHandleEvent_Invoked")]
        public event FlowEventHandler Invoked;
        protected override void OnInitialize()
        {
            Scope.PreviewEventInvoked += Scope_PreviewEventInvoked;
        }

        void Scope_PreviewEventInvoked(FunctionScopeArg obj)
        {
            if (obj.EventName == EventName)
            {
                Arg = obj.Arg;
                FireEvent(Invoked, true);
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
            get { return "Scope.PreviewHandleEvent"; }
        }

        [ToolTipText("Scope_PreviewHandleEvent_Arg")]
        public object Arg
        {
            get;
            private set;
        }

        [ToolTipText("Scope_PreviewHandleEvent_EventName")]
        public string EventName
        {
            private get;
            set;
        }

        [ToolTipText("Scope_PreviewHandleEvent_Handled")]
        public bool Handled
        {
            private get;
            set;
        }

        [ToolTipText("Scope_PreviewHandleEvent_In")]
        public void In(FlowEventArgs e)
        {
            SetValue(nameof(EventName));
        }
    }
}
