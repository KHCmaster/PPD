using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Logic
{
    [ToolTipText("Logic_Xor_Summary", "Logic_Xor_Remark")]
    public partial class XorFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Logic.XOR"; }
        }

        [ToolTipText("FirstArgument")]
        public bool A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public bool B
        {
            private get;
            set;
        }

        [ToolTipText("Logic_Xor_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A ^ B;
            }
        }
    }
}
