using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Bit
{
    [ToolTipText("Bit_OnesCount_Summary")]
    public partial class OnesCountFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Bit.OnesCount"; }
        }

        [ToolTipText("Bit_OnesCount_Value")]
        public int Value
        {
            private get;
            set;
        }

        [ToolTipText("Bit_OnesCount_Count")]
        public int Count
        {
            get
            {
                SetValue(nameof(Value));
                return BitUtility.Ones(Value);
            }
        }
    }
}
