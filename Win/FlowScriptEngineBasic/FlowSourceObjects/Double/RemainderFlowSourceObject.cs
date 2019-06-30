using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Double
{
    [ToolTipText("Remainder_Summary")]
    public partial class RemainderFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Double.Remainder"; }
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

        [ToolTipText("Remainder_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A % B;
            }
        }
    }
}
