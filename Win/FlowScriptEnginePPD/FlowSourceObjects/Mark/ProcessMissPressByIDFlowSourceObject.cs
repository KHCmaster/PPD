using FlowScriptEnginePPD.Data;
using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_ProcessMissPressByID_Summary")]
    public partial class ProcessMissPressByIDFlowSourceObject : ProcessMissPressFlowSourceObjectBase
    {
        public override int Priority
        {
            get { return (int)ProcessMissPressPriorityType.ID; }
        }

        [ToolTipText("Mark_ProcessMissPressByID_TargetID")]
        public int TargetID
        {
            private get;
            set;
        }

        public override void Process(IMarkInfo markInfo, PPDCoreModel.Data.MarkType buttonType)
        {
            SetValue(nameof(TargetID));

            if (markInfo.ID == TargetID)
            {
                base.Process(markInfo, buttonType);
            }
            else
            {
                EvaluateHandled = false;
            }
        }
    }
}
