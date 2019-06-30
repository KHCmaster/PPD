using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Bit
{
    [ToolTipText("Bit_ZerosCount_Summary")]
    public partial class ZerosCountFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Bit.ZerosCount"; }
        }

        [ToolTipText("Bit_ZerosCount_Value")]
        public int Value
        {
            private get;
            set;
        }

        [ToolTipText("Bit_ZerosCount_Count")]
        public int Count
        {
            get
            {
                SetValue(nameof(Value));
                return BitUtility.Int32BitCount() - BitUtility.Ones(Value);
            }
        }
    }
}
