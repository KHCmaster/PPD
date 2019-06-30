using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Matrix_GetColumn_Summary")]
    public partial class GetColumnFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Matrix.GetColumn"; }
        }

        [ToolTipText("Matrix_GetColumn_A_Summary")]
        public SharpDX.Matrix A
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_GetColumn_Index_Summary")]
        public int Index
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_GetColumn_Value_Summary")]
        public SharpDX.Vector4 Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(Index));
                switch (Index)
                {
                    case 0:
                        return A.Column1;
                    case 1:
                        return A.Column2;
                    case 2:
                        return A.Column3;
                    case 3:
                        return A.Column4;
                }
                return SharpDX.Vector4.Zero;
            }
        }
    }
}
