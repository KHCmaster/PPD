using FlowScriptEngine;
using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.GreenGrayScale
{
    [ToolTipText("ColorFilter_GreenGrayScale_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.GreenGrayScale.Value"; }
        }

        [ToolTipText("ColorFilter_GreenGrayScale_Value_Filter")]
        public GreenGrayScaleColorFilter Filter
        {
            get;
            private set;
        }

        [ToolTipText("ColorFilter_GreenGrayScale_Value_Weight")]
        public float Weight
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Weight));
            Filter = new GreenGrayScaleColorFilter
            {
                Weight = Weight
            };
            OnSuccess();
        }
    }
}
