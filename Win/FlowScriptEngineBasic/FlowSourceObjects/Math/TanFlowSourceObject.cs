using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Tan_Summary")]
    public partial class TanFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Tan"; }
        }

        [ToolTipText("Math_Tan_A")]
        public double A
        {
            private get;
            set;
        }

        [ToolTipText("Math_Tan_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                return System.Math.Tan(A);
            }
        }
    }
}
