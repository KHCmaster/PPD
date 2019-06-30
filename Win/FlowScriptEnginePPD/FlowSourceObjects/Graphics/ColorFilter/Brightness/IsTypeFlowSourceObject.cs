using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.Brightness
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<BrightnessColorFilter>
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.Brightness.IsType"; }
        }
    }
}
