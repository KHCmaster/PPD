using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Logic
{
    [ToolTipText("Logic_Switch_Summary")]
    public partial class SwitchFlowSourceObject : ExecuteOnlyFlowSourceObject
    {
        [ToolTipText("Logic_Switch_Default")]
        public event FlowEventHandler Default;
        [ToolTipText("Logic_Switch_Out1")]
        public event FlowEventHandler Out1;
        [ToolTipText("Logic_Switch_Out2")]
        public event FlowEventHandler Out2;
        [ToolTipText("Logic_Switch_Out3")]
        public event FlowEventHandler Out3;
        [ToolTipText("Logic_Switch_Out4")]
        public event FlowEventHandler Out4;
        [ToolTipText("Logic_Switch_Out5")]
        public event FlowEventHandler Out5;
        [ToolTipText("Logic_Switch_Out6")]
        public event FlowEventHandler Out6;

        public override string Name
        {
            get { return "Logic.Switch"; }
        }

        [ToolTipText("Logic_Switch_Value")]
        public object Value
        {
            private get;
            set;
        }

        [ToolTipText("Logic_Switch_Target1")]
        public object Target1
        {
            private get;
            set;
        }

        [ToolTipText("Logic_Switch_Target2")]
        public object Target2
        {
            private get;
            set;
        }

        [ToolTipText("Logic_Switch_Target3")]
        public object Target3
        {
            private get;
            set;
        }

        [ToolTipText("Logic_Switch_Target4")]
        public object Target4
        {
            private get;
            set;
        }

        [ToolTipText("Logic_Switch_Target5")]
        public object Target5
        {
            private get;
            set;
        }

        [ToolTipText("Logic_Switch_Target6")]
        public object Target6
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            bool anyFired = false;
            SetValue(nameof(Value));
            if (IsConnected("Target1", true) && !anyFired)
            {
                SetValue(nameof(Target1));
                if (CheckEquals(Value, Target1))
                {
                    FireEvent(Out1);
                    anyFired = true;
                }
            }
            if (IsConnected("Target2", true) && !anyFired)
            {
                SetValue(nameof(Target2));
                if (CheckEquals(Value, Target2))
                {
                    FireEvent(Out2);
                    anyFired = true;
                }
            }
            if (IsConnected("Target3", true) && !anyFired)
            {
                SetValue(nameof(Target3));
                if (CheckEquals(Value, Target3))
                {
                    FireEvent(Out3);
                    anyFired = true;
                }
            }
            if (IsConnected("Target4", true) && !anyFired)
            {
                SetValue(nameof(Target4));
                if (CheckEquals(Value, Target4))
                {
                    FireEvent(Out4);
                    anyFired = true;
                }
            }
            if (IsConnected("Target5", true) && !anyFired)
            {
                SetValue(nameof(Target5));
                if (CheckEquals(Value, Target5))
                {
                    FireEvent(Out5);
                    anyFired = true;
                }
            }
            if (IsConnected("Target6", true) && !anyFired)
            {
                SetValue(nameof(Target6));
                if (CheckEquals(Value, Target6))
                {
                    FireEvent(Out6);
                    anyFired = true;
                }
            }
            if (!anyFired)
            {
                FireEvent(Default);
            }
        }

        private bool CheckEquals(object object1, object object2)
        {
            if (object1 == null && object2 == null)
            {
                return true;
            }
            else if (object1 != null)
            {
                return object1.Equals(object2);
            }
            else if (object2 != null)
            {
                return object2.Equals(object1);
            }
            return object1.Equals(object2);
        }
    }
}
