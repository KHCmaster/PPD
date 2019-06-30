using FlowScriptEngine;
using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.AverageGrayScale
{
    [ToolTipText("ColorFilter_AverageGrayScale_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.AverageGrayScale.Value"; }
        }

        [ToolTipText("ColorFilter_AverageGrayScale_Value_Filter")]
        public AverageGrayScaleColorFilter Filter
        {
            get;
            private set;
        }

        [ToolTipText("ColorFilter_AverageGrayScale_Value_Weight")]
        public float Weight
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Weight));
            Filter = new AverageGrayScaleColorFilter
            {
                Weight = Weight
            };
            OnSuccess();
        }
    }
}
