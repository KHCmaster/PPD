using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Float
{
    [ToolTipText("Nan_Summary")]
    public partial class NaNFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Float.NaN"; }
        }

        [ToolTipText("Nan_Value")]
        public float Value
        {
            get
            {
                return float.NaN;
            }
        }
    }
}
