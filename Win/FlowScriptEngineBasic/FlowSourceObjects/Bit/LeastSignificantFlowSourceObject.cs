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
                return Value & -Value;
            }
        }
    }
}
