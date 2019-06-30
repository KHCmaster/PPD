using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Scope
{
    [ToolTipText("Scope_HandleEvent_Summary")]
    public partial class HandleEventFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Scope_HandleEvent_Invoked")]
        public event FlowEventHandler Invoked;
        protected override void OnInitialize()
        {
            Scope.EventInvoked += Scope_EventInvoked;
        }

        void Scope_EventInvoked(FunctionScopeArg obj)
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
            get { return "Scope.HandleEvent"; }
        }

        [ToolTipText("Scope_HandleEvent_Arg")]
        public object Arg
        {
            get;
            private set;
        }

        [ToolTipText("Scope_HandleEvent_EventName")]
        public string EventName
        {
            private get;
            set;
        }

        [ToolTipText("Scope_HandleEvent_Handled")]
        public bool Handled
        {
            private get;
            set;
        }

        [ToolTipText("Scope_HandleEvent_In")]
        public void In(FlowEventArgs e)
        {
            SetValue(nameof(EventName));
        }
    }
}
