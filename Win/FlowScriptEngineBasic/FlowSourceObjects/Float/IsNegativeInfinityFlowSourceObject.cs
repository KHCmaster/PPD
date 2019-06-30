using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Float
{
    [ToolTipText("IsNegativeInfinity_Summary")]
    public partial class IsNegativeInfinityFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Float.IsNegativeInfinity"; }
        }

        [ToolTipText("IsNegativeInfinity_A")]
        public float A
        {
            private get;
            set;
        }

        [ToolTipText("IsNegativeInfinity_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(A));
                return float.IsNegativeInfinity(A);
            }
        }
    }
}
