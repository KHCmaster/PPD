using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Double
{
    [ToolTipText("Max_Summary")]
    public partial class MaxFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Double.Max"; }
        }

        [ToolTipText("Max_Value")]
        public double Value
        {
            get
            {
                return double.MaxValue;
            }
        }
    }
}
