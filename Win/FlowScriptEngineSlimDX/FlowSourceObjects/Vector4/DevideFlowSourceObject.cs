using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector4
{
    [ToolTipText("Devide_Summary")]
    public partial class DevideFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Vector4.Devide"; }
        }

        [ToolTipText("FirstArgument")]
        public System.Numerics.Vector4 A
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

        [ToolTipText("Devide_Value")]
        public System.Numerics.Vector4 Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return A / B;
            }
        }
    }
}
