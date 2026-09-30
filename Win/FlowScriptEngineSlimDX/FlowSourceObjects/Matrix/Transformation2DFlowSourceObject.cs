using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Matrix_Transformation2D_Summary")]
    public partial class Transformation2DFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Matrix.Transformation2D"; }
        }

        [ToolTipText("Matrix_Transformation2D_ScalingCenter_Summary")]
        public System.Numerics.Vector2 ScalingCenter
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Transformation2D_ScalingRotation_Summary")]
        public float ScalingRotation
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Transformation2D_Scaling_Summary")]
        public System.Numerics.Vector2 Scaling
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Transformation2D_RotationCenter_Summary")]
        public System.Numerics.Vector2 RotationCenter
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Transformation2D_Rotation_Summary")]
        public float Rotation
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Transformation2D_Translation_Summary")]
        public System.Numerics.Vector2 Translation
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Transformation2D_Value_Summary")]
        public System.Numerics.Matrix4x4 Value
        {
            get
            {
                SetValue(nameof(ScalingCenter));
                SetValue(nameof(ScalingRotation));
                SetValue(nameof(Scaling));
                SetValue(nameof(RotationCenter));
                SetValue(nameof(Rotation));
                SetValue(nameof(Translation));
                return System.Numerics.Matrix4x4.Transformation2D(ScalingCenter, ScalingRotation, Scaling, RotationCenter, Rotation, Translation);
            }
        }
    }
}
