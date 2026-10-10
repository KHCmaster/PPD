using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Quaternion
{
    [ToolTipText("Quaternion_RotationMatrix_Summary")]
    public partial class RotationMatrixFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Quaternion.RotationMatrix"; }
        }

        [ToolTipText("Quaternion_RotationMatrix_Matrix_Summary")]
        public System.Numerics.Matrix4x4 Matrix
        {
            private get;
            set;
        }

        [ToolTipText("Quaternion_RotationMatrix_Value_Summary")]
        public System.Numerics.Quaternion Value
        {
            get
            {
                SetValue(nameof(Matrix));
                return System.Numerics.Quaternion.RotationMatrix(Matrix);
            }
        }
    }
}
