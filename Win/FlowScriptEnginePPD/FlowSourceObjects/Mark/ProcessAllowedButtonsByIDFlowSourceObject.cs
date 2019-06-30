using FlowScriptEnginePPD.Data;
using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_ProcessAllowedButtonsByID_Summary")]
    public partial class ProcessAllowedButtonsByIDFlowSourceObject : ProcessAllowedButtonsFlowSourceObjectBase
    {
        public override int Priority
        {
            get { return (int)ProcessAllowedButtonsPriorityType.ID; }
        }

        [ToolTipText("Mark_ProcessAllowedButtonsByID_TargetID")]
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
