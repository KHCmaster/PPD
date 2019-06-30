using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_IndexOf_Summary", "String_IndexOf_Remark")]
    public partial class IndexOfFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.IndexOf"; }
        }

        [ToolTipText("String_IndexOf_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_IndexOf_B")]
        public string B
        {
            private get;
            set;
        }

        [ToolTipText("String_IndexOf_Value")]
        public int Value
        {
            get
            {
                SetValue(nameof(A));
                if (A != null)
                {
                    SetValue(nameof(B));
                    return A.IndexOf(B);
                }
                return -1;
            }
        }
    }
}
