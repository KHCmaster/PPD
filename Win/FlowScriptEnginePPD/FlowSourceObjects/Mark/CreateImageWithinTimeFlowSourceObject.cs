using FlowScriptEnginePPD.Data;
using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_CreateImageWithinTime_Summary")]
    public partial class CreateImageWithinTimeFlowSourceObject : CreateImageFlowSourceObjectBase
    {
        public override int Priority
        {
            get { return (int)CreateMarkImagePriorityType.WithinTime; }
        }

        [ToolTipText("Mark_CreateImageWithinTime_StartTime")]
        public float StartTime
        {
            private get;
            set;
        }

        [ToolTipText("Mark_CreateImageWithinTime_EndTime")]
        public float EndTime
        {
            private get;
            set;
        }

        public override void Create(IMarkInfo markInfo)
        {
            SetValue(nameof(StartTime));
            SetValue(nameof(EndTime));

            if (StartTime <= markInfo.Time && markInfo.Time <= EndTime)
            {
                base.Create(markInfo);
            }
            else
            {
                EvaluateHandled = false;
            }
        }
    }
}
