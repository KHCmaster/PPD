using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Int32
{
    [ToolTipText("Multiply_Summary")]
    public partial class MultiplyFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Int32.Multiply"; }
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

        [ToolTipText("Multiply_Value")]
        public int Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A * B;
            }
        }
    }
}
