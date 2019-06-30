using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Asin_Summary")]
    public partial class AsinFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Asin"; }
        }

        [ToolTipText("Math_Asin_A")]
        public double A
        {
            private get;
            set;
        }

        [ToolTipText("Math_Asin_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                return System.Math.Asin(A);
            }
        }
    }
}
