namespace PPDCoreModel
{
    public interface IEvaluate : IPriority
    {
        bool IsEvaluateRequired();
        bool EvaluateHandled { get; }
    }
}
