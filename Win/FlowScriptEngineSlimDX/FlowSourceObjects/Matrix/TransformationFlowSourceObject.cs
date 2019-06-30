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
        public SharpDX.Vector3 ScalingCenter
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Transformation_ScalingRotation_Summary")]
        public SharpDX.Quaternion ScalingRotation
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Transformation_Scaling_Summary")]
        public SharpDX.Vector3 Scaling
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Transformation_RotationCenter_Summary")]
        public SharpDX.Vector3 RotationCenter
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Transformation_Rotation_Summary")]
        public SharpDX.Quaternion Rotation
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Transformation_Translation_Summary")]
        public SharpDX.Vector3 Translation
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Transformation_Value_Summary")]
        public SharpDX.Matrix Value
        {
            get
            {
                SetValue(nameof(ScalingCenter));
                SetValue(nameof(ScalingRotation));
                SetValue(nameof(Scaling));
                SetValue(nameof(RotationCenter));
                SetValue(nameof(Rotation));
                SetValue(nameof(Translation));
                return SharpDX.Matrix.Transformation(ScalingCenter, ScalingRotation, Scaling, RotationCenter, Rotation, Translation);
            }
        }
    }
}
