using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Float
{
    [ToolTipText("Epsilon_Summary")]
    public partial class EpsilonFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Float.Epsilon"; }
        }

        [ToolTipText("Epsilon_Value")]
        public float Value
        {
            get
            {
                return float.Epsilon;
            }
        }
    }
}
