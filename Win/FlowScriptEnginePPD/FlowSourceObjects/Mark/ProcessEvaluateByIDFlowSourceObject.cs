using FlowScriptEnginePPD.Data;
using PPDCoreModel;
using SharpDX;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_ProcessEvaluateByID_Summary")]
    public partial class ProcessEvaluateByIDFlowSourceObject : ProcessEvaluateFlowSourceObjectBase
    {
        public override int Priority
        {
            get { return (int)ProcessMarkEvaluatePriorityType.ID; }
        }

        [ToolTipText("Mark_ProcessEvaluateByID_TargetID")]
        public int TargetID
        {
            private get;
            set;
        }

        public override void ProcessEvaluate(IMarkInfo markInfo, PPDCoreModel.Data.EffectType effectType, bool missPress, bool release, Vector2 position)
        {
            SetValue(nameof(TargetID));

            if (markInfo.ID == TargetID)
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
