using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.TimeSpan
{
    [ToolTipText("TimeSpan_Equal_Summary")]
    public partial class EqualFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "TimeSpan.Equal"; }
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

        [ToolTipText("TimeSpan_Equal_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A == B;
            }
        }
    }
}
