using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Double
{
    [ToolTipText("NegativeInfinity_Summary")]
    public partial class NegativeInfinityFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Double.NegativeInfinity"; }
        }

        [ToolTipText("NegativeInfinity_Value")]
        public double Value
        {
            get
            {
                return double.NegativeInfinity;
            }
        }
    }
}
