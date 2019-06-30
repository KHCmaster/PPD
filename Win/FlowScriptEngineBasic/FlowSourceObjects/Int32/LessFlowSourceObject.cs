using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Int32
{
    [ToolTipText("Less_Summary")]
    public partial class LessFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Int32.Less"; }
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

        [ToolTipText("Less_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A < B;
            }
        }
    }
}
