using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Float
{
    [ToolTipText("LessOrEqual_Summary")]
    public partial class LessOrEqualFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Float.LessOrEqual"; }
        }

        [ToolTipText("FirstArgument")]
        public float A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public float B
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
