using PPDFramework;

namespace PPDCore
{
    /// <summary>
    /// 速度つき画像表示クラス
    /// </summary>
    class OnpuObject : PictureObject
    {
        public OnpuObject(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, PathObject filename, float x, float y, bool center)
            : base(device, resourceManager, filename, center)
        {
            Position = new SharpDX.Vector2(x, y);
        }

        public float VelX
        {
            get;
            set;
        }
    }
}
