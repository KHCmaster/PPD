using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Min_Summary")]
    public partial class MinFloatFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Min"; }
        }

        [ToolTipText("FirstArgument")]
        public float A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public float B
        {
            private get;
            set;
        }

        [ToolTipText("Math_Min_Value")]
        public float Value
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
