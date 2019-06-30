using SharpDX;

namespace PPDEditorCommon
{
    public interface IPosAndAngle
    {
        Vector2? Position { get; }
        float? Rotation { get; }
    }
}
