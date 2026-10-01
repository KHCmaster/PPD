namespace PPDFramework.Logger
{
    /// <summary>
    /// ログの情報です。
    /// </summary>
    /// <remarks>
    /// コンストラクターです。
    /// </remarks>
    /// <param name="logLevel">ログレベルです。</param>
    /// <param name="message">メッセージです。</param>
    public class LogInfo(LogLevel logLevel, string message)
    {
        /// <summary>
        /// ログレベルを取得します。
        /// </summary>
        public LogLevel LogLevel
        {
            get;
            private set;
        } = logLevel;

        /// <summary>
        /// メッセージを取得します。
        /// </summary>
        public string Message
        {
            get;
            private set;
        } = message;
    }
}
