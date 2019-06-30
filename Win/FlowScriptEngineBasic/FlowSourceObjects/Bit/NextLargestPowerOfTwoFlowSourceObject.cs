using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Bit
{
    [ToolTipText("Bit_NextLargestPowerOfTwo_Summary")]
    public partial class NextLargestPowerOfTwoFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Bit.NextLargestPowerOfTwo"; }
        }

        [ToolTipText("Bit_NextLargestPowerOfTwo_Value")]
        public int Value
        {
            private get;
            set;
        }

        [ToolTipText("Bit_NextLargestPowerOfTwo_Result")]
        public int Result
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
                return (int)(x + 1);
            }
        }
    }
}
