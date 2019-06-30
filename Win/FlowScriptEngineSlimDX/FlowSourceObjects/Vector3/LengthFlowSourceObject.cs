using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector3
{
    [ToolTipText("Vector_Length_Summary")]
    public partial class LengthFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector3.Length"; }
        }

        [ToolTipText("Vector_Length_Vector_Summary")]
        public SharpDX.Vector3 Vector
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
