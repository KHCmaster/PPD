using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Sinh_Summary")]
    public partial class SinhFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Sinh"; }
        }

        [ToolTipText("Math_Sinh_A")]
        public double A
        {
            private get;
            set;
        }

        [ToolTipText("Math_Sinh_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                return System.Math.Sinh(A);
            }
        }
    }
}
