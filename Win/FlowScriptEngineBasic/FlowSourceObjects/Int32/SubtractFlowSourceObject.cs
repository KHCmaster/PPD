using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Int32
{
    [ToolTipText("Substract_Summary")]
    public partial class SubtractFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Int32.Subtract"; }
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

        [ToolTipText("Substract_Value")]
        public int Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A - B;
            }
        }
    }
}
