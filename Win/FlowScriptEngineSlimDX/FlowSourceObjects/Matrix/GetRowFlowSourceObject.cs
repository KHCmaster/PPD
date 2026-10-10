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
                return Index switch
                {
                    0 => A.Row1,
                    1 => A.Row2,
                    2 => A.Row3,
                    3 => A.Row4,
                    _ => System.Numerics.Vector4.Zero,
                };
            }
        }
    }
}
