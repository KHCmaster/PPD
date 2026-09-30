using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector4
{
    [ToolTipText("Multiply_Summary")]
    public partial class MultiplyFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector4.Multiply"; }
        }

        [ToolTipText("FirstArgument")]
        public System.Numerics.Vector4 A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public float B
        {
            private get;
            set;
        }

        [ToolTipText("Multiply_Value")]
        public System.Numerics.Vector4 Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A * B;
            }
        }
    }
}
