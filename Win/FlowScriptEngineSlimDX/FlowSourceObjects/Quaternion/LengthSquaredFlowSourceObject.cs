using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Quaternion
{
    [ToolTipText("Quaternion_LengthSquared_Summary")]
    public partial class LengthSquaredFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Quaternion.LengthSquared"; }
        }

        [ToolTipText("Quaternion_LengthSquared_Quaternion_Summary")]
        public SharpDX.Quaternion Quaternion
        {
            private get;
            set;
        }

        [ToolTipText("Quaternion_LengthSquared_Value_Summary")]
        public float Value
        {
            get
            {
                SetValue(nameof(Quaternion));
                return Quaternion.LengthSquared();
            }
        }
    }
}
