using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector4
{
    [ToolTipText("Vector_Length_Summary")]
    public partial class LengthFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector4.Length"; }
        }

        [ToolTipText("Vector_Length_Vector_Summary")]
        public SharpDX.Vector4 Vector
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
