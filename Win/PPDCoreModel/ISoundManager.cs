using PPDCoreModel.Data;

namespace PPDCoreModel
{
    public interface ISoundManager
    {
        bool Play(MarkType button, double playRatio);
        bool Play(MarkType button, int volume, double playRatio);
        bool Play(int index, int volume, double playRatio);
        bool Stop(MarkType button);
        bool Stop(int index);
    }
}
