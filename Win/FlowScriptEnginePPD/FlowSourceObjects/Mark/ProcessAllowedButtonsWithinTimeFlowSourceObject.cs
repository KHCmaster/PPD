using FlowScriptEnginePPD.Data;
using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_ProcessAllowedButtonsWithinTime_Summary")]
    public partial class ProcessAllowedButtonsWithinTimeFlowSourceObject : ProcessAllowedButtonsFlowSourceObjectBase
    {
        public override int Priority
        {
            get { return (int)ProcessAllowedButtonsPriorityType.WithinTime; }
        }

        [ToolTipText("Mark_ProcessAllowedButtonsWithinTime_StartTime")]
        public float StartTime
        {
            private get;
            set;
        }

        [ToolTipText("Mark_ProcessAllowedButtonsWithinTime_EndTime")]
        public float EndTime
        {
            private get;
            set;
        }

        public override void Process(IMarkInfo markInfo)
        {
            SetValue(nameof(StartTime));
            SetValue(nameof(EndTime));

            if (StartTime <= markInfo.Time && markInfo.Time <= EndTime)
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
