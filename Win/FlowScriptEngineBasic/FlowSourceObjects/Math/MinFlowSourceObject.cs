using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Min_Summary")]
    public partial class MinFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Min"; }
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

        [ToolTipText("Math_Min_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return System.Math.Min(A, B);
            }
        }
    }
}
