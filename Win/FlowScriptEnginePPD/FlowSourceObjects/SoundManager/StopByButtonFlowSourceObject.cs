using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects.SoundManager
{
    [ToolTipText("SoundManager_Stop_Summary")]
    public partial class StopByButtonFlowSourceObject : SoundManagerFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.SoundManager.Stop"; }
        }

        [ToolTipText("SoundManager_Stop_Button")]
        public PPDCoreModel.Data.MarkType Button
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            if (soundManager != null)
            {
                SetValue(nameof(Button));
                if (soundManager.Stop(Button))
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
