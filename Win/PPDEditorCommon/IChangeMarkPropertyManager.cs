using PPDCoreModel.Data;
using SharpDX;

namespace PPDEditorCommon
{
    public interface IChangeMarkPropertyManager
    {
        void ChangeMarkAngle(IEditorMarkInfo mark, float angle);
        void ChangeMarkPosition(IEditorMarkInfo mark, Vector2 position);
        void ChangeMarkType(IEditorMarkInfo mark, MarkType markType);
        void ChangeMarkTime(IEditorMarkInfo mark, float time);
        void ChangeParameter(IEditorMarkInfo mark, string key, string value);
        void RemoveParameter(IEditorMarkInfo mark, string key);
        void AddMark(Vector2 position, float angle, float time, MarkType markType);
        void AddExMark(Vector2 position, float angle, float time, float endTime, MarkType markType);
        void AssignID(IEditorMarkInfo mark);
        void UnassignID(IEditorMarkInfo mark);
        void Remove(IEditorMarkInfo mark);
    }
}
