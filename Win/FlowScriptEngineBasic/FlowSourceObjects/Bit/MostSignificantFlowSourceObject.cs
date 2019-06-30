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
                var x = (uint)Value;
                x |= (x >> 1);
                x |= (x >> 2);
                x |= (x >> 4);
                x |= (x >> 8);
                x |= (x >> 16);
                return (int)(x & ~(x >> 1));
            }
        }
    }
}
