namespace PPDFramework.Logger
{
    /// <summary>
    /// ログの情報です。
    /// </summary>
    public class LogInfo
    {
        /// <summary>
        /// ログレベルを取得します。
        /// </summary>
        public LogLevel LogLevel
        {
            get;
            private set;
        }

        /// <summary>
        /// メッセージを取得します。
        /// </summary>
        public string Message
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクターです。
        /// </summary>
        /// <param name="logLevel">ログレベルです。</param>
        /// <param name="message">メッセージです。</param>
        public LogInfo(LogLevel logLevel, string message)
        {
            LogLevel = logLevel;
            Message = message;
        }
    }
}
