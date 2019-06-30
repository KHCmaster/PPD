using FlowScriptEnginePPD.Data;
using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_CalculatePosWithinTime_Summary")]
    public partial class CalculatePosWithinTimeFlowSourceObject : CalculatePosFlowSourceObjectBase
    {
        [ToolTipText("Mark_CalculatePosWithinTime_StartTime")]
        public float StartTime
        {
            private get;
            set;
        }

        [ToolTipText("Mark_CalculatePosWithinTime_EndTime")]
        public float EndTime
        {
            private get;
            set;
        }

        public override int Priority
        {
            get { return (int)CalculateMarkPriorityType.WithinTime; }
        }

        public override void Calculate(IMarkInfo markInfo, float currentTime, float bpm)
        {
            SetValue(nameof(StartTime));
            SetValue(nameof(EndTime));

            if (StartTime <= markInfo.Time && markInfo.Time <= EndTime)
            {
                base.Calculate(markInfo, currentTime, bpm);
            }
            else
            {
                EvaluateHandled = false;
            }
        }
    }
}
