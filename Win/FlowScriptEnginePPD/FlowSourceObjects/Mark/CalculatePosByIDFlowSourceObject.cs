using FlowScriptEnginePPD.Data;
using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_CalculatePosByID_Summary")]
    public partial class CalculatePosByIDFlowSourceObject : CalculatePosFlowSourceObjectBase
    {
        public override int Priority
        {
            get { return (int)CalculateMarkPriorityType.ID; }
        }

        [ToolTipText("Mark_CalculatePosByID_TargetID")]
        public int TargetID
        {
            private get;
            set;
        }

        public override void Calculate(IMarkInfo markInfo, float currentTime, float bpm)
        {
            SetValue(nameof(TargetID));

            if (markInfo.ID == TargetID)
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
