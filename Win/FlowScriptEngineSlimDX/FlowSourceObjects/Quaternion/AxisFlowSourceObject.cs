using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Quaternion
{
    [ToolTipText("Quaternion_Axis_Summary")]
    public partial class AxisFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Quaternion.Axis"; }
        }

        [ToolTipText("Quaternion_Axis_Quaternion_Summary")]
        public SharpDX.Quaternion Quaternion
        {
            private get;
            set;
        }

        [ToolTipText("Quaternion_Axis_Value_Summary")]
        public SharpDX.Vector3 Value
        {
            get
            {
                SetValue(nameof(Quaternion));
                return Quaternion.Axis;
            }
        }
    }
}
