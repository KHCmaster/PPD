using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Quaternion
{
    [ToolTipText("Quaternion_Angle_Summary")]
    public partial class AngleFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Quaternion.Angle"; }
        }

        [ToolTipText("Quaternion_Angle_Quaternion_Summary")]
        public SharpDX.Quaternion Quaternion
        {
            private get;
            set;
        }

        [ToolTipText("Quaternion_Angle_Value_Summary")]
        public float Value
        {
            get
            {
                SetValue(nameof(Quaternion));
                return Quaternion.Angle;
            }
        }
    }
}
