using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector2
{
    [ToolTipText("Vector_Zero_Summary")]
    public partial class ZeroFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector2.Zero"; }
        }

        [ToolTipText("Vector_Zero_Value_Summary")]
        public SharpDX.Vector2 Value
        {
            get
            {
                return SharpDX.Vector2.Zero;
            }
        }
    }
}
