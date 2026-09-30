using SharpDX;
using System.Numerics;
using PPDMath;

namespace PPDEditorCommon
{
    public interface IPosAndAngle
    {
        Vector2? Position { get; }
        float? Rotation { get; }
    }
}
