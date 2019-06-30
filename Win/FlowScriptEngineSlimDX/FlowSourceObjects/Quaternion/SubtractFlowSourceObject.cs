using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Quaternion
{
    [ToolTipText("Substract_Summary")]
    public partial class SubtractFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Quaternion.Subtract"; }
        }

        [ToolTipText("FirstArgument")]
        public SharpDX.Quaternion A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public SharpDX.Quaternion B
        {
            private get;
            set;
        }

        [ToolTipText("Substract_Value")]
        public SharpDX.Quaternion Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A - B;
            }
        }
    }
}
