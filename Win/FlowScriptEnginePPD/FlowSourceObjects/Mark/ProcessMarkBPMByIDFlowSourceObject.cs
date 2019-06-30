using FlowScriptEnginePPD.Data;
using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_ProcessMarkBPMByID_Summary")]
    public partial class ProcessMarkBPMByIDFlowSourceObject : ProcessMarkBPMFlowSourceObjectBase
    {
        public override int Priority
        {
            get { return (int)ProcessMarkBPMPriorityType.ID; }
        }

        [ToolTipText("Mark_ProcessMarkBPMByID_TargetID")]
        public int TargetID
        {
            private get;
            set;
        }

        public override void Process(IMarkInfo markInfo)
        {
            SetValue(nameof(TargetID));

            if (markInfo.ID == TargetID)
            {
                base.Process(markInfo);
            }
            else
            {
                EvaluateHandled = false;
            }
        }
    }
}
