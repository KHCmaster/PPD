using FlowScriptEngine;
using PPDFramework.ScreenFilters;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ScreenFilter.Gaussian
{
    [ToolTipText("ScreenFilter_Gaussian_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ScreenFilter.Gaussian.Value"; }
        }

        [ToolTipText("ScreenFilter_Gaussian_Value_Disperson")]
        public float Disperson
        {
            private get;
            set;
        }

        [ToolTipText("ScreenFilter_Gaussian_Value_Filter")]
        public GaussianFilter Filter
        {
            get;
            private set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Disperson));
            Filter = new GaussianFilter
            {
                Disperson = Disperson
            };
            OnSuccess();
        }
    }
}
