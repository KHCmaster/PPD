using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Abs_Summary")]
    public partial class AbsFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Abs"; }
        }

        [ToolTipText("Math_Abs_A")]
        public double A
        {
            private get;
            set;
        }

        [ToolTipText("Math_Abs_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                return System.Math.Abs(A);
            }
        }
    }
}
