using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Matrix_RotationX_Summary")]
    public partial class RotationXFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Matrix.RotationX"; }
        }

        [ToolTipText("Matrix_RotationX_Angle_Summary")]
        public float Angle
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_RotationX_Value_Summary")]
        public System.Numerics.Matrix4x4 Value
        {
            get
            {
                SetValue(nameof(Angle));
                return System.Numerics.Matrix4x4.RotationX(Angle);
            }
        }
    }
}
