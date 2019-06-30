using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector3
{
    [ToolTipText("Vector_UnitY_Summary")]
    public partial class UnitYFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector3.UnitY"; }
        }

        [ToolTipText("Vector_UnitY_Value_Summary")]
        public SharpDX.Vector3 Value
        {
            get
            {
                return SharpDX.Vector3.UnitY;
            }
        }
    }
}
