using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Matrix_RotationY_Summary")]
    public partial class RotationYFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Matrix.RotationY"; }
        }

        [ToolTipText("Matrix_RotationY_Angle_Summary")]
        public float Angle
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_RotationY_Value_Summary")]
        public System.Numerics.Matrix4x4 Value
        {
            get
            {
                SetValue(nameof(Angle));
                return System.Numerics.Matrix4x4.RotationY(Angle);
            }
        }
    }
}
