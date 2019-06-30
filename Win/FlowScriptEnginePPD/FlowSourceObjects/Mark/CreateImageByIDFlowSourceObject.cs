using FlowScriptEnginePPD.Data;
using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_CreateImageByID_Summary")]
    public partial class CreateImageByIDFlowSourceObject : CreateImageFlowSourceObjectBase
    {
        public override int Priority
        {
            get { return (int)CreateMarkImagePriorityType.ID; }
        }

        [ToolTipText("Mark_CreateImageByID_TargetID")]
        public int TargetID
        {
            private get;
            set;
        }

        public override void Create(IMarkInfo markInfo)
        {
            SetValue(nameof(TargetID));

            if (markInfo.ID == TargetID)
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
