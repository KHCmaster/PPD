using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.AverageGrayScale
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<AverageGrayScaleColorFilter>
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.AverageGrayScale.IsType"; }
        }
    }
}
