using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Bit
{
    [ToolTipText("Bit_IsPowerOfTwo_Summary")]
    public partial class IsPowerOfTwoFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Bit.IsPowerOfTwo"; }
        }

        [ToolTipText("Bit_IsPowerOfTwo_Value")]
        public int Value
        {
            private get;
            set;
        }

        [ToolTipText("Bit_IsPowerOfTwo_Result")]
        public bool Result
        {
            get
            {
                SetValue(nameof(Value));
                return (Value & (Value - 1)) == 0;
            }
        }
    }
}
