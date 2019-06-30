using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects.SoundManager
{
    [ToolTipText("SoundManager_Play_Summary")]
    public partial class PlayByButtonFlowSourceObject : SoundManagerFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.SoundManager.Play"; }
        }

        [ToolTipText("SoundManager_Play_Button")]
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
                if (soundManager.Play(Button, playRatio))
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
