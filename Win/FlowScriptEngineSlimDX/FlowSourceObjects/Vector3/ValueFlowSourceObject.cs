using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector3
{
    [ToolTipText("Vector_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        private SharpDX.Vector3 value;

        public override string Name
        {
            get { return "Vector3.Value"; }
        }

        [ToolTipText("Vector3_Value_Value_Summary")]
        public SharpDX.Vector3 Value
        {
            get
            {
                SetValue(nameof(Value));
                return value;
            }
            set { this.value = value; }
        }

        [ToolTipText("Vector_Value_X_Summary")]
        public float X
        {
            get
            {
                SetValue(nameof(Value));
                return Value.X;
            }
        }

        [ToolTipText("Vector_Value_Y_Summary")]
        public float Y
        {
            get
            {
                SetValue(nameof(Value));
                return Value.Y;
            }
        }

        [ToolTipText("Vector_Value_Z_Summary")]
        public float Z
        {
            get
            {
                SetValue(nameof(Value));
                return Value.Z;
            }
        }
    }
}
