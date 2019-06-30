using PPDEditorCommon;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_Remove_Summary")]
    public partial class RemoveFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPDEditor.Mark.Remove"; }
        }

        [ToolTipText("Mark_Remove_Mark")]
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
                Mark.Layer.ChangeMarkPropertyManager.Remove(Mark);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
