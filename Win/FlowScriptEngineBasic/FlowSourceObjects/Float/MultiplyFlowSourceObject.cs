using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Float
{
    [ToolTipText("Multiply_Summary")]
    public partial class MultiplyFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Float.Multiply"; }
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

        [ToolTipText("Multiply_Value")]
        public float Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A * B;
            }
        }
    }
}
