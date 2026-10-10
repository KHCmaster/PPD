using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Matrix_Transformation_Summary")]
    public partial class TransformationFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Matrix.Transformation"; }
        }

        [ToolTipText("Matrix_Transformation_ScalingCenter_Summary")]
        public System.Numerics.Vector3 ScalingCenter
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Transformation_ScalingRotation_Summary")]
        public System.Numerics.Quaternion ScalingRotation
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Transformation_Scaling_Summary")]
        public System.Numerics.Vector3 Scaling
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Transformation_RotationCenter_Summary")]
        public System.Numerics.Vector3 RotationCenter
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Transformation_Rotation_Summary")]
        public System.Numerics.Quaternion Rotation
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Transformation_Translation_Summary")]
        public System.Numerics.Vector3 Translation
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Transformation_Value_Summary")]
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
                return System.Numerics.Matrix4x4.Transformation(ScalingCenter, ScalingRotation, Scaling, RotationCenter, Rotation, Translation);
            }
        }
    }
}
