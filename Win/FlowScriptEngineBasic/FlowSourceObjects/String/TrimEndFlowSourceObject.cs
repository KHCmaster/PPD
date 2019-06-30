using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_TrimEnd_Summary")]
    public partial class TrimEndFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.TrimEnd"; }
        }

        [ToolTipText("String_TrimEnd_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_TrimEnd_Value")]
        public string Value
        {
            get
            {
                SetValue(nameof(A));
                if (A != null)
                {
                    return A.TrimEnd();
                }
                return null;
            }
        }
    }
}
