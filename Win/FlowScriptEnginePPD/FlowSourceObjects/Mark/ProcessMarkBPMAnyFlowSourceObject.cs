using FlowScriptEnginePPD.Data;
using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_ProcessMarkBPMAny_Summary")]
    public partial class ProcessMarkBPMAnyFlowSourceObject : ProcessMarkBPMFlowSourceObjectBase
    {
        public override int Priority
        {
            get { return (int)ProcessMarkBPMPriorityType.Any; }
        }

        public override void Process(IMarkInfo markInfo)
        {
            base.Process(markInfo);
        }
    }
}
