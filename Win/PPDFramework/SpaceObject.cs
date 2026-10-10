namespace PPDFramework
{
    /// <summary>
    /// スペースを空けるオブジェクトです。
    /// </summary>
    /// <remarks>
    /// コンストラクターです。
    /// </remarks>
    /// <param name="device"></param>
    /// <param name="width">幅。</param>
    /// <param name="height">高さ。</param>
    public class SpaceObject(PPDDevice device, float width, float height) : SpriteObject(device)
    {
        readonly float width = width;
        readonly float height = height;

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
    }
}
