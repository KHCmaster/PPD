using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Float
{
    [ToolTipText("NegativeInfinity_Summary")]
    public partial class NegativeInfinityFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Float.NegativeInfinity"; }
        }

        [ToolTipText("NegativeInfinity_Value")]
        public float Value
        {
            get
            {
                return float.NegativeInfinity;
            }
        }
    }
}
