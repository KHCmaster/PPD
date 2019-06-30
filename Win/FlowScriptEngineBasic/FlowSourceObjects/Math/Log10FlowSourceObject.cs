using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Log10_Summary")]
    public partial class Log10FlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Log10"; }
        }

        [ToolTipText("Math_Log10_A")]
        public double A
        {
            private get;
            set;
        }

        [ToolTipText("Math_Log10_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                return System.Math.Log10(A);
            }
        }
    }
}
