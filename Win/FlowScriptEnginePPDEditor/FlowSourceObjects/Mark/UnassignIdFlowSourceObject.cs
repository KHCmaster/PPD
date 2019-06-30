using PPDEditorCommon;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_UnassignId_Summary")]
    public partial class UnassignIdFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPDEditor.Mark.UnassignID"; }
        }

        [ToolTipText("Mark_UnassignId_Mark")]
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
                Mark.Layer.ChangeMarkPropertyManager.UnassignID(Mark);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
