using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_Contains_Summary")]
    public partial class ContainsFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.Contains"; }
        }

        [ToolTipText("String_Contains_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_Contains_B")]
        public string B
        {
            private get;
            set;
        }

        [ToolTipText("String_Contains_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(A));
                if (A != null)
                {
                    SetValue(nameof(B));
                    return A.Contains(B);
                }
                return false;
            }
        }
    }
}
