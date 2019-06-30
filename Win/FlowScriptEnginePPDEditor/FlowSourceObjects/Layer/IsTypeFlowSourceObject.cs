using PPDEditorCommon;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Layer
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<ILayer>
    {
        public override string Name
        {
            get { return "PPDEditor.Layer.IsType"; }
        }
    }
}
