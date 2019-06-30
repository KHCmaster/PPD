using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector2
{
    [ToolTipText("Vector_TransformCoordinate_Summary")]
    public partial class TransformCoordinateFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector2.TransformCoordinate"; }
        }

        [ToolTipText("Vector_TransformCoordinate_A_Summary")]
        public SharpDX.Vector2 A
        {
            private get;
            set;
        }

        [ToolTipText("Vector_TransformCoordinate_B_Summary")]
        public SharpDX.Matrix B
        {
            private get;
            set;
        }

        [ToolTipText("Vector_TransformCoordinate_Value_Summary")]
        public SharpDX.Vector2 Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return SharpDX.Vector2.TransformCoordinate(A, B);
            }
        }
    }
}
