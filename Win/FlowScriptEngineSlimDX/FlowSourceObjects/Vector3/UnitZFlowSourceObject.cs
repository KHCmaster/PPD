using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector3
{
    [ToolTipText("Vector_UnitZ_Summary")]
    public partial class UnitZFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector3.UnitZ"; }
        }

        [ToolTipText("Vector_UnitZ_Value_Summary")]
        public SharpDX.Vector3 Value
        {
            get
            {
                return SharpDX.Vector3.UnitZ;
            }
        }
    }
}
