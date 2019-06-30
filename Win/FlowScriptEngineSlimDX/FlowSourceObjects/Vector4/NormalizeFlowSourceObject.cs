using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector4
{
    [ToolTipText("Vector_Normalize_Summary")]
    public partial class NormalizeFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector4.Normalize"; }
        }

        [ToolTipText("Vector_Normalize_Vector_Summary")]
        public SharpDX.Vector4 Vector
        {
            private get;
            set;
        }

        [ToolTipText("Vector_Normalize_Value_Summary")]
        public SharpDX.Vector4 Value
        {
            get
            {
                SetValue(nameof(Vector));
                return SharpDX.Vector4.Normalize(Vector);
            }
        }
    }
}
