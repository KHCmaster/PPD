using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Float
{
    [ToolTipText("Less_Summary")]
    public partial class LessFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Float.Less"; }
        }

        [ToolTipText("FirstArgument")]
        public float A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public float B
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
