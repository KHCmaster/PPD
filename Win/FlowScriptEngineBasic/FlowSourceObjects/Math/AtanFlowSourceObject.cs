using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Atan_Summary")]
    public partial class AtanFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Atan"; }
        }

        [ToolTipText("Math_Atan_A")]
        public double A
        {
            private get;
            set;
        }

        [ToolTipText("Math_Atan_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                return System.Math.Atan(A);
            }
        }
    }
}
