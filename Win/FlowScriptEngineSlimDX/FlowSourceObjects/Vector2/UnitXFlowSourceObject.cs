using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector2
{
    [ToolTipText("Vector_UnitX_Summary")]
    public partial class UnitXFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector2.UnitX"; }
        }

        [ToolTipText("Vector_UnitX_Value_Summary")]
        public System.Numerics.Vector2 Value
        {
            get
            {
                return System.Numerics.Vector2.UnitX;
            }
        }
    }
}
