using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Color4
{
    [ToolTipText("Color4_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        private SharpDX.Color4 color;

        public override string Name
        {
            get { return "Color4.Value"; }
        }

        [ToolTipText("Color4_Value_Summary")]
        public SharpDX.Color4 Value
        {
            get
            {
                SetValue(nameof(Value));
                return color;
            }
            set { color = value; }
        }

        [ToolTipText("Color4_Value_Alpha")]
        public float Alpha
        {
            get
            {
                SetValue(nameof(Value));
                return Value.Alpha;
            }
        }

        [ToolTipText("Color4_Value_Red")]
        public float Red
        {
            get
            {
                SetValue(nameof(Value));
                return Value.Red;
            }
        }

        [ToolTipText("Color4_Value_Green")]
        public float Green
        {
            get
            {
                SetValue(nameof(Value));
                return Value.Green;
            }
        }

        [ToolTipText("Color4_Value_Blue")]
        public float Blue
        {
            get
            {
                SetValue(nameof(Value));
                return Value.Blue;
            }
        }
    }
}
