using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Bit
{
    [ToolTipText("Bit_TrailingZerosCount_Summary")]
    public partial class TrailingZerosCountFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Bit.TrailingZerosCount"; }
        }

        [ToolTipText("Bit_TrailingZerosCount_Value")]
        public int Value
        {
            private get;
            set;
        }

        [ToolTipText("Bit_TrailingZerosCount_Count")]
        public int Count
        {
            get
            {
                SetValue(nameof(Value));
                return BitUtility.Ones((Value & -Value) - 1);
            }
        }
    }
}
