using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Float
{
    [ToolTipText("IsPositiveInfinity_Summary")]
    public partial class IsPositiveInfinityFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Float.IsPositiveInfinity"; }
        }

        [ToolTipText("IsPositiveInfinity_A")]
        public float A
        {
            private get;
            set;
        }

        [ToolTipText("IsPositiveInfinity_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(A));
                return float.IsPositiveInfinity(A);
            }
        }
    }
}
