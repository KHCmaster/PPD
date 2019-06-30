namespace PPDFramework
{
    /// <summary>
    /// ゲーム情報クラスです
    /// </summary>
    public abstract class GameInformationBase
    {
        /// <summary>
        /// ゲーム名です
        /// </summary>
        public abstract string GameName
        {
            get;
        }

        /// <summary>
        /// ゲームの説明です
        /// </summary>
        public abstract string GameDescription
        {
            get;
        }

        /// <summary>
        /// ゲームのアイコンのパスです
        /// </summary>
        public abstract string GameIconPath
        {
            get;
        }
    }
}
