using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_DivRem_Summary", "Math_DivRem_Remark")]
    public partial class DivRemFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.DivRem"; }
        }

        [ToolTipText("FirstArgument")]
        public int A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public int B
        {
            private get;
            set;
        }

        [ToolTipText("Math_DivRem_Remainder")]
        public int Remainder
        {
            get;
            private set;
        }

        [ToolTipText("Math_DivRem_Quotient")]
        public int Quotient
        {
            get;
            private set;
        }

        [ToolTipText("Math_DivRem_In")]
        public void In(FlowEventArgs e)
        {
            SetValue(nameof(A));
            SetValue(nameof(B));
            if (B != 0)
            {
                Quotient = System.Math.DivRem(A, B, out int remainder);
                Remainder = remainder;
            }
            else
            {
                Quotient = A > 0 ? int.MaxValue : int.MinValue;
                Remainder = 0;
            }
        }
    }
}