using PPDCoreModel.Data;
using PPDFramework;
using PPDFramework.PPDStructure.PPDData;
using SharpDX;

namespace PPDCoreModel
{
    public interface IMarkManager
    {
        void AddMark(Vector2 position, float angle, float time, MarkType markType, int id);
        void AddLongMark(Vector2 position, float angle, float time, float endTime, MarkType markType, int id);

        int AllMarkCount { get; }
        int AllLongMarkCount { get; }
        ScoreDifficultyMeasureResult ScoreDifficultyMeasureResult { get; }
        MarkDataBase[] Marks { get; }
        int GetMarkCount(int sameNum);
    }
}
