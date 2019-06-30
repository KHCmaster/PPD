using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_Remove_Summary")]
    public partial class RemoveCountFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.Remove"; }
        }

        [ToolTipText("String_Remove_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_Remove_Index")]
        public int Index
        {
            private get;
            set;
        }

        [ToolTipText("String_Remove_Count")]
        public int Count
        {
            private get;
            set;
        }

        [ToolTipText("String_Remove_Value")]
        public string Value
        {
            get
            {
                SetValue(nameof(A));
                if (A != null)
                {
                    SetValue(nameof(Index));
                    SetValue(nameof(Count));
                    return A.Remove(Index, Count);
                }
                return null;
            }
        }
    }
}
