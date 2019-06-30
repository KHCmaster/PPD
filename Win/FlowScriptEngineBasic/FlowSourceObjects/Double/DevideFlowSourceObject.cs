using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Double
{
    [ToolTipText("Devide_Summary")]
    public partial class DevideFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Double.Devide"; }
        }

        [ToolTipText("FirstArgument")]
        public double A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public double B
        {
            private get;
            set;
        }

        [ToolTipText("Devide_Value")]
        public double Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A / B;
            }
        }
    }
}
