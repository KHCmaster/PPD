using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_Length_Summary")]
    public partial class LengthFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.Length"; }
        }

        [ToolTipText("String_Length_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_Length_Value")]
        public int Length
        {
            get
            {
                SetValue(nameof(A));
                if (A != null)
                {
                    return A.Length;
                }
                return 0;
            }
        }
    }
}
