using SharpDX;

namespace PPDFramework.Chars
{
    class AvailableSpace : ReturnableComponent
    {
        SizeTexture sizeTexture;

        public Point Position
        {
            get;
            private set;
        }

        public Size2 Size
        {
            get;
            private set;
        }

        public AvailableSpace(SizeTexture sizeTexture, Point position, Size2 size)
        {
            this.sizeTexture = sizeTexture;
            Position = position;
            Size = size;
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
            sizeTexture.Return(this);
        }
    }
}
