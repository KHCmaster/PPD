using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector4
{
    [ToolTipText("Equal_Summary")]
    public partial class EqualFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector4.Equal"; }
        }

        [ToolTipText("FirstArgument")]
        public SharpDX.Vector4 A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public SharpDX.Vector4 B
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
