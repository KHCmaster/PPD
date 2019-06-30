using FlowScriptEnginePPD.Data;
using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_ProcessMissPressWithinTime_Summary")]
    public partial class ProcessMissPressWithinTimeFlowSourceObject : ProcessMissPressFlowSourceObjectBase
    {
        public override int Priority
        {
            get { return (int)ProcessMissPressPriorityType.WithinTime; }
        }

        [ToolTipText("Mark_ProcessMissPressWithinTime_StartTime")]
        public float StartTime
        {
            private get;
            set;
        }

        [ToolTipText("Mark_ProcessMissPressWithinTime_EndTime")]
        public float EndTime
        {
            private get;
            set;
        }

        public override void Process(IMarkInfo markInfo, PPDCoreModel.Data.MarkType buttonType)
        {
            SetValue(nameof(StartTime));
            SetValue(nameof(EndTime));

            if (StartTime <= markInfo.Time && markInfo.Time <= EndTime)
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
