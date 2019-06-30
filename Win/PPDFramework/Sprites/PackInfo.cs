using SharpDX;

namespace PPDFramework.Sprites
{
    class PackInfo
    {
        public ImageInfo ImageInfo
        {
            get;
            private set;
        }

        public Vector2 Position
        {
            get;
            private set;
        }

        public PackInfo(ImageInfo imageInfo, Vector2 position)
        {
            ImageInfo = imageInfo;
            Position = position;
        }
    }
}
