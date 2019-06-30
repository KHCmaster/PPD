using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Double
{
    [ToolTipText("IsInfinity_Summary")]
    public partial class IsInfinityFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Double.IsInfinity"; }
        }

        [ToolTipText("IsInfinity_A")]
        public double A
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
                return double.IsInfinity(A);
            }
        }
    }
}
