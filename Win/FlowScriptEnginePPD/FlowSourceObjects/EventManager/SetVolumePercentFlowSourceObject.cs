namespace FlowScriptEnginePPD.FlowSourceObjects.EventManager
{
    [ToolTipText("EventManager_SetVolumePercent_Summary")]
    public partial class SetVolumePercentFlowSourceObject : ExecutableEventManagerFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.EventManager.SetVolume"; }
        }

        [ToolTipText("EventManager_SetVolumePercent_Button")]
        public PPDCoreModel.Data.MarkType Button
        {
            private get;
            set;
        }

        [ToolTipText("EventManager_SetVolumePercent_VolumePercent")]
        public int VolumePercent
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            if (eventManager != null)
            {
                SetValue(nameof(Button));
                SetValue(nameof(VolumePercent));
                if (eventManager.SetVolumePercent(Button, VolumePercent))
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
