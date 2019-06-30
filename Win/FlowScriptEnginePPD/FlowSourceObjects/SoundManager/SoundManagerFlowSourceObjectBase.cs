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
            if (Manager.Items.ContainsKey("SoundManager"))
            {
                soundManager = (ISoundManager)Manager.Items["SoundManager"];
            }
            if (Manager.Items.ContainsKey("PlayRatio"))
            {
                playRatio = (double)Manager.Items["PlayRatio"];
            }
        }
    }
}
