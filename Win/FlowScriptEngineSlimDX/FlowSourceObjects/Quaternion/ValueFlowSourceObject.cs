using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Quaternion
{
    [ToolTipText("Quaternion_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        private SharpDX.Quaternion value;
        public override string Name
        {
            get { return "Quaternion.Value"; }
        }

        [ToolTipText("Quaternion_Value_Value_Summary")]
        public SharpDX.Quaternion Value
        {
            get
            {
                SetValue(nameof(Value));
                return value;
            }
            set { this.value = value; }
        }

        [ToolTipText("Quaternion_Value_X_Summary")]
        public float X
        {
            get
            {
                SetValue(nameof(Value));
                return value.X;
            }
        }

        [ToolTipText("Quaternion_Value_Y_Summary")]
        public float Y
        {
            get
            {
                SetValue(nameof(Value));
                return value.Y;
            }
        }

        [ToolTipText("Quaternion_Value_Z_Summary")]
        public float Z
        {
            get
            {
                SetValue(nameof(Value));
                return value.Z;
            }
        }

        [ToolTipText("Quaternion_Value_W_Summary")]
        public float W
        {
            get
            {
                SetValue(nameof(Value));
                return value.W;
            }
        }
    }
}
