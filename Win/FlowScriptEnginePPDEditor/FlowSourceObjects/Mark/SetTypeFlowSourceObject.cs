using PPDCoreModel.Data;
using PPDEditorCommon;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_SetType_Summary")]
    public partial class SetTypeFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPDEditor.Mark.SetType"; }
        }

        [ToolTipText("Mark_SetType_Mark")]
        public IEditorMarkInfo Mark
        {
            private get;
            set;
        }

        [ToolTipText("Mark_SetType_Type")]
        public MarkType Type
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Mark));
            if (Mark != null)
            {
                SetValue(nameof(Type));
                Mark.Layer.ChangeMarkPropertyManager.ChangeMarkType(Mark, Type);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
