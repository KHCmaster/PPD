using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector3
{
    [ToolTipText("Vector_Normalize_Summary")]
    public partial class NormalizeFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector3.Normalize"; }
        }

        [ToolTipText("Vector_Normalize_Vector_Summary")]
        public SharpDX.Vector3 Vector
        {
            private get;
            set;
        }

        [ToolTipText("Vector_Normalize_Value_Summary")]
        public SharpDX.Vector3 Value
        {
            get
            {
                SetValue(nameof(Vector));
                return SharpDX.Vector3.Normalize(Vector);
            }
        }
    }
}
