using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Bit
{
    [ToolTipText("Bit_Xor_Summary")]
    public partial class XorFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Bit.XOR"; }
        }

        [ToolTipText("Bit_Xor_A")]
        public int A
        {
            private get;
            set;
        }

        [ToolTipText("Bit_Xor_B")]
        public int B
        {
            private get;
            set;
        }

        [ToolTipText("Bit_Xor_Value")]
        public int Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A ^ B;
            }
        }
    }
}
