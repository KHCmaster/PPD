using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_Equal_Summary")]
    public partial class EqualFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.Equal"; }
        }

        [ToolTipText("String_Equal_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_Equal_B")]
        public string B
        {
            private get;
            set;
        }

        [ToolTipText("String_Equal_Value")]
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
