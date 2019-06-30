using PPDSound;

namespace FlowScriptEnginePPD.FlowSourceObjects.Audio.Sound
{
    public partial class IsTypeFlowSourceObject : TemplateIsTypeFlowSourceObject<SoundResource>
    {
        public override string Name
        {
            get { return "PPD.Audio.Sound.IsType"; }
        }
    }
}
