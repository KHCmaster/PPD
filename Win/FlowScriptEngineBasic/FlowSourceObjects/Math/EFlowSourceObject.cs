using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_E_Summary")]
    public partial class EFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.E"; }
        }

        [ToolTipText("Math_E_Value")]
        public double Value
        {
            get
            {
                return System.Math.E;
            }
        }
    }
}
