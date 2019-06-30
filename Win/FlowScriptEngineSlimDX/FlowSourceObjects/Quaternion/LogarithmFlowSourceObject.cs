using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Quaternion
{
    [ToolTipText("Quaternion_Logarithm_Summary")]
    public partial class LogarithmFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Quaternion.Logarithm"; }
        }

        [ToolTipText("Quaternion_Logarithm_Quaternion_Summary")]
        public SharpDX.Quaternion Quaternion
        {
            private get;
            set;
        }

        [ToolTipText("Quaternion_Logarithm_Value_Summary")]
        public SharpDX.Quaternion Value
        {
            get
            {
                SetValue(nameof(Quaternion));
                return SharpDX.Quaternion.Logarithm(Quaternion);
            }
        }
    }
}
