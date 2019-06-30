using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector2
{
    [ToolTipText("Vector_UnitY_Summary")]
    public partial class UnitYFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector2.UnitY"; }
        }

        [ToolTipText("Vector_UnitY_Value_Summary")]
        public SharpDX.Vector2 Value
        {
            get
            {
                return SharpDX.Vector2.UnitY;
            }
        }
    }
}
