using PPDMath;
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
        public System.Numerics.Matrix4x4 A
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
        public System.Numerics.Vector4 Value
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
                return System.Numerics.Vector4.Zero;
            }
        }
    }
}
