using FlowScriptEngine;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.SoundManager
{
    [ToolTipText("SoundManager_Play_Summary")]
    public partial class PlayFlowSourceObject : SoundManagerFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.SoundManager.Play"; }
        }

        [ToolTipText("SoundManager_Play_Index")]
        public int Index
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
                SetValue(nameof(Index));
                SetValue(nameof(Volume));
                Volume = Volume >= SoundMasterControl.Max ? SoundMasterControl.Max : (Volume < SoundMasterControl.Min ? SoundMasterControl.Min : Volume);
                if (soundManager.Play(Index, Volume, playRatio))
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
