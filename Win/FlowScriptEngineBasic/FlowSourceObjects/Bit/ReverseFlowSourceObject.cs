using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Bit
{
    [ToolTipText("Bit_Reverse_Summary")]
    public partial class ReverseFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Bit.Reverse"; }
        }

        [ToolTipText("Bit_Reverse_Value")]
        public int Value
        {
            private get;
            set;
        }

        [ToolTipText("Bit_Reverse_Result")]
        public int Result
        {
            get
            {
                SetValue(nameof(Value));
                var temp = (uint)Value;
                temp = (((temp & 0xaaaaaaaa) >> 1) | ((temp & 0x55555555) << 1));
                temp = (((temp & 0xcccccccc) >> 2) | ((temp & 0x33333333) << 2));
                temp = (((temp & 0xf0f0f0f0) >> 4) | ((temp & 0x0f0f0f0f) << 4));
                temp = (((temp & 0xff00ff00) >> 8) | ((temp & 0x00ff00ff) << 8));
                temp = ((temp >> 16) | (temp << 16));
                return (int)temp;
            }
        }
    }
}
