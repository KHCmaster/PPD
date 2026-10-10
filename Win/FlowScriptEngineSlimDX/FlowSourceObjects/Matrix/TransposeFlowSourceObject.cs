using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Matrix_Transpose_Summary")]
    public partial class TransposeFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Matrix.Transpose"; }
        }

        [ToolTipText("Matrix_Transpose_Matrix_Summary")]
        public System.Numerics.Matrix4x4 Matrix
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Transpose_Value_Summary")]
        public System.Numerics.Matrix4x4 Value
        {
            get
            {
                SetValue(nameof(Matrix));
                return System.Numerics.Matrix4x4.Transpose(Matrix);
            }
        }
    }
}
