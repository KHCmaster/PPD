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
        public SharpDX.Matrix Matrix
        {
            private get;
            set;
        }

        [ToolTipText("Quaternion_RotationMatrix_Value_Summary")]
        public SharpDX.Quaternion Value
        {
            get
            {
                SetValue(nameof(Matrix));
                return SharpDX.Quaternion.RotationMatrix(Matrix);
            }
        }
    }
}
