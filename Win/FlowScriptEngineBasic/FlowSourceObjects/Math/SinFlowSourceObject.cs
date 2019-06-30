using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Sin_Summary")]
    public partial class SinFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Sin"; }
        }

        [ToolTipText("Math_Sin_A")]
        public double A
        {
            private get;
            set;
        }

        [ToolTipText("Math_Sin_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                return System.Math.Sin(A);
            }
        }
    }
}
