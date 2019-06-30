using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_Replace_Summary")]
    public partial class ReplaceFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.Replace"; }
        }

        [ToolTipText("String_Replace_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_Replace_Old")]
        public string Old
        {
            private get;
            set;
        }

        [ToolTipText("String_Replace_New")]
        public string New
        {
            private get;
            set;
        }

        [ToolTipText("String_Replace_Value")]
        public string Value
        {
            get
            {
                SetValue(nameof(A));
                if (A != null)
                {
                    SetValue(nameof(Old));
                    SetValue(nameof(New));
                    return A.Replace(Old, New);
                }
                return null;
            }
        }
    }
}
