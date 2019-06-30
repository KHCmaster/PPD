using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Vector2
{
    [ToolTipText("Vector_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        private SharpDX.Vector2 value;

        public override string Name
        {
            get { return "Vector2.Value"; }
        }

        [ToolTipText("Vector2_Value_Value_Summary")]
        public SharpDX.Vector2 Value
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
    }
}
