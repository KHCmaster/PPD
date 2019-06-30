using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_Add_Summary")]
    public partial class AddFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.Add"; }
        }

        [ToolTipText("String_Add_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_Add_B")]
        public string B
        {
            private get;
            set;
        }

        [ToolTipText("String_Add_Value")]
        public string Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A + B;
            }
        }
    }
}
