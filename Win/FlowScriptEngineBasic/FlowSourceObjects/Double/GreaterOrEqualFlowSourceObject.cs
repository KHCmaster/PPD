using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Double
{
    [ToolTipText("GreaterOrEqual_Summary")]
    public partial class GreaterOrEqualFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Double.GreaterOrEqual"; }
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

        [ToolTipText("GreaterOrEqual_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A >= B;
            }
        }
    }
}
