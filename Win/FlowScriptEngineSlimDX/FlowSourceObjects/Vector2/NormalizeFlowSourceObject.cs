using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector2
{
    [ToolTipText("Vector_Normalize_Summary")]
    public partial class NormalizeFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector2.Normalize"; }
        }

        [ToolTipText("Vector_Normalize_Vector_Summary")]
        public SharpDX.Vector2 Vector
        {
            private get;
            set;
        }

        [ToolTipText("Vector_Normalize_Value_Summary")]
        public SharpDX.Vector2 Value
        {
            get
            {
                SetValue(nameof(Vector));
                return SharpDX.Vector2.Normalize(Vector);
            }
        }
    }
}
