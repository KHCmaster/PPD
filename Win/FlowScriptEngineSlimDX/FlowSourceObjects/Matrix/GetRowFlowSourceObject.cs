using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Matrix_GetRow_Summary")]
    public partial class GetRowFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Matrix.GetRow"; }
        }

        [ToolTipText("Matrix_GetRow_A_Summary")]
        public SharpDX.Matrix A
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_GetRow_Index_Summary")]
        public int Index
        {
            private get;
            set;
        }

        [ToolTipText("Matrix_GetRow_Value_Summary")]
        public SharpDX.Vector4 Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(Index));
                switch (Index)
                {
                    case 0:
                        return A.Row1;
                    case 1:
                        return A.Row2;
                    case 2:
                        return A.Row3;
                    case 3:
                        return A.Row4;
                }
                return SharpDX.Vector4.Zero;
            }
        }
    }
}
