using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector3
{
    [ToolTipText("Substract_Summary")]
    public partial class SubtractFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector3.Subtract"; }
        }

        [ToolTipText("FirstArgument")]
        public SharpDX.Vector3 A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public SharpDX.Vector3 B
        {
            private get;
            set;
        }

        [ToolTipText("Substract_Value")]
        public SharpDX.Vector3 Value
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
