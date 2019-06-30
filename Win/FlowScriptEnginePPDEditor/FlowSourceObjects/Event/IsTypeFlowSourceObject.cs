using PPDEditorCommon;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Event
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<EventData>
    {
        public override string Name
        {
            get { return "PPDEditor.Event.IsType"; }
        }
    }
}
