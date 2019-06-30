using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Float
{
    [ToolTipText("Min_Summary")]
    public partial class MinFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Float.Min"; }
        }

        [ToolTipText("Min_Value")]
        public float Value
        {
            get
            {
                return float.MinValue;
            }
        }
    }
}
