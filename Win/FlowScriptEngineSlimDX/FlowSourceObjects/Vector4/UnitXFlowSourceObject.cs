using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector4
{
    [ToolTipText("Vector_UnitX_Summary")]
    public partial class UnitXFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector4.UnitX"; }
        }

        [ToolTipText("Vector_UnitX_Value_Summary")]
        public SharpDX.Vector4 Value
        {
            get
            {
                return SharpDX.Vector4.UnitX;
            }
        }
    }
}
