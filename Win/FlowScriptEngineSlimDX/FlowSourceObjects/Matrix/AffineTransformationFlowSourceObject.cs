using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Matrix_AffineTransformation_Summary")]
    public partial class AffineTransformationFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Matrix.AffineTransformation"; }
        }

        [ToolTipText("Matrix_AffineTransformation_Scale_Summary")]
        public float Scale
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_AffineTransformation_RotationCenter_Summary")]
        public SharpDX.Vector3 RotationCenter
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_AffineTransformation_Rotation_Summary")]
        public SharpDX.Quaternion Rotation
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_AffineTransformation_Translation_Summary")]
        public SharpDX.Vector3 Translation
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_AffineTransformation_Value_Summary")]
        public SharpDX.Matrix Value
        {
            get
            {
                SetValue(nameof(Scale));
                SetValue(nameof(RotationCenter));
                SetValue(nameof(Rotation));
                SetValue(nameof(Translation));
                return SharpDX.Matrix.AffineTransformation(Scale, RotationCenter, Rotation, Translation);
            }
        }
    }
}
