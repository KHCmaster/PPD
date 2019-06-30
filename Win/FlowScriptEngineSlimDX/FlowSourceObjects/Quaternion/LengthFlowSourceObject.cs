using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Quaternion
{
    [ToolTipText("Quaternion_Length_Summary")]
    public partial class LengthFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Quaternion.Length"; }
        }

        [ToolTipText("Quaternion_Length_Quaternion_Summary")]
        public SharpDX.Quaternion Quaternion
        {
            private get;
            set;
        }

        [ToolTipText("Quaternion_Length_Value_Summary")]
        public float Value
        {
            get
            {
                SetValue(nameof(Quaternion));
                return Quaternion.Length();
            }
        }
    }
}
