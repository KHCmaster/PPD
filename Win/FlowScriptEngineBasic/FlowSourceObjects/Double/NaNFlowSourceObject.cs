using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Double
{
    [ToolTipText("Nan_Summary")]
    public partial class NaNFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Double.NaN"; }
        }

        [ToolTipText("Nan_Value")]
        public double Value
        {
            get
            {
                return double.NaN;
            }
        }
    }
}
