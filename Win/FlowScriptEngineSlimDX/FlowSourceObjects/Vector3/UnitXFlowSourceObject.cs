using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector3
{
    [ToolTipText("Vector_UnitX_Summary")]
    public partial class UnitXFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector3.UnitX"; }
        }

        [ToolTipText("Vector_UnitX_Value_Summary")]
        public SharpDX.Vector3 Value
        {
            get
            {
                return SharpDX.Vector3.UnitX;
            }
        }
    }
}
