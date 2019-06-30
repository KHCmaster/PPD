using FlowScriptEngine;
using System.Collections.Generic;

namespace FlowScriptEngineBasic.FlowSourceObjects.Random
{
    [ToolTipText("Random_Trigger_Summary")]
    public partial class TriggerFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Random_Trigger_Out0")]
        public event FlowEventHandler Out0;
        [ToolTipText("Random_Trigger_Out1")]
        public event FlowEventHandler Out1;
        [ToolTipText("Random_Trigger_Out2")]
        public event FlowEventHandler Out2;
        [ToolTipText("Random_Trigger_Out3")]
        public event FlowEventHandler Out3;
        [ToolTipText("Random_Trigger_Out4")]
        public event FlowEventHandler Out4;
        [ToolTipText("Random_Trigger_Out5")]
        public event FlowEventHandler Out5;
        [ToolTipText("Random_Trigger_Out6")]
        public event FlowEventHandler Out6;

        public override string Name
        {
            get { return "Random.Trigger"; }
        }

        private List<FlowEventHandler> triggers;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            triggers = new List<FlowEventHandler>();
            if (IsConnected("Out0", false))
            {
                triggers.Add(Out0);
            }
            if (IsConnected("Out1", false))
            {
                triggers.Add(Out1);
            }
            if (IsConnected("Out2", false))
            {
                triggers.Add(Out2);
            }
            if (IsConnected("Out3", false))
            {
                triggers.Add(Out3);
            }
            if (IsConnected("Out4", false))
            {
                triggers.Add(Out4);
            }
            if (IsConnected("Out5", false))
            {
                triggers.Add(Out5);
            }
            if (IsConnected("Out6", false))
            {
                triggers.Add(Out6);
            }
        }

        [ToolTipText("Random_Trigger_In")]
        public void In(FlowEventArgs e)
        {
            if (triggers.Count == 0)
            {
                return;
            }

            var triggerIndex = RandomUtility.Rand.Next(triggers.Count);
            FireEvent(triggers[triggerIndex]);
        }
    }
}
