using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector3
{
    [ToolTipText("Vector_Transform_Summary")]
    public partial class TransformFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector3.Transform"; }
        }

        [ToolTipText("Vector_Transform_A_Summary")]
        public System.Numerics.Vector3 A
        {
            private get;
            set;
        }

        [ToolTipText("Vector_Transform_B_Summary")]
        public System.Numerics.Matrix4x4 B
        {
            private get;
            set;
        }

        [ToolTipText("Vector_Transform_Value_Summary")]
        public System.Numerics.Vector4 Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return MathCompat.TransformToVector4(A, B);
            }
        }
    }
}
