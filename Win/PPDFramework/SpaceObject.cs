namespace PPDFramework
{
    /// <summary>
    /// スペースを空けるオブジェクトです。
    /// </summary>
    public class SpaceObject : SpriteObject
    {
        float width;
        float height;

        /// <summary>
        /// 幅を取得します。
        /// </summary>
        public override float Width
        {
            get
            {
                return width;
            }
        }

        /// <summary>
        /// 高さを取得します。
        /// </summary>
        public override float Height
        {
            get
            {
                return height;
            }
        }

        /// <summary>
        /// コンストラクターです。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="width">幅。</param>
        /// <param name="height">高さ。</param>
        public SpaceObject(PPDDevice device, float width, float height) : base(device)
        {
            this.width = width;
            this.height = height;
        }
    }
}
