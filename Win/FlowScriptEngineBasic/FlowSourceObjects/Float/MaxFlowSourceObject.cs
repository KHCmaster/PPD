using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Float
{
    [ToolTipText("Max_Summary")]
    public partial class MaxFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Float.Max"; }
        }

        [ToolTipText("Max_Value")]
        public float Value
        {
            get
            {
                return float.MaxValue;
            }
        }
    }
}
