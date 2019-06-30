using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Double
{
    [ToolTipText("Min_Summary")]
    public partial class MinFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Double.Min"; }
        }

        [ToolTipText("Min_Value")]
        public double Value
        {
            get
            {
                return double.MinValue;
            }
        }
    }
}
