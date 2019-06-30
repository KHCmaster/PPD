using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_Insert_Summary", "String_Insert_Remark")]
    public partial class InsertFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.Insert"; }
        }

        [ToolTipText("String_Insert_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_Insert_B")]
        public string B
        {
            private get;
            set;
        }

        [ToolTipText("String_Insert_Index")]
        public int Index
        {
            private get;
            set;
        }

        [ToolTipText("String_Insert_Value")]
        public string Value
        {
            get
            {
                SetValue(nameof(A));
                if (A != null)
                {
                    SetValue(nameof(B));
                    SetValue(nameof(Index));
                    return A.Insert(Index, B);
                }
                return null;
            }
        }
    }
}
