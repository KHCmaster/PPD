using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector4
{
    [ToolTipText("Vector_LengthSquared_Summary")]
    public partial class LengthSquaredFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector4.LengthSquared"; }
        }

        [ToolTipText("Vector_LengthSquared_Vector_Summary")]
        public SharpDX.Vector4 Vector
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
