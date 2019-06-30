using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.Saturation
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<SaturationColorFilter>
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.Saturation.IsType"; }
        }
    }
}
