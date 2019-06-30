using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_EndsWith_Summary")]
    public partial class EndsWithFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.EndsWith"; }
        }

        [ToolTipText("String_EndsWith_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_EndsWith_B")]
        public string B
        {
            private get;
            set;
        }

        [ToolTipText("String_EndsWith_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(A));
                if (A != null)
                {
                    SetValue(nameof(B));
                    return A.EndsWith(B);
                }
                return false;
            }
        }
    }
}
