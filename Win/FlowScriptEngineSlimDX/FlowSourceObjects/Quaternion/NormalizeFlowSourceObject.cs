using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Quaternion
{
    [ToolTipText("Quaternion_Normalize_Summary")]
    public partial class NormalizeFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Quaternion.Normalize"; }
        }

        [ToolTipText("Quaternion_Normalize_Quaternion_Summary")]
        public SharpDX.Quaternion Quaternion
        {
            private get;
            set;
        }

        [ToolTipText("Quaternion_Normalize_Value_Summary")]
        public SharpDX.Quaternion Value
        {
            get
            {
                SetValue(nameof(Quaternion));
                return SharpDX.Quaternion.Normalize(Quaternion);
            }
        }
    }
}
