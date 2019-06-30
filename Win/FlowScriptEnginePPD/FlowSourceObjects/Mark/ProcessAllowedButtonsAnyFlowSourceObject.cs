using FlowScriptEnginePPD.Data;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_ProcessAllowedButtonsAny_Summary")]
    public partial class ProcessAllowedButtonsAnyFlowSourceObject : ProcessAllowedButtonsFlowSourceObjectBase
    {
        public override int Priority
        {
            get { return (int)ProcessAllowedButtonsPriorityType.Any; }
        }
    }
}
