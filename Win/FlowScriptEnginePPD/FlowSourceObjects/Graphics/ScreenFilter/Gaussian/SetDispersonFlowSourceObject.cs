using FlowScriptEngine;
using PPDFramework.ScreenFilters;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ScreenFilter.Gaussian
{
    [ToolTipText("ScreenFilter_Gaussian_SetDisperson_Summary")]
    public partial class SetDispersonFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Graphics.ScreenFilter.Gaussian.SetDisperson"; }
        }

        [ToolTipText("ScreenFilter_Gaussian_SetDisperson_Filter")]
        public GaussianFilter Filter
        {
            private get;
            set;
        }

        [ToolTipText("ScreenFilter_Gaussian_SetDisperson_Disperson")]
        public float Disperson
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Filter));
            if (Filter != null)
            {
                SetValue(nameof(Disperson));
                Filter.Disperson = Disperson;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
