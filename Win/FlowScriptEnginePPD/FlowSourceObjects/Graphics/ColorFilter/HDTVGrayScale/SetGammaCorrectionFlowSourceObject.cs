using FlowScriptEngine;
using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.HDTVGrayScale
{
    [ToolTipText("ColorFilter_HDTVGrayScale_SetGammaCorrection_Summary")]
    public partial class SetGammaCorrectionFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.HDTVGrayScale.SetGammaCorrection"; }
        }

        [ToolTipText("ColorFilter_HDTVGrayScale_SetGammaCorrection_Filter")]
        public HDTVGrayScaleColorFilter Filter
        {
            private get;
            set;
        }

        [ToolTipText("ColorFilter_HDTVGrayScale_SetGammaCorrection_GammaCorrection")]
        public float GammaCorrection
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Filter));
            if (Filter != null)
            {
                SetValue(nameof(GammaCorrection));
                Filter.GammaCorrection = GammaCorrection;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
