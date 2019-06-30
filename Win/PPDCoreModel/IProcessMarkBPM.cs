namespace PPDCoreModel
{
    public interface IProcessMarkBPM : IEvaluate
    {
        void Process(IMarkInfo markInfo);
        float ProcessBPM { get; }
    }
}
