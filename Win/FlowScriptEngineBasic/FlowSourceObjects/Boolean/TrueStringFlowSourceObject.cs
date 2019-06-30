using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Boolean
{
    [ToolTipText("Boolean_TrueString_Summary")]
    public partial class TrueStringFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Boolean.TrueString"; }
        }

        [ToolTipText("Boolean_TrueString_Value")]
        public string Value
        {
            get
            {
                return bool.TrueString;
            }
        }
    }
}
