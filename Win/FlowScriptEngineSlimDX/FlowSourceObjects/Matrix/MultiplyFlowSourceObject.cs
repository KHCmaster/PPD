using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Multiply_Summary")]
    public partial class MultiplyFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Matrix.Multiply"; }
        }

        [ToolTipText("FirstArgument")]
        public SharpDX.Matrix A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public float B
        {
            private get;
            set;
        }

        [ToolTipText("Multiply_Value")]
        public SharpDX.Matrix Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A * B;
            }
        }
    }
}
