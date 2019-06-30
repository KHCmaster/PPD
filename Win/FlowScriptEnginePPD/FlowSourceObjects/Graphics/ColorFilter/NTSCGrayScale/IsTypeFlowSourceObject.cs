using PPDFramework.Shaders;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.ColorFilter.NTSCGrayScale
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<NTSCGrayScaleColorFilter>
    {
        public override string Name
        {
            get { return "PPD.Graphics.ColorFilter.NTSCGrayScale.IsType"; }
        }
    }
}
