using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Min_Summary")]
    public partial class MinInt32FlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Min"; }
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

        [ToolTipText("Math_Min_Value")]
        public int Value
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
