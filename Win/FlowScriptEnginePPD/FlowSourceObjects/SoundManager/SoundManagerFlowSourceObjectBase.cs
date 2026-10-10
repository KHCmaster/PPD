using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects.SoundManager
{
    public abstract class SoundManagerFlowSourceObjectBase : ExecutableFlowSourceObject
    {
        protected double playRatio;
        protected ISoundManager soundManager;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (Manager.Items.TryGetValue("SoundManager", out object soundManagerValue))
            {
                soundManager = (ISoundManager)soundManagerValue;
            }
            if (Manager.Items.TryGetValue("PlayRatio", out object playRatioValue))
            {
                playRatio = (double)playRatioValue;
            }
        }
    }
}
