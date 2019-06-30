using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.TimeSpan
{
    [ToolTipText("TimeSpan_Add_Summary")]
    public partial class AddFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "TimeSpan.Add"; }
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

        [ToolTipText("TimeSpan_Add_Value")]
        public System.TimeSpan Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A + B;
            }
        }
    }
}
