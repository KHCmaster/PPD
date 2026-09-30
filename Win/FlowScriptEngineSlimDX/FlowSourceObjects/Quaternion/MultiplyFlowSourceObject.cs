using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Quaternion
{
    [ToolTipText("Multiply_Summary")]
    public partial class MultiplyFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Quaternion.Multiply"; }
        }

        [ToolTipText("FirstArgument")]
        public System.Numerics.Quaternion A
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
        public System.Numerics.Quaternion Value
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
