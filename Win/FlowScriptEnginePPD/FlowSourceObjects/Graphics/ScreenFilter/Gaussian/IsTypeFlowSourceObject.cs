using PPDFramework.ScreenFilters;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ScreenFilter.Gaussian
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<GaussianFilter>
    {
        public override string Name
        {
            get { return "PPD.Graphics.ScreenFilter.Gaussian.IsType"; }
        }
    }
}
