using FlowScriptEnginePPD.Data;
using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_CreateImageAny_Summary")]
    public partial class CreateImageAnyFlowSourceObject : CreateImageFlowSourceObjectBase
    {
        public override int Priority
        {
            get { return (int)CreateMarkImagePriorityType.Any; }
        }

        public override void Create(IMarkInfo markInfo)
        {
            base.Create(markInfo);
        }
    }
}
