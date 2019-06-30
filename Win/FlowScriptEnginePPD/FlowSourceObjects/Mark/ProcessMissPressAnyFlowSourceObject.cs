using FlowScriptEnginePPD.Data;
using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_ProcessMissPressAny_Summary")]
    public partial class ProcessMissPressAnyFlowSourceObject : ProcessMissPressFlowSourceObjectBase
    {
        public override int Priority
        {
            get { return (int)ProcessMissPressPriorityType.Any; }
        }

        public override void Process(IMarkInfo markInfo, PPDCoreModel.Data.MarkType buttonType)
        {
            base.Process(markInfo, buttonType);
        }
    }
}
