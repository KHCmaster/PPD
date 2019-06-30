using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_Trim_Summary")]
    public partial class TrimFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.Trim"; }
        }

        [ToolTipText("String_Trim_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_Trim_Value")]
        public string Value
        {
            get
            {
                SetValue(nameof(A));
                if (A != null)
                {
                    return A.Trim();
                }
                return null;
            }
        }
    }
}
