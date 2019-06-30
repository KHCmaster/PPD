using PPDCoreModel.Data;

namespace PPDCoreModel
{
    public interface IProcessMissPress : IEvaluate
    {
        void Process(IMarkInfo markInfo, MarkType pressedButton);
        bool ProcessMissPress { get; }
    }
}
