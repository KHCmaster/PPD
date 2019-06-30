using FlowScriptEngine;

namespace FlowScriptEngineSlimDX.FlowSourceObjects.Color4
{
    [ToolTipText("Color4_FromElement_Summary")]
    public partial class FromElementFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Color4.FromElement"; }
        }

        [ToolTipText("Color4_FromElement_Alpha")]
        public float Alpha
        {
            private get;
            set;
        }

        [ToolTipText("Color4_FromElement_Red")]
        public float Red
        {
            private get;
            set;
        }

        [ToolTipText("Color4_FromElement_Green")]
        public float Green
        {
            private get;
            set;
        }

        [ToolTipText("Color4_FromElement_Blue")]
        public float Blue
        {
            private get;
            set;
        }

        [ToolTipText("Color4_FromElement_Value")]
        public SharpDX.Color4 Value
        {
            get
            {
                SetValue(nameof(Alpha));
                SetValue(nameof(Red));
                SetValue(nameof(Green));
                SetValue(nameof(Blue));
                return new SharpDX.Color4(Red, Green, Blue, Alpha);
            }
        }
    }
}
