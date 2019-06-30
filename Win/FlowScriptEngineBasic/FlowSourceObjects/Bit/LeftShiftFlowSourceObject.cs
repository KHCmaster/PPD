using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Bit
{
    [ToolTipText("Bit_LeftShift_Summary")]
    public partial class LeftShiftFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Bit.LeftShift"; }
        }

        [ToolTipText("Bit_LeftShift_A")]
        public int A
        {
            private get;
            set;
        }

        [ToolTipText("Bit_LeftShift_B")]
        public int B
        {
            private get;
            set;
        }

        [ToolTipText("Bit_LeftShift_Value")]
        public int Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A << B;
            }
        }
    }
}
