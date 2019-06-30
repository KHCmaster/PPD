using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Int32
{
    [ToolTipText("LessOrEqual_Summary")]
    public partial class LessOrEqualFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Int32.LessOrEqual"; }
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

        [ToolTipText("LessOrEqual_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A <= B;
            }
        }
    }
}
