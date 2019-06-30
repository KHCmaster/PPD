using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Pi_Summary")]
    public partial class PiFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.PI"; }
        }

        [ToolTipText("Math_Pi_Value")]
        public double Value
        {
            get
            {
                return System.Math.PI;
            }
        }
    }
}
