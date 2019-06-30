using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Truncate_Summary")]
    public partial class TruncateFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Truncate"; }
        }

        [ToolTipText("Math_Truncate_A")]
        public double A
        {
            private get;
            set;
        }

        [ToolTipText("Math_Truncate_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                return System.Math.Truncate(A);
            }
        }
    }
}
