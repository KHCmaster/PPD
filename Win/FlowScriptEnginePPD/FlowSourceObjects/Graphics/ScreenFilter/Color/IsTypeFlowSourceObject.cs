using PPDFramework.ScreenFilters;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ScreenFilter.Color
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<ColorScreenFilter>
    {
        public override string Name
        {
            get { return "PPD.Graphics.ScreenFilter.Color.IsType"; }
        }
    }
}
