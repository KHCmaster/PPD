using SharpDX;

namespace PPDCoreModel
{
    public interface ICalculatePosition : IEvaluate
    {
        void Calculate(IMarkInfo markInfo, float currentTime, float bpm);
        float BPM { get; }
        Vector2 CalculatePosition { get; }
    }
}
