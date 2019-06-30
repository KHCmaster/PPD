using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_IEEERemainder_Summary", "Math_IEEERemainder_Remark")]
    public partial class IEEERemainderFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.IEEERemainder"; }
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

        [ToolTipText("Math_IEEERemainder_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return System.Math.IEEERemainder(A, B);
            }
        }
    }
}
