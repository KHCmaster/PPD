using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_Equal_Summary")]
    public partial class EqualFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "DateTime.Equal"; }
        }

        [ToolTipText("FirstArgument")]
        public System.DateTime A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public System.DateTime B
        {
            private get;
            set;
        }

        [ToolTipText("DateTime_Equal_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A == B;
            }
        }
    }
}
