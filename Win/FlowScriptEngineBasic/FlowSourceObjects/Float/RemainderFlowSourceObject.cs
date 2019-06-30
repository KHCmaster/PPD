using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Float
{
    [ToolTipText("Remainder_Summary")]
    public partial class RemainderFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Float.Remainder"; }
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

        [ToolTipText("Remainder_Value")]
        public float Value
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
