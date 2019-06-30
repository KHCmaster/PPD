using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Substract_Summary")]
    public partial class SubtractFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Matrix.Subtract"; }
        }

        [ToolTipText("FirstArgument")]
        public SharpDX.Matrix A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public SharpDX.Matrix B
        {
            private get;
            set;
        }

        [ToolTipText("Substract_Value")]
        public SharpDX.Matrix Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A - B;
            }
        }
    }
}
