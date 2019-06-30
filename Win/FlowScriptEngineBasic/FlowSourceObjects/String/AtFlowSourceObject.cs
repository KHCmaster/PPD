using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_At_Summary")]
    public partial class AtFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.At"; }
        }

        [ToolTipText("String_At_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_At_Index")]
        public int Index
        {
            private get;
            set;
        }

        [ToolTipText("String_At_Value")]
        public string Value
        {
            get
            {
                SetValue(nameof(A));
                if (A != null)
                {
                    SetValue(nameof(Index));
                    return A[Index].ToString();
                }
                else
                {
                    return "";
                }
            }
        }
    }
}
