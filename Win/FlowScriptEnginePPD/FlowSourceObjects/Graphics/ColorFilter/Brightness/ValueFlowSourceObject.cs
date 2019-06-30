using FlowScriptEngine;
using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.Brightness
{
    [ToolTipText("ColorFilter_Brightness_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.Brightness.Value"; }
        }

        [ToolTipText("ColorFilter_Brightness_Value_Filter")]
        public BrightnessColorFilter Filter
        {
            get;
            private set;
        }

        [ToolTipText("ColorFilter_Brightness_Value_Weight")]
        public float Weight
        {
            private get;
            set;
        }

        [ToolTipText("ColorFilter_Brightness_Value_Scale")]
        public float Scale
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Weight));
            SetValue(nameof(Scale));
            Filter = new BrightnessColorFilter
            {
                Weight = Weight,
                Scale = Scale
            };
            OnSuccess();
        }
    }
}
