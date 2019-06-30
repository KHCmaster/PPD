using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.GlobalScope
{
    [ToolTipText("GlobalScope_HandleEvent_Summary")]
    public partial class HandleEventFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("GlobalScope_HandleEvent_Invoked")]
        public event FlowEventHandler Invoked;
        protected override void OnInitialize()
        {
            GlobalScope.EventInvoked += GlobalScope_EventInvoked;
        }

        void GlobalScope_EventInvoked(FunctionScopeArg obj)
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
            get { return "GlobalScope.HandleEvent"; }
        }

        [ToolTipText("GlobalScope_HandleEvent_Arg")]
        public object Arg
        {
            get;
            private set;
        }

        [ToolTipText("GlobalScope_HandleEvent_EventName")]
        public string EventName
        {
            private get;
            set;
        }

        [ToolTipText("GlobalScope_HandleEvent_Handled")]
        public bool Handled
        {
            private get;
            set;
        }

        [ToolTipText("GlobalScope_HandleEvent_In")]
        public void In(FlowEventArgs e)
        {
            SetValue(nameof(EventName));
        }
    }
}
