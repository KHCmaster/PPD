using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector2
{
    [ToolTipText("Vector_Length_Summary")]
    public partial class LengthFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector2.Length"; }
        }

        [ToolTipText("Vector_Length_Vector_Summary")]
        public SharpDX.Vector2 Vector
        {
            private get;
            set;
        }

        [ToolTipText("Vector_Length_Value_Summary")]
        public float Value
        {
            get
            {
                SetValue(nameof(Vector));
                return Vector.Length();
            }
        }
    }
}
