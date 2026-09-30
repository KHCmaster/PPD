using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Matrix_RotationQuaternion_Summary")]
    public partial class RotationQuaternionFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Matrix.RotationQuaternion"; }
        }

        [ToolTipText("Matrix_RotationQuaternion_Quaternion_Summary")]
        public System.Numerics.Quaternion Quaternion
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_RotationQuaternion_Value_Summary")]
        public System.Numerics.Matrix4x4 Value
        {
            get
            {
                SetValue(nameof(Quaternion));
                return System.Numerics.Matrix4x4.RotationQuaternion(Quaternion);
            }
        }
    }
}
