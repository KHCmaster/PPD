using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Bit
{
    [ToolTipText("Bit_Complement_Summary")]
    public partial class ComplementFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Bit.Complement"; }
        }

        [ToolTipText("Bit_Complement_Value")]
        public int Value
        {
            private get;
            set;
        }

        [ToolTipText("Bit_Complement_Result")]
        public int Result
        {
            get
            {
                SetValue(nameof(Value));
                return ~Value;
            }
        }
    }
}
