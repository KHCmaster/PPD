using SharpDX;
using System.Collections.Generic;

namespace PPDCoreModel
{
    public interface IMarkInfo
    {
        Vector2 ColorPosition { get; set; }
        Vector2 Position { get; }
        float Angle { get; }
        float Time { get; }
        uint ID { get; }
        Data.MarkType Type { get; }
        bool IsLong { get; }
        bool IsAC { get; }
        bool HasSameTimingMark { get; }
        float ReleaseTime { get; }
        bool IsACFT { get; }
        bool IsScratch { get; }
        bool IsRight { get; }
        Dictionary<object, object> Parameters { get; }
        float SlideScale { get; }
        int SameTimingMarks { get; }
    }
}
