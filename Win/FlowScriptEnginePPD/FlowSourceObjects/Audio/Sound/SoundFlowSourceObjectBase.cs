using PPDSound;

namespace FlowScriptEnginePPD.FlowSourceObjects.Audio.Sound
{
    public abstract class SoundFlowSourceObjectBase : ExecutableFlowSourceObject
    {
        [ToolTipText("Sound_Object")]
        public SoundResource Object
        {
            protected get;
            set;
        }
    }
}
