using PPDFramework;

namespace PPDCoreModel
{
    public interface IProcessAllowedButtons : IEvaluate
    {
        void Process(IMarkInfo markInfo);
        ButtonType[] ProcessAllowedButtons { get; }
    }
}
