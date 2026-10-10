using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector4
{
    [ToolTipText("Vector_UnitW_Summary")]
    public partial class UnitWFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector4.UnitW"; }
        }

        [ToolTipText("Vector_UnitW_Value_Summary")]
        public System.Numerics.Vector4 Value
        {
            get
            {
                return System.Numerics.Vector4.UnitW;
            }
        }
    }
}
