using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_IndexOf_Summary", "String_IndexOf_Remark")]
    public partial class IndexOfWithLimitFlowSourceObject : FlowSourceObjectBase
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

        [ToolTipText("String_IndexOf_StartIndex")]
        public int StartIndex
        {
            private get;
            set;
        }

        [ToolTipText("String_IndexOf_Count")]
        public int Count
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
                    SetValue(nameof(StartIndex));
                    SetValue(nameof(Count));
                    return A.IndexOf(B, StartIndex, Count);
                }
                return -1;
            }
        }
    }
}