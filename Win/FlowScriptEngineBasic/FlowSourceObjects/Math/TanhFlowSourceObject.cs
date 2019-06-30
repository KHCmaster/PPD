using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Tanh_Summary")]
    public partial class TanhFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Tanh"; }
        }

        [ToolTipText("Math_Tanh_A")]
        public double A
        {
            private get;
            set;
        }

        [ToolTipText("Math_Tanh_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                return System.Math.Tanh(A);
            }
        }
    }
}