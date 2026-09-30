using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Devide_Summary")]
    public partial class DevideFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Matrix.Devide"; }
        }

        [ToolTipText("FirstArgument")]
        public System.Numerics.Matrix4x4 A
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

        [ToolTipText("Devide_Value")]
        public System.Numerics.Matrix4x4 Value
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
