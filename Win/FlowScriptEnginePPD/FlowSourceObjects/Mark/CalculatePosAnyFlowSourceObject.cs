using FlowScriptEnginePPD.Data;
using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_CalculatePosAny_Summary")]
    public partial class CalculatePosAnyFlowSourceObject : CalculatePosFlowSourceObjectBase
    {
        public override int Priority
        {
            get { return (int)CalculateMarkPriorityType.Any; }
        }

        public override void Calculate(IMarkInfo markInfo, float currentTime, float bpm)
        {
            base.Calculate(markInfo, currentTime, bpm);
        }
    }
}
