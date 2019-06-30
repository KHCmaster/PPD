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
        public SharpDX.Vector3 Scale
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Scaling_Value_Summary")]
        public SharpDX.Matrix Value
        {
            get
            {
                SetValue(nameof(Scale));
                return SharpDX.Matrix.Scaling(Scale);
            }
        }
    }
}
