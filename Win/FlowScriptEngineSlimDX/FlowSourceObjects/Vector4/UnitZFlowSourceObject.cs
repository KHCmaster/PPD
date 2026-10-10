using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector4
{
    [ToolTipText("Vector_UnitZ_Summary")]
    public partial class UnitZFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector4.UnitZ"; }
        }

        [ToolTipText("Vector_UnitZ_Value_Summary")]
        public System.Numerics.Vector4 Value
        {
            get
            {
                return System.Numerics.Vector4.UnitZ;
            }
        }
    }
}
