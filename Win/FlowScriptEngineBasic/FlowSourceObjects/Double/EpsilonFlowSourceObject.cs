using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Double
{
    [ToolTipText("Epsilon_Summary")]
    public partial class EpsilonFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Double.Epsilon"; }
        }

        [ToolTipText("Epsilon_Value")]
        public double Value
        {
            get
            {
                return double.Epsilon;
            }
        }
    }
}
