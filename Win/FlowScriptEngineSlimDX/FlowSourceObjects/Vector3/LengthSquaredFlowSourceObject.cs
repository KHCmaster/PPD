using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector3
{
    [ToolTipText("Vector_LengthSquared_Summary")]
    public partial class LengthSquaredFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector3.LengthSquared"; }
        }

        [ToolTipText("Vector_LengthSquared_Vector_Summary")]
        public SharpDX.Vector3 Vector
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
