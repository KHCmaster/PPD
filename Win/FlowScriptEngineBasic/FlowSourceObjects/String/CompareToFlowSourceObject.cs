using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_CompareTo_Summary", "String_CompareTo_Remark")]
    public partial class CompareToFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.ComapreTo"; }
        }

        [ToolTipText("String_CompareTo_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_CompareTo_B")]
        public string B
        {
            private get;
            set;
        }

        [ToolTipText("String_CompareTo_Value")]
        public int Value
        {
            get
            {
                SetValue(nameof(A));
                if (A != null)
                {
                    SetValue(nameof(B));
                    return A.CompareTo(B);
                }
                return -1;
            }
        }
    }
}
