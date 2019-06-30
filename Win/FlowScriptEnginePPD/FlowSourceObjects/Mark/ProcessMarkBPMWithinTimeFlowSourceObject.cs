using FlowScriptEnginePPD.Data;
using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_ProcessMarkBPMWithinTime_Summary")]
    public partial class ProcessMarkBPMWithinTimeFlowSourceObject : ProcessMarkBPMFlowSourceObjectBase
    {
        public override int Priority
        {
            get { return (int)ProcessMarkBPMPriorityType.WithinTime; }
        }

        [ToolTipText("Mark_ProcessMarkBPMWithinTime_StartTime")]
        public float StartTime
        {
            private get;
            set;
        }

        [ToolTipText("Mark_ProcessMarkBPMWithinTime_EndTime")]
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
