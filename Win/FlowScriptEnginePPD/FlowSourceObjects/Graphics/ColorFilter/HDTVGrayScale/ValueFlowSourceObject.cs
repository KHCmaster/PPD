using FlowScriptEngine;
using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.HDTVGrayScale
{
    [ToolTipText("ColorFilter_HDTVGrayScale_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.HDTVGrayScale.Value"; }
        }

        [ToolTipText("ColorFilter_HDTVGrayScale_Value_Filter")]
        public HDTVGrayScaleColorFilter Filter
        {
            get;
            private set;
        }

        [ToolTipText("ColorFilter_HDTVGrayScale_Value_Weight")]
        public float Weight
        {
            private get;
            set;
        }

        [ToolTipText("ColorFilter_HDTVGrayScale_Value_GammaCorrection")]
        public float GammaCorrection
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Weight));
            SetValue(nameof(GammaCorrection));
            Filter = new HDTVGrayScaleColorFilter
            {
                Weight = Weight,
                GammaCorrection = GammaCorrection
            };
            OnSuccess();
        }
    }
}
