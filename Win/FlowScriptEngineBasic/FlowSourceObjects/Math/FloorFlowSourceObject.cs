using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Floor_Summary")]
    public partial class FloorFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Floor"; }
        }

        [ToolTipText("Math_Floor_A")]
        public double A
        {
            private get;
            set;
        }

        [ToolTipText("Math_Floor_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                return System.Math.Floor(A);
            }
        }
    }
}
