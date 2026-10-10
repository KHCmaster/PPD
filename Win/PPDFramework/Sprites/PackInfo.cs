using System.Numerics;
using PPDMath;

namespace PPDFramework.Sprites
{
    class PackInfo(ImageInfo imageInfo, Vector2 position)
    {
        public ImageInfo ImageInfo
        {
            get;
            private set;
        } = imageInfo;

        public Vector2 Position
        {
            get;
            private set;
        } = position;
    }
}
