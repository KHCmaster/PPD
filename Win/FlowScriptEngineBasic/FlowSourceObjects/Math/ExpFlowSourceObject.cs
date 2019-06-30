using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Exp_Summary")]
    public partial class ExpFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Exp"; }
        }

        [ToolTipText("Math_Exp_A")]
        public double A
        {
            private get;
            set;
        }

        [ToolTipText("Math_Exp_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                return System.Math.Exp(A);
            }
        }
    }
}
