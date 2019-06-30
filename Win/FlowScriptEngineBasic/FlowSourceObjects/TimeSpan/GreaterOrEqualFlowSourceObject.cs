using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.TimeSpan
{
    [ToolTipText("GreaterOrEqual_Summary")]
    public partial class GreaterOrEqualFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "TimeSpan.GreaterOrEqual"; }
        }

        [ToolTipText("FirstArgument")]
        public System.TimeSpan A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public System.TimeSpan B
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
