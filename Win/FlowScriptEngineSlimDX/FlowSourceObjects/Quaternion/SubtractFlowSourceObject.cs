using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Quaternion
{
    [ToolTipText("Substract_Summary")]
    public partial class SubtractFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Quaternion.Subtract"; }
        }

        [ToolTipText("FirstArgument")]
        public System.Numerics.Quaternion A
        {
            private get;
            set;
        }

        [ToolTipText("SecondArgument")]
        public System.Numerics.Quaternion B
        {
            private get;
            set;
        }

        [ToolTipText("Substract_Value")]
        public System.Numerics.Quaternion Value
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
