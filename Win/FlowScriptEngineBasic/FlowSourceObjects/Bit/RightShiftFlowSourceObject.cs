using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Bit
{
    [ToolTipText("Bit_RightShift_Summary")]
    public partial class RightShiftFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Bit.RightShift"; }
        }

        [ToolTipText("Bit_RightShift_A")]
        public int A
        {
            private get;
            set;
        }

        [ToolTipText("Bit_RightShift_B")]
        public int B
        {
            private get;
            set;
        }

        [ToolTipText("Bit_RightShift_Value")]
        public int Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A >> B;
            }
        }
    }
}
