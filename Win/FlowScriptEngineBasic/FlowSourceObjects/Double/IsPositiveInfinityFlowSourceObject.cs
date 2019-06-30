using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Double
{
    [ToolTipText("IsPositiveInfinity_Summary")]
    public partial class IsPositiveInfinityFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Double.IsPositiveInfinity"; }
        }

        [ToolTipText("IsPositiveInfinity_A")]
        public double A
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
                return double.IsPositiveInfinity(A);
            }
        }
    }
}
