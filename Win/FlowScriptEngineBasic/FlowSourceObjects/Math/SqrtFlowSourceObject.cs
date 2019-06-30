using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Sqrt_Summary")]
    public partial class SqrtFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Sqrt"; }
        }

        [ToolTipText("Math_Sqrt_A")]
        public double A
        {
            private get;
            set;
        }

        [ToolTipText("Math_Sqrt_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                return System.Math.Sqrt(A);
            }
        }
    }
}