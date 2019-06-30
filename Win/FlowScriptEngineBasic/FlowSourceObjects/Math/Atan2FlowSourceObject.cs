using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Math
{
    [ToolTipText("Math_Atan2_Summary", "Math_Atan2_Remark")]
    public partial class Atan2FlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Math.Atan2"; }
        }

        [ToolTipText("FirstArgument")]
        public double X
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public double Y
        {
            private get;
            set;
        }

        [ToolTipText("Math_Atan2_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(X));
                SetValue(nameof(Y));
                return System.Math.Atan2(Y, X);
            }
        }
    }
}
