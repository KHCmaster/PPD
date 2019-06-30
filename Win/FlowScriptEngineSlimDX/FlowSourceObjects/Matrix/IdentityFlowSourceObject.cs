using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Matrix_Identity_Summary")]
    public partial class IdentityFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Matrix.Identity"; }
        }

        [ToolTipText("Matrix_Identity_Value_Summary")]
        public SharpDX.Matrix Value
        {
            get
            {
                return SharpDX.Matrix.Identity;
            }
        }
    }
}
