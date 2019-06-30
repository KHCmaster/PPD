using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_Remove_Summary")]
    public partial class RemoveFlowSourceObject : FlowSourceObjectBase
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

        [ToolTipText("String_Remove_Value")]
        public string Value
        {
            get
            {
                SetValue(nameof(A));
                if (A != null)
                {
                    SetValue(nameof(Index));
                    return A.Remove(Index);
                }
                return null;
            }
        }
    }
}
