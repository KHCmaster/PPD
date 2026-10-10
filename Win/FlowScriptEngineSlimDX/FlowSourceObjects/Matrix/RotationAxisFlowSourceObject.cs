using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Matrix_RotationAxis_Summary")]
    public partial class RotationAxisFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Matrix.RotationAxis"; }
        }

        [ToolTipText("Matrix_RotationAxis_Axis_Summary")]
        public System.Numerics.Vector3 Axis
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_RotationAxis_Angle_Summary")]
        public float Angle
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_RotationAxis_Value_Summary")]
        public System.Numerics.Matrix4x4 Value
        {
            get
            {
                SetValue(nameof(Axis));
                SetValue(nameof(Angle));
                return System.Numerics.Matrix4x4.RotationAxis(Axis, Angle);
            }
        }
    }
}
