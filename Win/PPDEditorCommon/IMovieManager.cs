using PPDFramework;

namespace PPDEditorCommon
{
    public interface IMovieManager
    {
        void Play();
        void Stop();
        void Pause();
        void Seek(double time);
        void SetTrimming(MovieTrimmingData trimming, bool applyToIniFileWriter);
        MovieTrimmingData MovieTrimmingData { get; }
    }
}
