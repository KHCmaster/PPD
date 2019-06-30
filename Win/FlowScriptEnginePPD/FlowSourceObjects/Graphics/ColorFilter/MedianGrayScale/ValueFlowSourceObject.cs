using FlowScriptEngine;
using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.MedianGrayScale
{
    [ToolTipText("ColorFilter_MedianGrayScale_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.MedianGrayScale.Value"; }
        }

        [ToolTipText("ColorFilter_MedianGrayScale_Value_Filter")]
        public MedianGrayScaleColorFilter Filter
        {
            get;
            private set;
        }

        [ToolTipText("ColorFilter_MedianGrayScale_Value_Weight")]
        public float Weight
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Weight));
            Filter = new MedianGrayScaleColorFilter
            {
                Weight = Weight
            };
            OnSuccess();
        }
    }
}
