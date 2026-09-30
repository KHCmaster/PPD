using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector4
{
    [ToolTipText("Substract_Summary")]
    public partial class SubtractFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector4.Subtract"; }
        }

        [ToolTipText("FirstArgument")]
        public System.Numerics.Vector4 A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public System.Numerics.Vector4 B
        {
            private get;
            set;
        }

        [ToolTipText("Substract_Value")]
        public System.Numerics.Vector4 Value
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
