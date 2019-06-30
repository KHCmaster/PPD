using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.MaxGrayScale
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<MaxGrayScaleColorFilter>
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.MaxGrayScale.IsType"; }
        }
    }
}
