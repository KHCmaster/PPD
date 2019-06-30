using FlowScriptEngine;
using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.Hue
{
    [ToolTipText("ColorFilter_Hue_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.Hue.Value"; }
        }

        [ToolTipText("ColorFilter_Hue_Value_Filter")]
        public HueColorFilter Filter
        {
            get;
            private set;
        }

        [ToolTipText("ColorFilter_Hue_Value_Weight")]
        public float Weight
        {
            private get;
            set;
        }

        [ToolTipText("ColorFilter_Hue_Value_Rotation")]
        public float Rotation
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Weight));
            SetValue(nameof(Rotation));
            Filter = new HueColorFilter
            {
                Weight = Weight,
                Rotation = Rotation
            };
            OnSuccess();
        }
    }
}
