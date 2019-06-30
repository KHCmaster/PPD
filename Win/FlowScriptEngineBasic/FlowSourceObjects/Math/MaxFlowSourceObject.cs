using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Max_Summary")]
    public partial class MaxFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Max"; }
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

        [ToolTipText("Math_Max_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return System.Math.Max(A, B);
            }
        }
    }
}
