using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Logic
{
    [ToolTipText("Logic_Sequential_Summary")]
    public partial class SequentialFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Logic_Sequential_Out0")]
        public event FlowEventHandler Out0;
        [ToolTipText("Logic_Sequential_Out1")]
        public event FlowEventHandler Out1;
        [ToolTipText("Logic_Sequential_Out2")]
        public event FlowEventHandler Out2;
        [ToolTipText("Logic_Sequential_Out3")]
        public event FlowEventHandler Out3;
        [ToolTipText("Logic_Sequential_Out4")]
        public event FlowEventHandler Out4;
        [ToolTipText("Logic_Sequential_Out5")]
        public event FlowEventHandler Out5;
        [ToolTipText("Logic_Sequential_Out6")]
        public event FlowEventHandler Out6;

        public override string Name
        {
            get { return "Logic.Sequential"; }
        }

        [ToolTipText("Logic_Sequential_In")]
        public void In(FlowEventArgs e)
        {
            FireEvent(Out0, true);
            FireEvent(Out1, true);
            FireEvent(Out2, true);
            FireEvent(Out3, true);
            FireEvent(Out4, true);
            FireEvent(Out5, true);
            FireEvent(Out6, true);
        }
    }
}
