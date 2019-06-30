using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Double
{
    [ToolTipText("IsNegativeInfinity_Summary")]
    public partial class IsNegativeInfinityFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Double.IsNegativeInfinity"; }
        }

        [ToolTipText("IsNegativeInfinity_A")]
        public double A
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
                return double.IsNegativeInfinity(A);
            }
        }
    }
}
