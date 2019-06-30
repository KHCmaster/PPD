using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Matrix
{
    [ToolTipText("Equal_Summary")]
    public partial class EqualFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Matrix.Equal"; }
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

        [ToolTipText("Equal_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A == B;
            }
        }
    }
}
