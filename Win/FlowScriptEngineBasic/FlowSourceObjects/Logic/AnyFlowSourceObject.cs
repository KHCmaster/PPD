using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Logic
{
    [ToolTipText("Logic_Any_Summary", "Logic_Any_Remark")]
    public partial class AnyFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Logic_Any_Out")]
        public event FlowEventHandler Out;
        public override string Name
        {
            get { return "Logic.Any"; }
        }

        [ToolTipText("Logic_Any_In0")]
        public void In0(FlowEventArgs e)
        {
            FireEvent(Out);
        }

        [ToolTipText("Logic_Any_In1")]
        public void In1(FlowEventArgs e)
        {
            FireEvent(Out);
        }

        [ToolTipText("Logic_Any_In2")]
        public void In2(FlowEventArgs e)
        {
            FireEvent(Out);
        }

        [ToolTipText("Logic_Any_In3")]
        public void In3(FlowEventArgs e)
        {
            FireEvent(Out);
        }

        [ToolTipText("Logic_Any_In4")]
        public void In4(FlowEventArgs e)
        {
            FireEvent(Out);
        }

        [ToolTipText("Logic_Any_In5")]
        public void In5(FlowEventArgs e)
        {
            FireEvent(Out);
        }
    }
}
