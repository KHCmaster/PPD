using FlowScriptEngine;
using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.Saturation
{
    [ToolTipText("ColorFilter_Saturation_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.Saturation.Value"; }
        }

        [ToolTipText("ColorFilter_Saturation_Value_Filter")]
        public SaturationColorFilter Filter
        {
            get;
            private set;
        }

        [ToolTipText("ColorFilter_Saturation_Value_Weight")]
        public float Weight
        {
            private get;
            set;
        }

        [ToolTipText("ColorFilter_Saturation_Value_Scale")]
        public float Scale
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Weight));
            SetValue(nameof(Scale));
            Filter = new SaturationColorFilter
            {
                Weight = Weight,
                Scale = Scale
            };
            OnSuccess();
        }
    }
}
