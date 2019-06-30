using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_LastIndexOf_Summary", "String_LastIndexOf_Remark")]
    public partial class LastIndexOfFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.LastIndexOf"; }
        }

        [ToolTipText("String_LastIndexOf_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_LastIndexOf_B")]
        public string B
        {
            private get;
            set;
        }

        [ToolTipText("String_LastIndexOf_Value")]
        public int Value
        {
            get
            {
                SetValue(nameof(A));
                if (A != null)
                {
                    SetValue(nameof(B));
                    return A.LastIndexOf(B);
                }
                return -1;
            }
        }
    }
}
