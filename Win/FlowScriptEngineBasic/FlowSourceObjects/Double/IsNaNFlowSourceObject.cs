using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Double
{
    [ToolTipText("IsNan_Summary")]
    public partial class IsNaNFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Double.IsNaN"; }
        }

        [ToolTipText("IsNan_A")]
        public double A
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
                return double.IsNaN(A);
            }
        }
    }
}
