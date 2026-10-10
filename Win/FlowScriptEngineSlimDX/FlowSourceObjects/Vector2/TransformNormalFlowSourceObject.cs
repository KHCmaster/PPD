using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector2
{
    [ToolTipText("Vector_TransformNormal_Summary")]
    public partial class TransformNormalFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector2.TransformNormal"; }
        }

        [ToolTipText("Vector_TransformNormal_A_Summary")]
        public System.Numerics.Vector2 A
        {
            private get;
            set;
        }

        [ToolTipText("Vector_TransformNormal_B_Summary")]
        public System.Numerics.Matrix4x4 B
        {
            private get;
            set;
        }

        [ToolTipText("Vector_TransformNormal_Value_Summary")]
        public System.Numerics.Vector2 Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return System.Numerics.Vector2.TransformNormal(A, B);
            }
        }
    }
}
