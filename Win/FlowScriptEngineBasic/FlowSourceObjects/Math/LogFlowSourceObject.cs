using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Log_Summary")]
    public partial class LogFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Log"; }
        }

        [ToolTipText("Math_Log_A")]
        public double A
        {
            private get;
            set;
        }

        [ToolTipText("Math_Log_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                return System.Math.Log(A);
            }
        }
    }
}
