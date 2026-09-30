using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Matrix_Scaling_Summary")]
    public partial class ScalingFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Matrix.Scaling"; }
        }

        [ToolTipText("Matrix_Scaling_Scale_Summary")]
        public System.Numerics.Vector3 Scale
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Scaling_Value_Summary")]
        public System.Numerics.Matrix4x4 Value
        {
            get
            {
                SetValue(nameof(Scale));
                return System.Numerics.Matrix4x4.Scaling(Scale);
            }
        }
    }
}
