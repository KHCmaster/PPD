using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.SoundManager
{
    [ToolTipText("SoundManager_Play_Summary")]
    public partial class PlayByButtonWithVolumeFlowSourceObject : SoundManagerFlowSourceObjectBase
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

        [ToolTipText("SoundManager_Play_Volume")]
        public int Volume
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            if (soundManager != null)
            {
                SetValue(nameof(Button));
                SetValue(nameof(Volume));
                Volume = Volume >= SoundMasterControl.Max ? SoundMasterControl.Max : (Volume < SoundMasterControl.Min ? SoundMasterControl.Min : Volume);
                // 元は Play(Button, Volume) で、音量が再生速度の Play(MarkType, double) に渡り、音量は使われていなかった
                if (soundManager.Play(Button, Volume, playRatio))
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
