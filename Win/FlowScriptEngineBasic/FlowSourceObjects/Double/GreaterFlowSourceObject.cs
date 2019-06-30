using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Double
{
    [ToolTipText("Greater_Summary")]
    public partial class GreaterFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Double.Greater"; }
        }

        [ToolTipText("FirstArgument")]
        public double A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public double B
        {
            private get;
            set;
        }

        [ToolTipText("Greater_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A > B;
            }
        }
    }
}
