using PPDMath;
using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Quaternion
{
    [ToolTipText("Quaternion_RotationAxis_Summary")]
    public partial class RotationAxisFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Quaternion.RotationAxis"; }
        }

        [ToolTipText("Quaternion_RotationAxis_Axis_Summary")]
        public System.Numerics.Vector3 Axis
        {
            private get;
            set;
        }

        [ToolTipText("Quaternion_RotationAxis_Angle_Summary")]
        public float Angle
        {
            private get;
            set;
        }

        [ToolTipText("Quaternion_RotationAxis_Value_Summary")]
        public System.Numerics.Quaternion Value
        {
            get
            {
                SetValue(nameof(Axis));
                SetValue(nameof(Angle));
                return System.Numerics.Quaternion.RotationAxis(Axis, Angle);
            }
        }
    }
}
