using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_PadLeft_Summary")]
    public partial class PadLeftFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.PadLeft"; }
        }

        [ToolTipText("String_PadLeft_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_PadLeft_TotalWidth")]
        public int TotalWidth
        {
            private get;
            set;
        }

        [ToolTipText("String_PadLeft_Value")]
        public string Value
        {
            get
            {
                SetValue(nameof(A));
                if (A != null)
                {
                    SetValue(nameof(TotalWidth));
                    return A.PadLeft(TotalWidth);
                }
                return null;
            }
        }
    }
}
