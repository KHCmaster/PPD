using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Bit
{
    [ToolTipText("Bit_MostSignificant_Summary")]
    public partial class MostSignificantFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Bit.MostSignificant"; }
        }

        [ToolTipText("Bit_MostSignificant_Value")]
        public int Value
        {
            private get;
            set;
        }

        [ToolTipText("Bit_MostSignificant_Index")]
        public int Index
        {
            get
            {
                SetValue(nameof(Value));
                // 1 になっている一番上のビットの位置（ないときは -1）。以前は位置ではなく、そのビットだけを残した値を返していた
                return Value == 0 ? -1 : 31 - System.Numerics.BitOperations.LeadingZeroCount((uint)Value);
            }
        }
    }
}
