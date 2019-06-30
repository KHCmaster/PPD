namespace FlowScriptEnginePPD.FlowSourceObjects.EventManager
{
    [ToolTipText("EventManager_SetKeepPlaying_Summary")]
    public partial class SetKeepPlayingFlowSourceObject : ExecutableEventManagerFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.EventManager.SetKeepPlaying"; }
        }

        [ToolTipText("EventManager_SetKeepPlaying_Button")]
        public PPDCoreModel.Data.MarkType Button
        {
            private get;
            set;
        }

        [ToolTipText("EventManager_SetKeepPlaying_KeepPlaying")]
        public bool KeepPlaying
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            if (eventManager != null)
            {
                SetValue(nameof(Button));
                SetValue(nameof(KeepPlaying));
                if (eventManager.SetKeepPlaying(Button, KeepPlaying))
                {
                    OnSuccess();
                }
                else
                {
                    OnFailed();
                }
            }
            else
            {
                OnFailed();
            }
        }
    }
}
