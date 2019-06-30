using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.TimeSpan
{
    [ToolTipText("Less_Summary")]
    public partial class LessFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "TimeSpan.Less"; }
        }

        [ToolTipText("FirstArgument")]
        public System.TimeSpan A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public System.TimeSpan B
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
