using PPDMath;
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
        public System.Numerics.Vector4 Value
        {
            get
            {
                return System.Numerics.Vector4.UnitY;
            }
        }
    }
}
