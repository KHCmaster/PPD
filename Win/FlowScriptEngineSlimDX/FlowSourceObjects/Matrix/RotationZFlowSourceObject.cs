using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Matrix_RotationZ_Summary")]
    public partial class RotationZFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Matrix.RotationZ"; }
        }

        [ToolTipText("Matrix_RotationZ_Angle_Summary")]
        public float Angle
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_RotationZ_Value_Summary")]
        public SharpDX.Matrix Value
        {
            get
            {
                SetValue(nameof(Angle));
                return SharpDX.Matrix.RotationZ(Angle);
            }
        }
    }
}
