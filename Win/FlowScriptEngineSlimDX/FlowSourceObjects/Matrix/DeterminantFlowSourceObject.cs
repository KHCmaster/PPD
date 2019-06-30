using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Matrix_Determinant_Summary")]
    public partial class DeterminantFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Matrix.Determinant"; }
        }

        [ToolTipText("Matrix_Determinant_Matrix_Summary")]
        public SharpDX.Matrix Matrix
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_Determinant_Value_Summary")]
        public float Value
        {
            get
            {
                SetValue(nameof(Matrix));
                return Matrix.Determinant();
            }
        }
    }
}
