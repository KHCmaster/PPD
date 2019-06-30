using PPDEditorCommon;

namespace FlowScriptEnginePPDEditor.FlowSourceObjects.Mark
{
    [ToolTipText("Mark_RemoveParameter_Summary")]
    public partial class RemoveParameterFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPDEditor.Mark.RemoveParameter"; }
        }

        [ToolTipText("Mark_RemoveParameter_Mark")]
        public IEditorMarkInfo Mark
        {
            private get;
            set;
        }

        [ToolTipText("Mark_RemoveParameter_Key")]
        public string Key
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Mark));
            if (Mark != null)
            {
                SetValue(nameof(Key));
                Mark.Layer.ChangeMarkPropertyManager.RemoveParameter(Mark, Key);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
