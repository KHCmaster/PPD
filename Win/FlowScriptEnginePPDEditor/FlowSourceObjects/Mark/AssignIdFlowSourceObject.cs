using PPDEditorCommon;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_AssignId_Summary")]
    public partial class AssignIdFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPDEditor.Mark.AssignID"; }
        }

        [ToolTipText("Mark_AssignId_Mark")]
        public IEditorMarkInfo Mark
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Mark));
            if (Mark != null)
            {
                Mark.Layer.ChangeMarkPropertyManager.AssignID(Mark);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
