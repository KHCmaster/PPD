using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Pow_Summary", "Math_Pow_Remark")]
    public partial class PowFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Pow"; }
        }

        [ToolTipText("FirstArgument")]
        public double A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public double B
        {
            private get;
            set;
        }

        [ToolTipText("Math_Pow_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return System.Math.Pow(A, B);
            }
        }
    }
}
