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
                return Index switch
                {
                    0 => A.Column1,
                    1 => A.Column2,
                    2 => A.Column3,
                    3 => A.Column4,
                    _ => System.Numerics.Vector4.Zero,
                };
            }
        }
    }
}
