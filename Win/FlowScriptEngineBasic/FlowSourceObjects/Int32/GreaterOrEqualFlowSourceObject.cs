using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Int32
{
    [ToolTipText("GreaterOrEqual_Summary")]
    public partial class GreaterOrEqualFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Int32.GreaterOrEqual"; }
        }

        [ToolTipText("FirstArgument")]
        public int A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public int B
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
