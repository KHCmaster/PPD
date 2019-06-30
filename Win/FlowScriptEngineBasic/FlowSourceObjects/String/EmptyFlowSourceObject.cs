using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_Empty_Summary")]
    public partial class EmptyFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.Empty"; }
        }

        [ToolTipText("String_Empty_Value")]
        public string Value
        {
            get
            {
                return "";
            }
        }
    }
}
