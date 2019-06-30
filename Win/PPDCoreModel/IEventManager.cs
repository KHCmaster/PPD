using PPDCoreModel.Data;

namespace PPDCoreModel
{
    public interface IEventManager
    {
        bool SetVolumePercent(MarkType button, int volPercent);
        bool SetKeepPlaying(MarkType button, bool keepPlaying);
        bool SetReleaseSound(MarkType button, bool releaseSound);
        int[] VolumePercents { get; }
        bool[] KeepPlayings { get; }
        bool[] ReleaseSounds { get; }
    }
}
