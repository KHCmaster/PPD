using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Int32
{
    [ToolTipText("Remainder_Summary")]
    public partial class RemainderFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Int32.Remainder"; }
        }

        [ToolTipText("FirstArgument")]
        public int A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public int B
        {
            private get;
            set;
        }

        [ToolTipText("Remainder_Value")]
        public int Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                if (B != 0)
                {
                    return A % B;
                }
                else
                {
                    return -1;
                }
            }
        }
    }
}
