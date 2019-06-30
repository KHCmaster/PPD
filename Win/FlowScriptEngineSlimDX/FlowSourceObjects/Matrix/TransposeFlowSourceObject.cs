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
        public SharpDX.Matrix Matrix
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Transpose_Value_Summary")]
        public SharpDX.Matrix Value
        {
            get
            {
                SetValue(nameof(Matrix));
                return SharpDX.Matrix.Transpose(Matrix);
            }
        }
    }
}
