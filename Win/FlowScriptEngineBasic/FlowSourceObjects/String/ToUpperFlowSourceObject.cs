using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_ToUpper_Summary")]
    public partial class ToUpperFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.ToUpper"; }
        }

        [ToolTipText("String_ToUpper_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_ToUpper_Value")]
        public string Value
        {
            get
            {
                SetValue(nameof(A));
                if (A != null)
                {
                    return A.ToUpper();
                }
                return null;
            }
        }
    }
}
