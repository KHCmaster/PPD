using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector3
{
    [ToolTipText("Vector_TransformNormal_Summary")]
    public partial class TransformNormalFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector3.TransformNormal"; }
        }

        [ToolTipText("Vector_TransformNormal_A_Summary")]
        public SharpDX.Vector3 A
        {
            private get;
            set;
        }

        [ToolTipText("Vector_TransformNormal_B_Summary")]
        public SharpDX.Matrix B
        {
            private get;
            set;
        }

        [ToolTipText("Vector_TransformNormal_Value_Summary")]
        public SharpDX.Vector3 Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return SharpDX.Vector3.TransformNormal(A, B);
            }
        }
    }
}
