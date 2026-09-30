using PPDMath;
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
        public System.Numerics.Matrix4x4 A
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
        public System.Numerics.Vector4 Value
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
                return System.Numerics.Vector4.Zero;
            }
        }
    }
}
