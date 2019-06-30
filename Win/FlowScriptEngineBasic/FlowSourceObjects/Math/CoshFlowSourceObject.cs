using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Cosh_Summary")]
    public partial class CoshFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Cosh"; }
        }

        [ToolTipText("Math_Cosh_A")]
        public double A
        {
            private get;
            set;
        }

        [ToolTipText("Math_Cosh_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                return System.Math.Cosh(A);
            }
        }
    }
}
