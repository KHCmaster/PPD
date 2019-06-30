using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_IsNullOrEmpty_Summary")]
    public partial class IsNullOrEmptyFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.IsNullOrEmpty"; }
        }

        [ToolTipText("String_IsNullOrEmpty_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_IsNullOrEmpty_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(A));
                return System.String.IsNullOrEmpty(A);
            }
        }
    }
}
