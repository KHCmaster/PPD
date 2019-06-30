using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_PadRight_Summary")]
    public partial class PadRightFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.PadRight"; }
        }

        [ToolTipText("String_PadRight_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_PadRight_TotalWidth")]
        public int TotalWidth
        {
            private get;
            set;
        }

        [ToolTipText("String_PadRight_Value")]
        public string Value
        {
            get
            {
                SetValue(nameof(A));
                if (A != null)
                {
                    SetValue(nameof(TotalWidth));
                    return A.PadRight(TotalWidth);
                }
                return null;
            }
        }
    }
}
