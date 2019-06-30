using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector2
{
    [ToolTipText("Vector_LengthSquared_Summary")]
    public partial class LengthSquaredFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector2.LengthSquared"; }
        }

        [ToolTipText("Vector_LengthSquared_Vector_Summary")]
        public SharpDX.Vector2 Vector
        {
            private get;
            set;
        }

        [ToolTipText("Vector_LengthSquared_Value_Summary")]
        public float Value
        {
            get
            {
                SetValue(nameof(Vector));
                return Vector.LengthSquared();
            }
        }
    }
}
