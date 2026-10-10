using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Bit
{
    [ToolTipText("Bit_LeastSignificant_Summary")]
    public partial class LeastSignificantFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Bit.LeastSignificant"; }
        }

        [ToolTipText("Bit_LeastSignificant_Value")]
        public int Value
        {
            private get;
            set;
        }

        [ToolTipText("Bit_LeastSignificant_Index")]
        public int Index
        {
            get
            {
                SetValue(nameof(Value));
                // 1 になっている一番下のビットの位置（ないときは -1）。以前は位置ではなく、そのビットだけを残した値を返していた
                return Value == 0 ? -1 : System.Numerics.BitOperations.TrailingZeroCount(Value);
            }
        }
    }
}
