using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Float
{
    [ToolTipText("IsNan_Summary")]
    public partial class IsNaNFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Float.IsNaN"; }
        }

        [ToolTipText("IsNan_A")]
        public float A
        {
            private get;
            set;
        }

        [ToolTipText("IsNan_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(A));
                return float.IsNaN(A);
            }
        }
    }
}
