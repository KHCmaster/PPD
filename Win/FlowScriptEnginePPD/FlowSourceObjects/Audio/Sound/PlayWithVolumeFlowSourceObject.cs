using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Audio.Sound
{
    [ToolTipText("Sound_PlayWithVolume_Summary")]
    public partial class PlayWithVolumeFlowSourceObject : SoundFlowSourceObjectBase
    {
        double playRatio = 1;

        public override string Name
        {
            get { return "PPD.Audio.Sound.PlayWithVolume"; }
        }

        [ToolTipText("Sound_PlayWithVolume_Volume")]
        public int Volume
        {
            private get;
            set;
        }

        protected override void OnInitialize()
        {
            if (Manager.Items.ContainsKey("PlayRatio"))
            {
                playRatio = (double)Manager.Items["PlayRatio"];
            }
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Object));
            SetValue(nameof(Volume));
            if (Object != null)
            {
                Volume = Volume >= SoundMasterControl.Max ? SoundMasterControl.Max : (Volume < SoundMasterControl.Min ? SoundMasterControl.Min : Volume);
                Object.Play(Volume, playRatio);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
