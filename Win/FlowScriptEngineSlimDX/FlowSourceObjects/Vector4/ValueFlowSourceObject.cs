using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector4
{
    [ToolTipText("Vector_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        private SharpDX.Vector4 value;

        public override string Name
        {
            get { return "Vector4.Value"; }
        }

        [ToolTipText("Vector4_Value_Value_Summary")]
        public SharpDX.Vector4 Value
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

        [ToolTipText("Vector_Value_W_Summary")]
        public float W
        {
            get
            {
                SetValue(nameof(Value));
                return Value.W;
            }
        }
    }
}
