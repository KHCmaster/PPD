using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Bit
{
    [ToolTipText("Bit_Or_Summary")]
    public partial class OrFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Bit.OR"; }
        }

        [ToolTipText("Bit_Or_A")]
        public int A
        {
            private get;
            set;
        }

        [ToolTipText("Bit_Or_B")]
        public int B
        {
            private get;
            set;
        }

        [ToolTipText("Bit_Or_Value")]
        public int Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A | B;
            }
        }
    }
}
