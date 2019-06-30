using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Bit
{
    [ToolTipText("Bit_LeadingZerosCount_Summary")]
    public partial class LeadingZerosCountFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Bit.LeadingZerosCount"; }
        }

        [ToolTipText("Bit_LeadingZerosCount_Value")]
        public int Value
        {
            private get;
            set;
        }

        [ToolTipText("Bit_LeadingZerosCount_Count")]
        public int Count
        {
            get
            {
                SetValue(nameof(Value));
                var x = (uint)Value;
                x |= (x >> 1);
                x |= (x >> 2);
                x |= (x >> 4);
                x |= (x >> 8);
                x |= (x >> 16);
                return BitUtility.Int32BitCount() - BitUtility.Ones((int)x);
            }
        }
    }
}
