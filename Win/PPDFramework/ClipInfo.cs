namespace PPDFramework
{
    /// <summary>
    /// クリップの情報です
    /// </summary>
    /// <remarks>
    /// コンストラクタです。
    /// </remarks>
    /// <param name="gameHost">ゲームホストです。</param>
    public class ClipInfo(IGameHost gameHost)
    {
        /// <summary>
        /// ゲームホストです。
        /// </summary>
        public IGameHost GameHost
        {
            get;
            private set;
        } = gameHost;

        /// <summary>
        /// X座標の位置です。
        /// </summary>
        public int PositionX
        {
            get;
            set;
        }

        /// <summary>
        /// Y座標の位置です。
        /// </summary>
        public int PositionY
        {
            get;
            set;
        }

        /// <summary>
        /// 幅です。
        /// </summary>
        public int Width
        {
            get;
            set;
        }

        /// <summary>
        /// 高さです。
        /// </summary>
        public int Height
        {
            get;
            set;
        }
    }
}
