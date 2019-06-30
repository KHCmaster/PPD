using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.TimeSpan
{
    [ToolTipText("Greater_Summary")]
    public partial class GreaterFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "TimeSpan.Greater"; }
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
