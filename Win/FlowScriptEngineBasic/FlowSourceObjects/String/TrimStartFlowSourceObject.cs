using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_TrimStart_Summary")]
    public partial class TrimStartFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.TrimStart"; }
        }

        [ToolTipText("String_TrimStart_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_TrimStart_Value")]
        public string Value
        {
            get
            {
                SetValue(nameof(A));
                if (A != null)
                {
                    return A.TrimStart();
                }
                return null;
            }
        }
    }
}
