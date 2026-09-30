using PPDMath;
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
        public System.Numerics.Matrix4x4 Value
        {
            get
            {
                return System.Numerics.Matrix4x4.Identity;
            }
        }
    }
}
