using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Round_Summary")]
    public partial class RoundFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Round"; }
        }

        [ToolTipText("Math_Round_A")]
        public double A
        {
            private get;
            set;
        }

        [ToolTipText("Math_Round_B")]
        public int B
        {
            private get;
            set;
        }

        [ToolTipText("Math_Round_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return System.Math.Round(A, B);
            }
        }
    }
}
