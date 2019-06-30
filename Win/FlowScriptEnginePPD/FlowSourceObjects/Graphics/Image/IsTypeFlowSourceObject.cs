using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Image
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<PictureObject>
    {
        public override string Name
        {
            get { return "PPD.Graphics.Image.IsType"; }
        }
    }
}
