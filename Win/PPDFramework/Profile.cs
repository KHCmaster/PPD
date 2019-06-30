namespace PPDFramework
{
    /// <summary>
    /// プロファイルクラス
    /// </summary>
    public class Profile
    {
        internal Profile()
        {
        }

        /// <summary>
        /// 表示テキスト
        /// </summary>
        public string DisplayText
        {
            get;
            internal set;
        }

        /// <summary>
        /// クール時のポイント
        /// </summary>
        public int CoolPoint
        {
            get;
            internal set;
        }

        /// <summary>
        /// グッド時のポイント
        /// </summary>
        public int GoodPoint
        {
            get;
            internal set;
        }

        /// <summary>
        /// セーフ時のポイント
        /// </summary>
        public int SafePoint
        {
            get;
            internal set;
        }

        /// <summary>
        /// サッド時のポイント
        /// </summary>
        public int SadPoint
        {
            get;
            internal set;
        }

        /// <summary>
        /// ワースト時のポイント
        /// </summary>
        public int WorstPoint
        {
            get;
            internal set;
        }

        /// <summary>
        /// ゴッドモードかどうか
        /// </summary>
        public bool GodMode
        {
            get;
            internal set;
        }

        /// <summary>
        /// インデックス
        /// </summary>
        public int Index
        {
            get;
            internal set;
        }

        /// <summary>
        /// 効果音無音かどうか
        /// </summary>
        public bool MuteSE
        {
            get;
            internal set;
        }

        /// <summary>
        /// 同時押しマークをつなぐか
        /// </summary>
        public bool Connect
        {
            get;
            internal set;
        }
    }
}
