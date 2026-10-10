using System.Numerics;
using PPDMath;
using Point = Vortice.Mathematics.Int2;
using Size2 = Vortice.Mathematics.SizeI;

namespace PPDFramework.Chars
{
    class AvailableSpace(SizeTexture sizeTexture, Point position, Size2 size) : ReturnableComponent
    {
        readonly SizeTexture sizeTexture = sizeTexture;

        public Point Position
        {
            get;
            private set;
        } = position;

        public Size2 Size
        {
            get;
            private set;
        } = size;

        protected override void DisposeResource()
        {
            base.DisposeResource();
            sizeTexture.Return(this);
        }
    }
}
