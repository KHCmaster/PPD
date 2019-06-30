using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Float
{
    [ToolTipText("PositiveInfinity_Summary")]
    public partial class PositiveInfinityFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Float.PositiveInfinity"; }
        }

        [ToolTipText("PositiveInfinity_Value")]
        public float Value
        {
            get
            {
                return float.PositiveInfinity;
            }
        }
    }
}
