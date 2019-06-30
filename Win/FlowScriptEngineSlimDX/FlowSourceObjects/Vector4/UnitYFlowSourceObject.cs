using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector4
{
    [ToolTipText("Vector_UnitY_Summary")]
    public partial class UnitYFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector4.UnitY"; }
        }

        [ToolTipText("Vector_UnitY_Value_Summary")]
        public SharpDX.Vector4 Value
        {
            get
            {
                return SharpDX.Vector4.UnitY;
            }
        }
    }
}
