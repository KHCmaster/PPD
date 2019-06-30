using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Bit
{
    [ToolTipText("Bit_PreviousLargestPowerOfTwo_Summary")]
    public partial class PreviousLargestPowerOfTwoFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Bit.PreviousLargestPowerOfTwo"; }
        }

        [ToolTipText("Bit_PreviousLargestPowerOfTwo_Value")]
        public int Value
        {
            private get;
            set;
        }

        [ToolTipText("Bit_PreviousLargestPowerOfTwo_Result")]
        public int Result
        {
            get
            {
                SetValue(nameof(Value));
                var x = (uint)Value;
                x = x | (x >> 1);
                x = x | (x >> 2);
                x = x | (x >> 4);
                x = x | (x >> 8);
                x = x | (x >> 16);
                return (int)(x - (x >> 1));
            }
        }
    }
}
