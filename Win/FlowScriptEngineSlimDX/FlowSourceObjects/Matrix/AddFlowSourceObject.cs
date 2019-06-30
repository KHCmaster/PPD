using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Add_Summary")]
    public partial class AddFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Matrix.Add"; }
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

        [ToolTipText("Add_Value")]
        public SharpDX.Matrix Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A + B;
            }
        }
    }
}
