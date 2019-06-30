using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Double
{
    [ToolTipText("PositiveInfinity_Summary")]
    public partial class PositiveInfinityFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Double.PositiveInfinity"; }
        }

        [ToolTipText("PositiveInfinity_Value")]
        public double Value
        {
            get
            {
                return double.PositiveInfinity;
            }
        }
    }
}
