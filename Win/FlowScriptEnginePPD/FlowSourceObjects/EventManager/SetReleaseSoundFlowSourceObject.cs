namespace FlowScriptEnginePPD.FlowSourceObjects.EventManager
{
    [ToolTipText("EventManager_SetReleaseSound_Summary")]
    public partial class SetReleaseSoundFlowSourceObject : ExecutableEventManagerFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.EventManager.SetReleaseSound"; }
        }

        [ToolTipText("EventManager_SetReleaseSound_Button")]
        public PPDCoreModel.Data.MarkType Button
        {
            private get;
            set;
        }

        [ToolTipText("EventManager_SetReleaseSound_ReleaseSound")]
        public bool ReleaseSound
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            if (eventManager != null)
            {
                SetValue(nameof(Button));
                SetValue(nameof(ReleaseSound));
                if (eventManager.SetReleaseSound(Button, ReleaseSound))
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