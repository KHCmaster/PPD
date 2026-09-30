using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Quaternion
{
    [ToolTipText("Devide_Summary")]
    public partial class DevideFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Quaternion.Devide"; }
        }

        [ToolTipText("FirstArgument")]
        public System.Numerics.Quaternion A
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
        public System.Numerics.Quaternion Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return new System.Numerics.Quaternion(
                    A.X / B,
                    A.Y / B,
                    A.Z / B,
                    A.W / B
                    );
            }
        }
    }
}
