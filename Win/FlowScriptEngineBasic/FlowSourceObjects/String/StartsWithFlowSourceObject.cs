using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_StartsWith_Summary")]
    public partial class StartsWithFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.StartsWith"; }
        }

        [ToolTipText("String_StartsWith_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_StartsWith_B")]
        public string B
        {
            private get;
            set;
        }

        [ToolTipText("String_StartsWith_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(A));
                if (A != null)
                {
                    SetValue(nameof(B));
                    return A.StartsWith(B);
                }
                return false;
            }
        }
    }
}
