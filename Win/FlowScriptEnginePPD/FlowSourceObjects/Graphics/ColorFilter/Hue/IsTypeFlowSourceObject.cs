using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.Hue
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<HueColorFilter>
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.Hue.IsType"; }
        }
    }
}
