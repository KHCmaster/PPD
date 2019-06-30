using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.MedianGrayScale
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<MedianGrayScaleColorFilter>
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.MedianGrayScale.IsType"; }
        }
    }
}
