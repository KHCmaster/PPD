using FlowScriptEngine;
using SharpDX;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.Color
{
    [ToolTipText("ColorFilter_Color_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.Color.Value"; }
        }

        [ToolTipText("ColorFilter_Color_Value_Filter")]
        public PPDFramework.Shaders.ColorFilter Filter
        {
            get;
            private set;
        }

        [ToolTipText("ColorFilter_Color_Value_Weight")]
        public float Weight
        {
            private get;
            set;
        }

        [ToolTipText("ColorFilter_Color_Value_Color")]
        public Color4 Color
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Weight));
            SetValue(nameof(Color));
            Filter = new PPDFramework.Shaders.ColorFilter
            {
                Weight = Weight,
                Color = Color
            };
            OnSuccess();
        }
    }
}
