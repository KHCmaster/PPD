using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Abs_Summary")]
    public partial class AbsFloatFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Abs"; }
        }

        [ToolTipText("Math_Abs_A")]
        public float A
        {
            private get;
            set;
        }

        [ToolTipText("Math_Abs_Value")]
        public float Value
        {
            get
            {
                SetValue(nameof(A));
                return System.Math.Abs(A);
            }
        }
    }
}
