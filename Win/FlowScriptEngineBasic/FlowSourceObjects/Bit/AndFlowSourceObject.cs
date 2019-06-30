using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Bit
{
    [ToolTipText("Bit_And_Summary")]
    public partial class AndFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Bit.AND"; }
        }

        [ToolTipText("Bit_And_A")]
        public int A
        {
            private get;
            set;
        }

        [ToolTipText("Bit_And_B")]
        public int B
        {
            private get;
            set;
        }

        [ToolTipText("Bit_And_Value")]
        public int Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A & B;
            }
        }
    }
}
