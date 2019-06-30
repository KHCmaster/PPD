using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Boolean
{
    [ToolTipText("Boolean_FalseString_Summary")]
    public partial class FalseStringFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Boolean.FalseString"; }
        }

        [ToolTipText("Boolean_FalseString_Value")]
        public string Value
        {
            get
            {
                return bool.FalseString;
            }
        }
    }
}
