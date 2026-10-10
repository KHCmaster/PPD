using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector3
{
    [ToolTipText("Vector_TransformCoordinate_Summary")]
    public partial class TransformCoordinateFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector3.TransformCoordinate"; }
        }

        [ToolTipText("Vector_TransformCoordinate_A_Summary")]
        public System.Numerics.Vector3 A
        {
            private get;
            set;
        }

        [ToolTipText("Vector_TransformCoordinate_B_Summary")]
        public System.Numerics.Matrix4x4 B
        {
            private get;
            set;
        }

        [ToolTipText("Vector_TransformCoordinate_Value_Summary")]
        public System.Numerics.Vector3 Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return System.Numerics.Vector3.TransformCoordinate(A, B);
            }
        }
    }
}
