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
        public SharpDX.Quaternion A
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
        public SharpDX.Quaternion Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return new SharpDX.Quaternion(
                    A.X / B,
                    A.Y / B,
                    A.Z / B,
                    A.W / B
                    );
            }
        }
    }
}
