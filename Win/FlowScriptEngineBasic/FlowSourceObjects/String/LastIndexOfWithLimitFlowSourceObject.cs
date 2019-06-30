using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_LastIndexOf_Summary", "String_LastIndexOf_Remark")]
    public partial class LastIndexOfWithLimitFlowSourceObject : FlowSourceObjectBase
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

        [ToolTipText("String_LastIndexOf_StartIndex")]
        public int StartIndex
        {
            private get;
            set;
        }

        [ToolTipText("String_LastIndexOf_Count")]
        public int Count
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
                    SetValue(nameof(StartIndex));
                    SetValue(nameof(Count));
                    return A.LastIndexOf(B, StartIndex, Count);
                }
                return -1;
            }
        }
    }
}
