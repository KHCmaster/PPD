using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Cos_Summary")]
    public partial class CosFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Cos"; }
        }

        [ToolTipText("Math_Cos_A")]
        public double A
        {
            private get;
            set;
        }

        [ToolTipText("Math_Cos_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                return System.Math.Cos(A);
            }
        }
    }
}
