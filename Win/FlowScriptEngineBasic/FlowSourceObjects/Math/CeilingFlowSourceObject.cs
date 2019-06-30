using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Ceiling_Summary")]
    public partial class CeilingFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Ceiling"; }
        }

        [ToolTipText("Math_Ceiling_A")]
        public double A
        {
            private get;
            set;
        }

        [ToolTipText("Math_Ceiling_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                return System.Math.Ceiling(A);
            }
        }
    }
}
