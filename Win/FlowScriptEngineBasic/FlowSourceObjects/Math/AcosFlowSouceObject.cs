using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Acos_Summary")]
    public partial class AcosFlowSouceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Acos"; }
        }

        [ToolTipText("Math_Acos_A")]
        public double A
        {
            private get;
            set;
        }

        [ToolTipText("Math_Acos_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                return System.Math.Acos(A);
            }
        }
    }
}
