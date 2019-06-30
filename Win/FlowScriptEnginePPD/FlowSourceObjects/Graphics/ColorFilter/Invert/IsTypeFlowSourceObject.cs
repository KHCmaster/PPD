using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.Invert
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<InvertColorFilter>
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.Invert.IsType"; }
        }
    }
}
