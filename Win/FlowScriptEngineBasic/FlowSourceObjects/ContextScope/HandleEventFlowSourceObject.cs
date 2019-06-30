using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ContextScope
{
    [ToolTipText("ContextScope_HandleEvent_Summary")]
    public partial class HandleEventFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("ContextScope_HandleEvent_Invoked")]
        public event FlowEventHandler Invoked;
        protected override void OnInitialize()
        {
            ContextScope.EventInvoked += ContextScope_EventInvoked;
        }

        void ContextScope_EventInvoked(FunctionScopeArg obj)
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
            get { return "ContextScope.HandleEvent"; }
        }

        [ToolTipText("ContextScope_HandleEvent_Arg")]
        public object Arg
        {
            get;
            private set;
        }

        [ToolTipText("ContextScope_HandleEvent_EventName")]
        public string EventName
        {
            private get;
            set;
        }

        [ToolTipText("ContextScope_HandleEvent_Handled")]
        public bool Handled
        {
            private get;
            set;
        }

        [ToolTipText("ContextScope_HandleEvent_In")]
        public void In(FlowEventArgs e)
        {
            SetValue(nameof(EventName));
        }
    }
}
