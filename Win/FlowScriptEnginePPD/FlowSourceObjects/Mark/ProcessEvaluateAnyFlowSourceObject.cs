using FlowScriptEnginePPD.Data;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_ProcessEvaluateAny_Summary")]
    public partial class ProcessEvaluateAnyFlowSourceObject : ProcessEvaluateFlowSourceObjectBase
    {
        public override int Priority
        {
            get { return (int)ProcessMarkEvaluatePriorityType.Any; }
        }
    }
}
