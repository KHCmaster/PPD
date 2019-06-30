using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Matrix_AffineTransformation2D_Summary")]
    public partial class AffineTransformation2DFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Matrix.AffineTransformation2D"; }
        }

        [ToolTipText("Matrix_AffineTransformation2D_Scale_Summary")]
        public float Scale
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_AffineTransformation2D_RotationCenter_Summary")]
        public SharpDX.Vector2 RotationCenter
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_AffineTransformation2D_Rotation_Summary")]
        public float Rotation
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_AffineTransformation2D_Translation_Summary")]
        public SharpDX.Vector2 Translation
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_AffineTransformation2D_Value_Summary")]
        public SharpDX.Matrix Value
        {
            get
            {
                SetValue(nameof(Scale));
                SetValue(nameof(RotationCenter));
                SetValue(nameof(Rotation));
                SetValue(nameof(Translation));
                return SharpDX.Matrix.AffineTransformation2D(Scale, RotationCenter, Rotation, Translation);
            }
        }
    }
}
