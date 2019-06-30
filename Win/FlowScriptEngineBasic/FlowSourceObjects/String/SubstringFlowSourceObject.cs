using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_Substring_Summary")]
    public partial class SubstringFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.Substring"; }
        }

        [ToolTipText("String_Substring_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_Substring_Index")]
        public int Index
        {
            private get;
            set;
        }

        [ToolTipText("String_Substring_Value")]
        public string Value
        {
            get
            {
                SetValue(nameof(A));
                if (A != null)
                {
                    SetValue(nameof(Index));
                    return A.Substring(Index);
                }
                return null;
            }
        }
    }
}
