using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Quaternion
{
    [ToolTipText("Quaternion_Exponential_Summary")]
    public partial class ExponentialFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Quaternion.Exponential"; }
        }

        [ToolTipText("Quaternion_Exponential_Quaternion_Summary")]
        public SharpDX.Quaternion Quaternion
        {
            private get;
            set;
        }

        [ToolTipText("Quaternion_Exponential_Value_Summary")]
        public SharpDX.Quaternion Value
        {
            get
            {
                SetValue(nameof(Quaternion));
                return SharpDX.Quaternion.Exponential(Quaternion);
            }
        }
    }
}
