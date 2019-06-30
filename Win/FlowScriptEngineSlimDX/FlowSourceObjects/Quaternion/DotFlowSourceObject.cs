using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Quaternion
{
    [ToolTipText("Quaternion_Dot_Summary")]
    public partial class DotFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Quaternion.Dot"; }
        }

        [ToolTipText("Quaternion_Dot_A_Summary")]
        public SharpDX.Quaternion A
        {
            private get;
            set;
        }

        [ToolTipText("Quaternion_Dot_B_Summary")]
        public SharpDX.Quaternion B
        {
            private get;
            set;
        }

        [ToolTipText("Quaternion_Dot_Value_Summary")]
        public float Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(B));
                return SharpDX.Quaternion.Dot(A, B);
            }
        }
    }
}
