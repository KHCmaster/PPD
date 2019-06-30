using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector4
{
    [ToolTipText("Vector_Zero_Summary")]
    public partial class ZeroFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector4.Zero"; }
        }

        [ToolTipText("Vector_Zero_Value_Summary")]
        public SharpDX.Vector4 Value
        {
            get
            {
                return SharpDX.Vector4.Zero;
            }
        }
    }
}
