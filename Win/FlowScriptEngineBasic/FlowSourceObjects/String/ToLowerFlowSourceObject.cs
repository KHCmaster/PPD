using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_ToLower_Summary")]
    public partial class ToLowerFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.ToLower"; }
        }

        [ToolTipText("String_ToLower_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_ToLower_Value")]
        public string Value
        {
            get
            {
                SetValue(nameof(A));
                if (A != null)
                {
                    return A.ToLower();
                }
                return null;
            }
        }
    }
}
