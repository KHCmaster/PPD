using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Float
{
    [ToolTipText("IsInfinity_Summary")]
    public partial class IsInfinityFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Float.IsInfinity"; }
        }

        [ToolTipText("IsInfinity_A")]
        public float A
        {
            private get;
            set;
        }

        [ToolTipText("IsInfinity_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(A));
                return float.IsInfinity(A);
            }
        }
    }
}
