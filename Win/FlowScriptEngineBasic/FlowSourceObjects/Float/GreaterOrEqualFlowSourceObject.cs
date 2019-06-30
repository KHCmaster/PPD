using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Float
{
    [ToolTipText("GreaterOrEqual_Summary")]
    public partial class GreaterOrEqualFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Float.GreaterOrEqual"; }
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
