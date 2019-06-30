using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector3
{
    [ToolTipText("Vector_Zero_Summary")]
    public partial class ZeroFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector3.Zero"; }
        }

        [ToolTipText("Vector_Zero_Value_Summary")]
        public SharpDX.Vector3 Value
        {
            get
            {
                return SharpDX.Vector3.Zero;
            }
        }
    }
}
