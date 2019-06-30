using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector4
{
    [ToolTipText("Vector_Transform_Summary")]
    public partial class TransformFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector4.Transform"; }
        }

        [ToolTipText("Vector_Transform_A_Summary")]
        public SharpDX.Vector4 A
        {
            private get;
            set;
        }

        [ToolTipText("Vector_Transform_B_Summary")]
        public SharpDX.Matrix B
        {
            private get;
            set;
        }

        [ToolTipText("Vector_Transform_Value_Summary")]
        public SharpDX.Vector4 Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return SharpDX.Vector4.Transform(A, B);
            }
        }
    }
}
