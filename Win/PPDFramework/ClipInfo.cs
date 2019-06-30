namespace PPDFramework
{
    /// <summary>
    /// クリップの情報です
    /// </summary>
    public class ClipInfo
    {
        /// <summary>
        /// ゲームホストです。
        /// </summary>
        public IGameHost GameHost
        {
            get;
            private set;
        }

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

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="gameHost">ゲームホストです。</param>
        public ClipInfo(IGameHost gameHost)
        {
            GameHost = gameHost;
        }
    }
}
