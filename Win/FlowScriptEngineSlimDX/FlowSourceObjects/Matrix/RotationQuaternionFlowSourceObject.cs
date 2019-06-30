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
        public SharpDX.Quaternion Quaternion
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_RotationQuaternion_Value_Summary")]
        public SharpDX.Matrix Value
        {
            get
            {
                SetValue(nameof(Quaternion));
                return SharpDX.Matrix.RotationQuaternion(Quaternion);
            }
        }
    }
}
