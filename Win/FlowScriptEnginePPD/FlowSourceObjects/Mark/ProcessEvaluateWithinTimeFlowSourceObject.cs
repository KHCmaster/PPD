using FlowScriptEnginePPD.Data;
using PPDCoreModel;
using SharpDX;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_ProcessEvaluateWithinTime_Summary")]
    public partial class ProcessEvaluateWithinTimeFlowSourceObject : ProcessEvaluateFlowSourceObjectBase
    {
        public override int Priority
        {
            get { return (int)ProcessMarkEvaluatePriorityType.WithinTime; }
        }

        [ToolTipText("Mark_ProcessEvaluateWithinTime_StartTime")]
        public float StartTime
        {
            private get;
            set;
        }

        [ToolTipText("Mark_ProcessEvaluateWithinTime_EndTime")]
        public float EndTime
        {
            private get;
            set;
        }

        public override void ProcessEvaluate(IMarkInfo markInfo, PPDCoreModel.Data.EffectType effectType, bool missPress, bool release, Vector2 position)
        {
            SetValue(nameof(StartTime));
            SetValue(nameof(EndTime));

            if (StartTime <= markInfo.Time && markInfo.Time <= EndTime)
            {
                base.ProcessEvaluate(markInfo, effectType, missPress, release, position);
            }
            else
            {
                EvaluateHandled = false;
            }
        }
    }
}
